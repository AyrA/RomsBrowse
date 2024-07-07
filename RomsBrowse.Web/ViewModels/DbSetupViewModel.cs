using RomsBrowse.Common.Interfaces;
using RomsBrowse.Common.Validation;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using VE = RomsBrowse.Common.Validation.ValidationException;

namespace RomsBrowse.Web.ViewModels
{
    public enum DbProvider
    {
        None = 0,
        SQLServer = 1,
        SQLite = 2
    }

    public class DbSetupViewModel : IValidateable, ISensitiveData
    {
        [Required]
        public DbProvider Provider { get; set; }

        [Required]
        public string? ServerInstance { get; set; }

        [Required]
        public string? FileName { get; set; }

        [Required]
        public string? DatabaseName { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public bool UseWindowsAuth { get; set; }

        public bool Encrypt { get; set; }

        public string CurrentUser { get; } =
            OperatingSystem.IsWindows()
            ? string.Join('\\', Environment.UserDomainName, Environment.UserName)
            : Environment.UserName;

        public string ProviderString
        {
            get
            {
                return Provider switch
                {
                    DbProvider.SQLite => "sqlite",
                    DbProvider.SQLServer => "mssql",
                    _ => throw new NotImplementedException($"Unknown provider type: {Provider}"),
                };
            }
            set
            {
                Provider = value switch
                {
                    "mssql" => DbProvider.SQLServer,
                    "sqlite" => DbProvider.SQLite,
                    _ => throw new ArgumentException($"Invalid provider value: {value}", nameof(value)),
                };
            }
        }

        public string? DefaultDirectory { get; set; }

        public string GetConnectionString() => GetConnectionString(true);

        public void ClearSensitiveData()
        {
            Password = null;
        }

        public void Validate()
        {
            if (Provider == DbProvider.SQLite)
            {
                ValidationTools.ValidateFields(this, nameof(FileName));
                try
                {
                    if (string.IsNullOrEmpty(FileName))
                    {
                        throw new Exception("Provider cannot be used without a file name");
                    }
                }
                catch (VE)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new VE(nameof(FileName), $"'{FileName}' is not a valid database file name", ex);
                }
            }
            else if (Provider == DbProvider.SQLServer)
            {
                ValidationTools.ValidateFields(this, nameof(ServerInstance), nameof(DatabaseName));
                if (string.IsNullOrWhiteSpace(DatabaseName))
                {
                    throw new VE(nameof(DatabaseName), "Database name must be set");
                }
                if (UseWindowsAuth)
                {
                    if (!string.IsNullOrWhiteSpace(Username))
                    {
                        throw new VE(nameof(Username), "Cannot use windows authentication and use a username at the same time");
                    }
                }
                else if (string.IsNullOrWhiteSpace(Username))
                {
                    throw new VE(nameof(Username), "Username is required if windows authentication is disabled");
                }
                else if (string.IsNullOrWhiteSpace(Password))
                {
                    throw new VE(nameof(Password), "If a username is specified, a password is required");
                }
                try
                {
                    GetConnectionString(false);
                }
                catch (Exception ex)
                {
                    throw new VE("Multiple", "Unable to construct connection string. Some value may need escaping or is invalid for an SQL server", ex);
                }
            }
            else
            {
                throw new NotImplementedException($"Provider not implemented: {Provider}");
            }
        }

        /// <summary>
        /// Verifies that the file name is valid,
        /// and the file can be created,
        /// written to,
        /// and deleted.
        /// </summary>
        /// <param name="dataDirectory">Data directory</param>
        /// <remarks>
        /// Internally calls <see cref="Validate"/><br />
        /// Will do nothing if <see cref="Provider" /> is not set to use SQLite.
        /// This will set the full path in <see cref="FileName"/>
        /// </remarks>
        public void VerifySqliteFileName(string dataDirectory)
        {
            if (Provider != DbProvider.SQLite)
            {
                return;
            }

            Validate();
            try
            {
                var p = Path.Combine(dataDirectory, FileName!);
                if (string.IsNullOrEmpty(p))
                {
                    throw new IOException("Failed to combine path strings");
                }
                if (File.Exists(p))
                {
                    throw new Exception($"A file named '{p}' already exists");
                }
                //Ensure we can create the file
                try
                {
                    File.Create(p).Dispose();
                }
                catch (Exception ex)
                {
                    throw new VE(nameof(FileName), $"Unable to create file '{p}'.", ex);
                }
                //Ensure we can write to the file
                try
                {
                    using var f = File.OpenWrite(p);
                    var bytes = Enumerable.Range(0, 512).Select(m => (byte)m).ToArray();
                    f.Write(bytes, 0, 0);
                }
                catch (Exception ex)
                {
                    throw new VE(nameof(FileName), $"Able to create file '{p}' but cannot write to it later anymore.", ex);
                }
                try
                {
                    File.Delete(p);
                }
                catch (Exception ex)
                {
                    throw new VE(nameof(FileName), $"Able to create file '{p}' but cannot delete it anymore.", ex);
                }
                FileName = p;
                GetConnectionString(false);
            }
            catch (VE)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new VE(nameof(FileName), $"'{FileName}' is not a valid database file name", ex);
            }
        }

        private string GetConnectionString(bool validate)
        {
            if (validate)
            {
                Validate();
            }

            if (Provider == DbProvider.SQLite)
            {
                var builder = new DbConnectionStringBuilder()
                {
                    { "Data Source", FileName! }
                };
                return builder.ToString();
            }
            else if (Provider == DbProvider.SQLServer)
            {
                var builder = new DbConnectionStringBuilder
                {
                    { "Server", ServerInstance! },
                    { "Database", DatabaseName! }
                };

                if (UseWindowsAuth)
                {
                    builder.Add("Trusted_Connection", "True");
                }
                else
                {
                    builder.Add($"User Id", Username!);
                    builder.Add($"Password", Password!);
                }
                builder.Add("Encrypt", Encrypt ? "True" : "False");

                return builder.ToString();
            }
            else
            {
                throw new NotImplementedException($"Unknown provider: {Provider}");
            }
        }
    }
}