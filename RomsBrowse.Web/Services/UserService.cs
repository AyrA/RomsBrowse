using AyrA.AutoDI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RomsBrowse.Common.Services;
using RomsBrowse.Data;
using RomsBrowse.Data.Enums;
using RomsBrowse.Data.Models;
using RomsBrowse.Web.ServiceModels;
using System.Security.Claims;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Scoped)]
    public class UserService(RomsContext ctx, IPasswordCheckerService passwordChecker, PasswordService passwordService, ILogger<UserService> logger)
    {
        /// <summary>
        /// Cache value for the <see cref="HasAdmin"/> method
        /// </summary>
        private static bool? hasAdmin = null;

        public async Task<bool> Exists(string username)
        {
            return !string.IsNullOrEmpty(username) && await ctx.Users.AnyAsync(m => m.Username == username);
        }

        public async Task<User?> Get(string? username)
        {
            return string.IsNullOrEmpty(username)
                ? null
                : await ctx.Users.FirstOrDefaultAsync(m => m.Username == username);
        }

        public async Task<bool> Create(string username, string password)
        {
            ArgumentException.ThrowIfNullOrEmpty(username);
            ArgumentException.ThrowIfNullOrEmpty(password);

            if (ctx.Users.Any(m => m.Username == username))
            {
                return false;
            }
            passwordChecker.EnsureSafePassword(password);

            var u = new User()
            {
                Username = username,
                Hash = passwordService.HashPassword(password),
                LastActivity = DateTime.UtcNow
            };
            ctx.Users.Add(u);
            try
            {
                await ctx.SaveChangesAsync();
                hasAdmin = null;
                return true;
            }
            catch
            {
                ctx.ChangeTracker.Clear();
            }
            return false;
        }

        public async Task Delete(string userName)
        {
            var user = await ctx.Users
                .Include(m => m.SaveStates)
                .FirstOrDefaultAsync(m => m.Username == userName);
            if (user != null)
            {
                if (user.IsAdmin)
                {
                    throw new InvalidOperationException("Cannot delete an administrator. Remove the flag first");
                }
                ctx.SaveStates.RemoveRange(user.SaveStates);
                ctx.Users.Remove(user);
                await ctx.SaveChangesAsync();
            }
        }

        public int Cleanup(TimeSpan maxAge)
        {
            logger.LogInformation("Running User cleanup. Removing entries older than {Cutoff}", maxAge);
            var cutoff = DateTime.UtcNow.Subtract(maxAge);
            var total =
                ctx.SaveStates.Where(m => m.User.LastActivity < cutoff && !m.User.Flags.HasFlag(Data.Enums.UserFlags.Admin) && !m.User.Flags.HasFlag(Data.Enums.UserFlags.NoExpireUser)).ExecuteDelete() +
                ctx.Users.Where(m => m.LastActivity < cutoff && !m.Flags.HasFlag(Data.Enums.UserFlags.Admin) && !m.Flags.HasFlag(Data.Enums.UserFlags.NoExpireUser)).ExecuteDelete();
            if (total > 0)
            {
                hasAdmin = false;
            }
            return total;
        }

        public async Task<bool> HasAdmin()
        {
            if (hasAdmin.HasValue)
            {
                return hasAdmin.Value;
            }
            hasAdmin = await ctx.Users.AnyAsync(m => m.Flags.HasFlag(Data.Enums.UserFlags.Admin));
            return hasAdmin.Value;
        }

        public async Task<AccountVerifyModel> VerifyAccount(string username, string password)
        {
            ArgumentException.ThrowIfNullOrEmpty(username);
            ArgumentException.ThrowIfNullOrEmpty(password);

            var user = ctx.Users.FirstOrDefault(m => m.Username == username);
            if (user != null && user.CanSignIn)
            {
                if (passwordService.CheckPassword(password, user.Hash, out var update))
                {
                    user.LastActivity = DateTime.UtcNow;
                    if (update)
                    {
                        user.Hash = passwordService.HashPassword(password);
                    }
                    await ctx.SaveChangesAsync();
                    return new(true, user.Username);
                }
            }
            return new(false, username);
        }

        public ClaimsPrincipal GetPrincipal(string username)
        {
            Claim[] claims = [
                new Claim(ClaimTypes.Name, username),
                new Claim("CreatedAt", DateTime.UtcNow.ToString("O"), ClaimValueTypes.DateTime)
            ];
            var ident = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(ident);
        }

        public async Task<bool> Ping(string username)
        {
            return 0 < await ctx.Users
                .Where(m => m.Username == username)
                .ExecuteUpdateAsync(m => m.SetProperty(p => p.LastActivity, DateTime.UtcNow));
        }

        public async Task<bool> SetFlags(string username, UserFlags flags)
        {
            var u = await ctx.Users.FirstOrDefaultAsync(m => m.Username == username);
            if (u == null)
            {
                return false;
            }
            u.Flags = flags;
            if (flags.HasFlag(UserFlags.Admin))
            {
                hasAdmin = true;
            }
            else
            {
                hasAdmin = null;
            }
            await ctx.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddFlag(string username, UserFlags flags)
        {
            var u = await ctx.Users.FirstOrDefaultAsync(m => m.Username == username);
            if (u == null)
            {
                return false;
            }
            u.Flags |= flags;
            if (u.Flags.HasFlag(UserFlags.Admin))
            {
                hasAdmin = true;
            }
            await ctx.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFlag(string username, UserFlags flags)
        {
            var u = await ctx.Users.FirstOrDefaultAsync(m => m.Username == username);
            if (u == null)
            {
                return false;
            }
            u.Flags &= ~flags;
            if (flags.HasFlag(UserFlags.Admin))
            {
                hasAdmin = null;
            }
            await ctx.SaveChangesAsync();
            return true;
        }
    }
}
