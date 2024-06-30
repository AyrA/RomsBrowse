using AyrA.AutoDI;
using System.Security.Cryptography;

namespace RomsBrowse.Common.Services
{
    [AutoDIRegister(AutoDIType.Transient, typeof(ITempEncryptionService))]
    public class TempEncryptionService : ITempEncryptionService
    {
        private static readonly byte[] key = RandomNumberGenerator.GetBytes(32);
        private static readonly int tagSize = AesGcm.TagByteSizes.MaxSize;
        private static readonly int nonceSize = AesGcm.NonceByteSizes.MaxSize;

        public byte[] Encrypt(byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data);
            using var enc = new AesGcm(key, tagSize);
            var nonce = RandomNumberGenerator.GetBytes(nonceSize);
            var ciphertext = new byte[data.Length];
            var tag = new byte[tagSize];
            enc.Encrypt(nonce, data, ciphertext, tag);
            return [.. nonce, .. ciphertext, .. tag];
        }

        public byte[] Decrypt(byte[] data)
        {
            ArgumentNullException.ThrowIfNull(data);
            var nonce = data.Take(nonceSize).ToArray();
            var tag = data.TakeLast(tagSize).ToArray();
            var ciphertext = data.Skip(nonceSize).SkipLast(tagSize).ToArray();
            var plaintext = new byte[ciphertext.Length];
            using var dec = new AesGcm(key, tagSize);
            dec.Decrypt(nonce, ciphertext, tag, plaintext);
            return plaintext;
        }
    }
}
