using RomsBrowse.Common.Services;
using RomsBrowse.Data.Models;

namespace RomsBrowse.Web.ViewModels
{
    public class SaveDataViewModel(SaveData state, ICompressionService compressor)
    {
        public byte[] Screenshot { get; } = state.Image;

        public byte[] Data { get; } = compressor.Decompress(state.Data);

        public DateTime Created { get; } = state.Created;

        public int GameId { get; } = state.RomFileId;

        public string GameName { get; } = state.RomFile?.DisplayName ?? "<unknown>";

        public string Platform { get; } = state.RomFile?.Platform?.DisplayName ?? "<unknown>";

        public string SaveGameName { get; } = Path.ChangeExtension(state.RomFile?.FileName ?? "unknown", ".state");

        public string GetImageBase64Url()
        {
            return "data:image/png;base64," + Convert.ToBase64String(Screenshot);
        }
        public string GetDataBase64Url()
        {
            return "data:application/octet-stream;base64," + Convert.ToBase64String(Data);
        }
    }
}
