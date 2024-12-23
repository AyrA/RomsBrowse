﻿using AyrA.AutoDI;
using RomsBrowse.Common.Services;
using System.Text;
using System.Text.Json;

namespace RomsBrowse.Data.Services
{
    [AutoDIRegister(AutoDIType.Singleton)]
    public class DbContextSettingsProvider
    {
        public record SettingsContents(string ConnectionString, string DbProvider);

        private readonly string connStrFile;
        private readonly IPermEncryptionService _encService;
        private SettingsContents? settings;

        public string DataDirectry => Path.GetDirectoryName(connStrFile)
            ?? throw new InvalidOperationException("Connection string file has not been set up properly");

        public bool IsConnectionStringSet => File.Exists(connStrFile);

        public DbContextSettingsProvider(IPermEncryptionService encryptionService)
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

        public void ResetSettings()
        {
            if (File.Exists(connStrFile))
            {
                File.Delete(connStrFile);
            }
            settings = null;
        }

        public SettingsContents GetSettings()
        {
            if (settings == null)
            {
                var json = Encoding.UTF8.GetString(_encService.Decrypt(File.ReadAllBytes(connStrFile)));
                var data = JsonSerializer.Deserialize<SettingsContents>(json)
                    ?? throw new InvalidOperationException("Cannot deserialize settings");
                settings = data;
            }
            return settings;
        }

        public SettingsContents SetSettings(string connectionString, string provider)
        {
            ArgumentException.ThrowIfNullOrEmpty(connectionString);
            ArgumentException.ThrowIfNullOrEmpty(provider);

            var data = new SettingsContents(connectionString, provider);
            var json = JsonSerializer.Serialize(data);

            File.WriteAllBytes(connStrFile, _encService.Encrypt(Encoding.UTF8.GetBytes(json)));
            return settings = data;
        }
    }
}
