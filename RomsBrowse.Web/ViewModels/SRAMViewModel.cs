using RomsBrowse.Common.Services;
using RomsBrowse.Data.Models;

namespace RomsBrowse.Web.ViewModels
{
    public class SRAMViewModel(SRAM sram, ICompressionService compressor)
    {
        public byte[] Data { get; } = compressor.Decompress(sram.Data);

        public byte[] Image { get; } = sram.Image;

        public DateTime Created { get; } = sram.Created;

        public int GameId { get; } = sram.RomFileId;

        public string GameName { get; } = sram.RomFile?.DisplayName ?? "<unknown>";

        public string Platform { get; } = sram.RomFile?.Platform?.DisplayName ?? "<unknown>";

        public string SaveGameName { get; } = Path.ChangeExtension(sram.RomFile?.FileName ?? "unknown", ".srm");

        public string GetDataBase64Url()
        {
            return "data:application/octet-stream;base64," + Convert.ToBase64String(Data);
        }

        public string GetImageBase64Url()
        {
            return "data:image/png;base64," + Convert.ToBase64String(Image);
        }
    }
}
