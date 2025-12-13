using AyrA.AutoDI;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace RomsBrowse.Common.Services;

[AutoDIRegister(nameof(Register))]
public class PermEncryptionService(IDataProtectionProvider provider) : IPermEncryptionService
{
    private readonly IDataProtector protector = provider.CreateProtector("RomsBrowse");

    public byte[] Encrypt(byte[] data) => protector.Protect(data);

    public byte[] Decrypt(byte[] data) => protector.Unprotect(data);

    internal static void Register(IServiceCollection services)
    {
        services.AddTransient<IPermEncryptionService, PermEncryptionService>();
        services.AddDataProtection();
    }
}
