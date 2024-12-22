using AyrA.AutoDI;
using System.Net;
using System.Text;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Transient)]
    public class ChainDownloadService(ILogger<ChainDownloadService> logger)
    {
        public static readonly string Magic = "CHAIN";
        public static readonly ushort SupportedVersion = 1;

        public async Task Download(string target, Uri source)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(target);
            ArgumentNullException.ThrowIfNull(source);

            if (!Directory.Exists(target))
            {
                logger.LogError("Directory {Dir} does not exist", target);
                throw new DirectoryNotFoundException($"Directory does not exist: {target}");
            }

            using var cli = new HttpClient(new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });
            var res = await cli.GetAsync(source, HttpCompletionOption.ResponseHeadersRead);
            try
            {
                res.EnsureSuccessStatusCode();
            }
            catch
            {
                logger.LogError("Download of {Url} failed. Expected 'Ok' status but got {Status}",
                    source, res.StatusCode);
                throw;
            }
            using var data = res.Content.ReadAsStream();
            using var br = new BinaryReader(data);
            var compare = Encoding.UTF8.GetBytes(Magic);

            //Read header
            if (!br.ReadBytes(compare.Length).SequenceEqual(compare))
            {
                throw new InvalidDataException($"Chain data does not start with {Magic}");
            }
            if (Read16BitNumber(br) != SupportedVersion)
            {
                throw new NotSupportedException($"Version of chain data is not {SupportedVersion}");
            }
            //Read entries
            ChainEntry? entry;
            do
            {
                entry = ReadEntry(br);
                if (entry != null)
                {
                    logger.LogInformation("Processing {Entry}", entry);
                    entry.WriteTo(target);
                }
                else
                {
                    logger.LogInformation("No more entries");
                }
            } while (entry != null);
        }

        private static ushort Read16BitNumber(BinaryReader br)
        {
            return (ushort)IPAddress.NetworkToHostOrder(br.ReadInt16());
        }

        private static uint Read32BitNumber(BinaryReader br)
        {
            return (uint)IPAddress.NetworkToHostOrder(br.ReadInt32());
        }

        private static ChainEntry? ReadEntry(BinaryReader br)
        {
            var nameSize = Read16BitNumber(br);
            if (nameSize == 0)
            {
                return null;
            }
            var fileName = Encoding.UTF8.GetString(br.ReadBytes(nameSize))
                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            var lmod = DateTime.UnixEpoch.AddSeconds(Read32BitNumber(br));
            byte[]? data = null;
            if (!fileName.EndsWith(Path.DirectorySeparatorChar))
            {
                data = br.ReadBytes((int)Read32BitNumber(br));
            }
            return new ChainEntry(fileName.TrimEnd(Path.DirectorySeparatorChar), data == null, lmod, data);
        }

        private class ChainEntry(string name, bool isDirectory, DateTime lastModified, byte[]? data)
        {
            public string Name { get; } = name;
            public bool IsDirectory { get; } = isDirectory;
            public DateTime LastModified { get; } = lastModified;
            public byte[]? Data { get; } = data;

            public void WriteTo(string target)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(target);
                var finalName = Path.Combine(target, Name);
                if (IsDirectory)
                {
                    Directory.CreateDirectory(finalName).LastWriteTimeUtc = LastModified;
                }
                else
                {
                    File.WriteAllBytes(finalName, Data ?? []);
                    File.SetLastWriteTimeUtc(finalName, LastModified);
                }
            }

            public override string ToString()
            {
                return string.Format("{0} entry: {1} ({2} bytes)", IsDirectory ? "Directory" : "File", Name, Data?.Length ?? 0);
            }
        }
    }
}
