using AyrA.AutoDI;
using RomsBrowse.Data;

namespace RomsBrowse.Web.Services
{
    [AutoDIHostedService]
    public class PeriodicMaintenanceService(IServiceProvider provider, ILogger<PeriodicMaintenanceService> logger) : IHostedService, IDisposable
    {
        private Timer? _timer;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Periodic maintenance service has been started");
            _timer?.Dispose();
            _timer = new Timer(Callback, null, TimeSpan.FromSeconds(1), TimeSpan.FromHours(1));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Periodic maintenance service has been stopped");
            _timer?.Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _timer?.Dispose();
        }

        private void Callback(object? state)
        {
            logger.LogInformation("Starting periodic maintenance");
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
            if (db.IsConfigured)
            {
                var ss = scope.ServiceProvider.GetRequiredService<SettingsService>();
                var rgs = scope.ServiceProvider.GetRequiredService<RomGatherService>();

                var userAge = ss.GetValue<TimeSpan>(SettingsService.KnownSettings.UserExpiration);
                var ssAge = ss.GetValue<TimeSpan>(SettingsService.KnownSettings.SaveStateExpiration);

                try
                {
                    scope.ServiceProvider.GetRequiredService<SaveService>().Cleanup(ssAge);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to clean up save files");
                }
                try
                {
                    scope.ServiceProvider.GetRequiredService<UserService>().Cleanup(userAge);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to clean up user accounts");
                }
                if (!rgs.IsScanning)
                {
                    try
                    {
                        rgs.Scan();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to perform periodic ROM scan");
                    }
                }
                else
                {
                    logger.LogInformation("A ROM scanner task is already running. Scan skipped");
                }
            }
            else
            {
                logger.LogInformation("Application is not configured. Maintenance has been skipped.");
            }
        }
    }
}
