using AyrA.AutoDI;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace RomsBrowse.Data.Services
{
    [AutoDIRegister(nameof(Register))]

    public class MemoryCacheProvider(IMemoryCache cache)
    {
        public IMemoryCache Cache => cache;

        public void Purge()
        {
            if (cache is MemoryCache memoryCache)
            {
                memoryCache.Clear();
            }
            else
            {
                throw new NotImplementedException($"Purge is not implemented for cache of type {cache.GetType().FullName}");
            }
        }

        public static void Register(IServiceCollection services)
        {
            services
                .AddMemoryCache(opt =>
                {
                    opt.CompactionPercentage = 0.75;
                    opt.SizeLimit = 512 * 1024 * 1024;
                })
                .AddSingleton<MemoryCacheProvider>();
        }
    }
}
