using RomsBrowse.Common.Interfaces;
using RomsBrowse.Common.Validation;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using VE = RomsBrowse.Common.Validation.ValidationException;

namespace RomsBrowse.Web.ViewModels
{
    public enum DbProvider
    {
        None = 0,
        SQLite = 1,
        SQLServer = 2
    }

    public class DbSetupViewModel : IValidateable, ISensitiveData
    {
        [Required]
        public DbProvider Provider { get; set; }

        [Required]
        public string? ServerInstance { get; set; }

        public string? DatabaseName { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public bool UseWindowsAuth { get; set; }

        public bool Encrypt { get; set; }

        public string CurrentUser { get; } =
            OperatingSystem.IsWindows()
            ? string.Join('\\', Environment.UserDomainName, Environment.UserName)
            : Environment.UserName;

        public string GetConnectionString()
        {
            Validate();
            var builder = new DbConnectionStringBuilder
            {
                { "Server", ServerInstance },
                { "Database", DatabaseName }
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

        public void ClearSensitiveData()
        {
            Password = null;
        }

#pragma warning disable CS8774
        [MemberNotNull(nameof(DatabaseName), nameof(ServerInstance), nameof(Provider))]
        public void Validate()
        {
            ValidationTools.ValidatePublic(this);
            if (Provider == DbProvider.SQLite)
            {
                try
                {
                    if (string.IsNullOrEmpty(ServerInstance))
                    {
                        throw new Exception("Provider cannot be used without a file name");
                    }
                    var p = Path.Combine(AppContext.BaseDirectory, ServerInstance);
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
                        throw new VE(nameof(ServerInstance), $"Unable to create file '{p}'.", ex);
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
                        throw new VE(nameof(ServerInstance), $"Able to create file '{p}' but cannot write to it later anymore.", ex);
                    }
                    try
                    {
                        File.Delete(p);
                    }
                    catch (Exception ex)
                    {
                        throw new VE(nameof(ServerInstance), $"Able to create file '{p}' but cannot delete it anymore.", ex);
                    }
                }
                catch (VE)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new VE(nameof(ServerInstance), $"'{ServerInstance}' is not a valid database file name", ex);
                }
            }
            else if (Provider == DbProvider.SQLServer)
            {
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
            }
            else
            {
                throw new NotImplementedException($"Provider not implemented: {Provider}");
            }
        }
#pragma warning restore CS8774
    }
}
