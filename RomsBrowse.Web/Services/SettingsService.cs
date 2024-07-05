using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using RomsBrowse.Data;
using RomsBrowse.Data.Models;
using RomsBrowse.Web.Extensions;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Scoped)]
    public class SettingsService(RomsContext ctx, SetupService ss)
    {
        public static class KnownSettings
        {
            public const string AdminToken = nameof(AdminToken);
            public const string AllowRegister = nameof(AllowRegister);
            public const string AnonymousPlay = nameof(AnonymousPlay);
            public const string MaxSaveStatesPerUser = nameof(MaxSaveStatesPerUser);
            public const string SaveStateExpiration = nameof(SaveStateExpiration);
            public const string UserExpiration = nameof(UserExpiration);
            public const string RomDirectory = nameof(RomDirectory);
        }

        private static readonly Dictionary<string, string?> cache = new(StringComparer.InvariantCultureIgnoreCase);

        private static void SetCache(RomsContext ctx, SetupService ss)
        {
            if (cache.Count == 0 && ss.IsConfigured)
            {
                foreach (var setting in ctx.Settings.AsNoTracking())
                {
                    cache.Add(setting.Name, setting.Value);
                }
            }
        }

        /// <summary>
        /// Gets or sets raw settings
        /// </summary>
        /// <param name="name">Setting name</param>
        /// <returns>Setting value. null if setting was not found</returns>
        public string? this[string name]
        {
            get
            {
                ArgumentNullException.ThrowIfNull(name);
                lock (cache)
                {
                    SetCache(ctx, ss);
                    return cache.TryGetValue(name, out var value) ? value : null;
                }
            }
            set
            {
                ArgumentNullException.ThrowIfNull(name);
                lock (cache)
                {
                    SetCache(ctx, ss);
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
                SetCache(ctx, ss);
                return cache.ContainsKey(name);
            }
        }

        /// <summary>
        /// Gets a parsed setting value
        /// </summary>
        /// <typeparam name="T">Setting value type</typeparam>
        /// <param name="name">Setting name</param>
        /// <returns>Setting value, or default if not found</returns>
        public T? GetValue<T>(string name)
        {
            lock (cache)
            {
                SetCache(ctx, ss);
                if (cache.TryGetValue(name, out var s) && s != null)
                {
                    return s.FromJson<T>();
                }
                return default;
            }
        }

        /// <summary>
        /// Tries to get a parsed setting value
        /// </summary>
        /// <param name="name">Setting name</param>
        /// <param name="value">Setting value receiver</param>
        /// <returns>true, if setting was read, false if not found</returns>
        public bool TryGetSetting<T>(string name, out T? value)
        {
            lock (cache)
            {
                SetCache(ctx, ss);
                if (cache.TryGetValue(name, out var raw))
                {
                    value = raw == null ? default : raw.FromJson<T>();
                    return true;
                }
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Tries to get a raw setting value
        /// </summary>
        /// <param name="name">Setting name</param>
        /// <param name="value">Setting value receiver</param>
        /// <returns>true, if setting was read, false if not found</returns>
        public bool TryGetSettingRaw(string name, out string? value)
        {
            lock (cache)
            {
                SetCache(ctx, ss);
                return cache.TryGetValue(name, out value);
            }
        }

        /// <summary>
        /// Adds a setting or updates an existing one
        /// </summary>
        /// <param name="name">Setting name</param>
        /// <param name="value">Setting value</param>
        public void AddOrUpdate<T>(string name, T value)
        {
            ArgumentNullException.ThrowIfNull(name);
            var parsed = value == null ? "null" : value.ToJson();
            this[name] = parsed;
        }

        /// <summary>
        /// Adds a setting value only if it doesn't already exists
        /// </summary>
        /// <param name="name">Setting name</param>
        /// <param name="value">Setting value</param>
        /// <returns>true if added, false if it already exists</returns>
        public bool AddDefault<T>(string name, T value)
        {
            ArgumentNullException.ThrowIfNull(name);
            lock (cache)
            {
                var parsed = value == null ? "null" : value.ToJson();
                SetCache(ctx, ss);
                if (!cache.ContainsKey(name))
                {
                    this[name] = parsed;
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
                SetCache(ctx, ss);
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
