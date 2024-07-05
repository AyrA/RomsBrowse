namespace RomsBrowse.Common.Services
{
    public interface IEncryptionService
    {
        byte[] Decrypt(byte[] data);
        byte[] Encrypt(byte[] data);
    }
}