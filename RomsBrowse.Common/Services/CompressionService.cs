using AyrA.AutoDI;
using System.IO.Compression;

namespace RomsBrowse.Common.Services
{
    [AutoDIRegister(AutoDIType.Transient, typeof(ICompressionService))]
    public class CompressionService : ICompressionService
    {
        public byte[] Compress(byte[] data)
        {
            if (data.Length == 0)
            {
                return [];
            }
            using var msOUT = new MemoryStream();
            using (var gz = new GZipStream(msOUT, CompressionLevel.SmallestSize, true))
            {
                gz.Write(data, 0, data.Length);
            }
            return msOUT.ToArray();
        }

        public byte[] Decompress(byte[] data)
        {
            if (data.Length == 0)
            {
                return [];
            }
            using var msIN = new MemoryStream(data, false);
            using var msOUT = new MemoryStream();
            using var gz = new GZipStream(msIN, CompressionMode.Decompress);
            gz.CopyTo(msOUT);
            return msOUT.ToArray();
        }
    }
}
