using AyrA.AutoDI;
using System.Security.Cryptography;
using System.Text;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Singleton)]
    public class PasswordService
    {
        private record VersionInfo(int Iterations, HashAlgorithmName HashAlgorithm, int SaltLength, int DataSize);
        //Always leave a few cores unused if possible to prevent DoS of the entire service
        private readonly SemaphoreSlim _lock = new(Math.Min(1, Environment.ProcessorCount - 2));

        private const int CurrentVersion = 1;
        private readonly Dictionary<int, VersionInfo> versions = new()
        {
            { CurrentVersion, new(500_000, HashAlgorithmName.SHA256, 16, 32) }
        };

        private VersionInfo Current => versions[CurrentVersion];

        public string HashPassword(string password)
        {
            ArgumentException.ThrowIfNullOrEmpty(password);
            var salt = RandomNumberGenerator.GetBytes(Current.SaltLength);
            var data = Encoding.UTF8.GetBytes(password);
            byte[] hash;
            lock (_lock)
            {
                hash = Rfc2898DeriveBytes.Pbkdf2(data, salt, Current.Iterations, Current.HashAlgorithm, Current.DataSize);
            }
            return string.Join("$", CurrentVersion, B64(salt), B64(hash));
        }

        public bool CheckPassword(string password, string existingHash, out bool needsUpdate)
        {
            ArgumentException.ThrowIfNullOrEmpty(password);
            ArgumentException.ThrowIfNullOrEmpty(existingHash);

            var parts = existingHash.Split('$');
            if (parts.Length != 3)
            {
                throw new ArgumentException("Invalid hash format");
            }
            if (!int.TryParse(parts[0], out var version))
            {
                throw new ArgumentException("Invalid hash format");
            }
            if (!versions.TryGetValue(version, out var info))
            {
                throw new ArgumentException($"Unknown password version: {version}");
            }
            byte[] hash;
            lock (_lock)
            {
                hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), B64(parts[1]), info.Iterations, info.HashAlgorithm, info.DataSize);
            }
            needsUpdate = version != CurrentVersion;
            return B64(hash) == parts[2];
        }

        private static byte[] B64(string str)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(str);

            var extra = (int)Math.Ceiling(str.Length / 4.0) * 4;
            return Convert.FromBase64String(str
                .Replace('-', '+')
                .Replace('_', '/')
                .PadRight(extra, '='));
        }

        private static string B64(byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data);

            return Convert.ToBase64String(data)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }
    }
}
