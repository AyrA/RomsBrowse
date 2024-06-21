using AyrA.AutoDI;

namespace RomsBrowse.Web.Services
{
    [AutoDIHostedService]
    public class PeriodicMaintenanceService(IServiceProvider provider) : IHostedService, IDisposable
    {
        private static readonly TimeSpan maxAge = TimeSpan.FromDays(30);
        private Timer? _timer;

        public Task StartAsync(CancellationToken cancellationToken)
        {
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
            scope.ServiceProvider.GetRequiredService<SaveStateService>().Cleanup(maxAge);
            scope.ServiceProvider.GetRequiredService<UserService>().Cleanup(maxAge);
        }
    }
}
