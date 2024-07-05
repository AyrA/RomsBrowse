using AyrA.AutoDI;
using RomsBrowse.Common.Services;
using System.Text;

namespace RomsBrowse.Data.Services
{
    [AutoDIRegister(AutoDIType.Singleton)]
    public class ConnectionStringProvider
    {
        private readonly string connStrFile;
        private readonly IPermEncryptionService _encService;
        private string? connStr;

        public bool IsSet => File.Exists(connStrFile);

        public ConnectionStringProvider(IPermEncryptionService encryptionService)
        {
            string? baseDir = null;
            if (OperatingSystem.IsWindows())
            {
                var folders = new Environment.SpecialFolder[]
                {
                    Environment.SpecialFolder.LocalApplicationData,
                    Environment.SpecialFolder.ApplicationData,
                    Environment.SpecialFolder.UserProfile
                };
                foreach (var folder in folders)
                {
                    baseDir = Environment.GetFolderPath(folder);
                    if (!string.IsNullOrEmpty(baseDir))
                    {
                        break;
                    }
                }
                if (string.IsNullOrEmpty(baseDir))
                {
                    throw new PlatformNotSupportedException("Unable to determine location to store application data in");
                }
            }
            else
            {
                baseDir = Directory.Exists("/home") ? "/home" : "";
            }
            connStrFile = Path.Combine(Directory.CreateDirectory(Path.Combine(baseDir, "AyrA", "RomsBrowse")).FullName, "sql.bin");
            _encService = encryptionService;
        }

        public void ResetConnectionString()
        {
            if (File.Exists(connStrFile))
            {
                File.Delete(connStrFile);
            }
            connStr = null;
        }

        public string GetConnectionString()
        {
            connStr ??= Encoding.UTF8.GetString(_encService.Decrypt(File.ReadAllBytes(connStrFile)));
            return connStr;
        }

        public string SetConnectionString(string connectionString)
        {
            ArgumentException.ThrowIfNullOrEmpty(connectionString);
            File.WriteAllBytes(connStrFile, _encService.Encrypt(Encoding.UTF8.GetBytes(connectionString)));
            return connStr = connectionString;
        }
    }
}
