namespace RomsBrowse.Common.Services
{
    public interface ITempEncryptionService
    {
        byte[] Decrypt(byte[] data);
        byte[] Encrypt(byte[] data);
    }
}