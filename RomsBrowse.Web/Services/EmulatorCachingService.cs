using AyrA.AutoDI;

namespace RomsBrowse.Web.Services
{
    [AutoDIHostedSingletonService]
    public class EmulatorCachingService(IHostEnvironment env, ILogger<EmulatorCachingService> logger, ChainDownloadService downloader) : IHostedService
    {
        private static bool hasEmulator;
        private static bool isDownloading;
        private static readonly SemaphoreSlim semaphoreSlim = new(1);

        public bool HasEmulator => hasEmulator;

        public bool IsDownloading => isDownloading;

        public async Task<bool> EnsureEmulatorExists(CancellationToken ct)
        {
            //If a progress is already ongoing,
            //don't block the next call
            if (!semaphoreSlim.Wait(0))
            {
                return false;
            }
            try
            {
                if (isDownloading || hasEmulator)
                {
                    return false;
                }

                isDownloading = true;
                var root = Path.Combine(env.ContentRootPath, "wwwroot");
                if (!Directory.Exists(root))
                {
                    logger.LogCritical("wwwroot cannot be found at {Dir}. Skipping download", root);
                    return false;
                }
                //This file is written last. If it exists, the emulator was completely written to disk,
                //otherwise we download a fresh copy
                var checkFile = Path.Combine(root, "emulator", "complete.txt");
                if (!File.Exists(checkFile))
                {
                    logger.LogInformation("Cannot find emulator. Downloading a copy now");
                    Directory.CreateDirectory(Path.GetDirectoryName(checkFile)!);
                    await downloader.Download(root, new Uri("https://files.ayra.ch/deliver/emulator"), ct);
                    File.WriteAllText(checkFile, "Completed at " + DateTime.UtcNow.ToString("R"));
                }
                else
                {
                    logger.LogInformation("Emulator exists already. Skipping download");
                }
                hasEmulator = true;

            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Emulator download failed");
            }
            finally
            {
                isDownloading = false;
                semaphoreSlim.Release();
            }
            return true;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = EnsureEmulatorExists(cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // N/A
            return Task.CompletedTask;
        }
    }
}
