using AyrA.AutoDI;

namespace RomsBrowse.Web.Services
{
    [AutoDIHostedService, AutoDIRegister(AutoDIType.Singleton)]
    public class EmulatorCachingService(IHostEnvironment env, ILogger<EmulatorCachingService> logger, ChainDownloadService downloader) : IHostedService
    {
        private static bool hasEmulator;
        private static bool isDownloading;

        public bool HasEmulator => hasEmulator;

        public bool IsDownloading => isDownloading;

        public async Task EnsureEmulatorExists()
        {
            if (isDownloading || hasEmulator)
            {
                return;
            }

            isDownloading = true;
            try
            {
                var root = Path.Combine(env.ContentRootPath, "wwwroot");
                if (!Directory.Exists(root))
                {
                    logger.LogCritical("wwwroot cannot be found at {Dir}. Skipping download", root);
                    return;
                }
                if (!Directory.Exists(Path.Combine(root, "emulator")))
                {
                    logger.LogInformation("Cannot find emulator. Downloading a copy now");
                    await downloader.Download(root, new Uri("https://files.ayra.ch/deliver/emulator"));
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
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _ = EnsureEmulatorExists();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // N/A
            return Task.CompletedTask;
        }
    }
}
