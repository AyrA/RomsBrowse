namespace RomsBrowse.Common.Services
{
    public interface ICompressionService
    {
        byte[] Compress(byte[] data);
        byte[] Decompress(byte[] data);
    }
}