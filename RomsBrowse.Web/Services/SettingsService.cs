using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using RomsBrowse.Data;
using RomsBrowse.Data.Models;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Scoped)]
    public class SettingsService(RomsContext ctx)
    {
        private static readonly Dictionary<string, string?> cache = new(StringComparer.InvariantCultureIgnoreCase);

        private static void SetCache(RomsContext ctx)
        {
            if (cache.Count == 0)
            {
                foreach (var setting in ctx.Settings)
                {
                    cache.Add(setting.Name, setting.Value);
                }
            }
        }

        /// <summary>
        /// Gets or sets settings
        /// </summary>
        /// <param name="name">Setting name</param>
        /// <returns>Setting value. Null if setting was not found</returns>
        public string? this[string name]
        {
            get
            {
                ArgumentNullException.ThrowIfNull(name);
                lock (cache)
                {
                    SetCache(ctx);
                    return cache.TryGetValue(name, out var value) ? value : null;
                }
            }
            set
            {
                ArgumentNullException.ThrowIfNull(name);
                lock (cache)
                {
                    SetCache(ctx);
                    var setting = new Setting()
                    {
                        Name = name,
                        Value = value
                    };
                    setting.Validate();
                    if (cache.ContainsKey(setting.Name))
                    {
                        ctx.Settings.Update(setting);
                    }
                    else
                    {
                        ctx.Settings.Add(setting);
                    }
                    ctx.SaveChanges();
                    cache[name] = value;
                }
            }
        }

        /// <summary>
        /// Checks if a setting exists without reading its value
        /// </summary>
        /// <param name="name">Setting name</param>
        /// <returns>Setting value</returns>
        /// <remarks>
        /// Use <see cref="TryGetSetting(string, out string?)"/> if the value is needed
        /// </remarks>
        public bool HasSetting(string name)
        {
            lock (cache)
            {
                SetCache(ctx);
                return cache.ContainsKey(name);
            }
        }

        /// <summary>
        /// Tries to get a setting value
        /// </summary>
        /// <param name="name">Setting name</param>
        /// <param name="value">Setting value</param>
        /// <returns>true, if setting was read, false if not found</returns>
        public bool TryGetSetting(string name, out string? value)
        {
            lock (cache)
            {
                SetCache(ctx);
                return cache.TryGetValue(name, out value);
            }
        }

        /// <summary>
        /// Adds a setting or updates an existing one
        /// </summary>
        /// <param name="name">Setting name</param>
        /// <param name="value">Setting value</param>
        /// <remarks>
        /// This is an alias for the setter at <see cref="this[string]"/>
        /// </remarks>
        public void AddOrUpdate(string name, string? value)
        {
            ArgumentNullException.ThrowIfNull(name);
            this[name] = value;
        }

        /// <summary>
        /// Adds a setting value only if it doesn't already exists
        /// </summary>
        /// <param name="name">Setting name</param>
        /// <param name="value">Setting value</param>
        /// <returns>true if added, false if it already exists</returns>
        public bool AddDefault(string name, string? value)
        {
            ArgumentNullException.ThrowIfNull(name);
            lock (cache)
            {
                SetCache(ctx);
                if (!cache.ContainsKey(name))
                {
                    this[name] = value;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Deletes a setting
        /// </summary>
        /// <param name="name">Setting name</param>
        /// <returns>true, if deleted, false if no setting found</returns>
        public bool Delete(string name)
        {
            ArgumentNullException.ThrowIfNull(name);
            lock (cache)
            {
                SetCache(ctx);
                if (cache.Remove(name))
                {
                    ctx.Settings.Where(m => m.Name == name).ExecuteDelete();
                    return true;
                }
            }
            return false;
        }
    }
}
