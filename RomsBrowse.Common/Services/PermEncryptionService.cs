using AyrA.AutoDI;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;

namespace RomsBrowse.Common.Services
{
    [AutoDIRegister(nameof(Register))]
    public class PermEncryptionService(IDataProtectionProvider provider) : IPermEncryptionService
    {
        private readonly IDataProtector protector = provider.CreateProtector("RomsBrowse");
        private static readonly int tagSize = AesGcm.TagByteSizes.MaxSize;
        private static readonly int nonceSize = AesGcm.NonceByteSizes.MaxSize;

        public byte[] Encrypt(byte[] data) => protector.Protect(data);

        public byte[] Decrypt(byte[] data) => protector.Unprotect(data);

        internal static void Register(IServiceCollection services)
        {
            services.AddTransient<IPermEncryptionService, PermEncryptionService>();
            services.AddDataProtection();
        }
    }
}
