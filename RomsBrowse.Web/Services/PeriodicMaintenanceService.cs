using AyrA.AutoDI;
using RomsBrowse.Data;

namespace RomsBrowse.Web.Services
{
    [AutoDIHostedService]
    public class PeriodicMaintenanceService(IServiceProvider provider) : IHostedService, IDisposable
    {
        private Timer? _timer;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer?.Dispose();
            _timer = new Timer(Callback, null, TimeSpan.FromSeconds(1), TimeSpan.FromHours(1));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
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
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SqlServerContext>();
            if (db.IsConfigured)
            {
                var ss = scope.ServiceProvider.GetRequiredService<SettingsService>();
                var rgs = scope.ServiceProvider.GetRequiredService<RomGatherService>();

                var userAge = ss.GetValue<TimeSpan>(SettingsService.KnownSettings.UserExpiration);
                var ssAge = ss.GetValue<TimeSpan>(SettingsService.KnownSettings.SaveStateExpiration);

                scope.ServiceProvider.GetRequiredService<SaveService>().Cleanup(ssAge);
                scope.ServiceProvider.GetRequiredService<UserService>().Cleanup(userAge);
                if (!rgs.IsScanning)
                {
                    try
                    {
                        rgs.Scan();
                    }
                    catch
                    {
                        //NOOP
                    }
                }
            }
        }
    }
}
