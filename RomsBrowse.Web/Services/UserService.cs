using AyrA.AutoDI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RomsBrowse.Common.Services;
using RomsBrowse.Data;
using RomsBrowse.Data.Enums;
using RomsBrowse.Data.Models;
using RomsBrowse.Web.ServiceModels;
using RomsBrowse.Web.ViewModels;
using System.Security.Claims;
using System.Text;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Scoped)]
    public class UserService(RomsContext ctx, IMemoryCache cache, IPasswordCheckerService passwordChecker, PasswordService passwordService, ILogger<UserService> logger)
    {
        /// <summary>
        /// Cache value for the <see cref="HasAdmin"/> method
        /// </summary>
        private static bool? hasAdmin = null;

        public async Task<bool> Exists(string username)
        {
            return !string.IsNullOrEmpty(username) && await ctx.Users.AnyAsync(m => m.Username == username);
        }

        public User? Get(string? username, bool track)
        {
            if (string.IsNullOrEmpty(username))
            {
                return null;
            }

            var user = cache.GetOrCreate($"user:{username.ToLowerInvariant()}", (item) =>
            {
                var user = ctx.Users.AsNoTracking().FirstOrDefault(m => m.Username == username);
                item.Size = GetSize(user);
                if (user?.CanExpire ?? false)
                {
                    item.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
                }

                return user;
            });
            if (track && user != null)
            {
                ctx.Users.Attach(user);
            }
            return user;

        }

        public User? Get(int userId, bool track)
        {
            var user = cache.GetOrCreate($"user:{userId}", (item) =>
            {
                var user = ctx.Users.AsNoTracking().FirstOrDefault(m => m.Id == userId);
                item.Size = GetSize(user);
                return user;
            });
            if (track && user != null)
            {
                ctx.Users.Attach(user);
            }
            return user;
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
                .Include(m => m.SaveData)
                .FirstOrDefaultAsync(m => m.Username == userName);
            await DeleteUser(user);
        }

        public async Task Delete(int userId)
        {
            var user = await ctx.Users
                .Include(m => m.SaveData)
                .FirstOrDefaultAsync(m => m.Id == userId);
            await DeleteUser(user);
        }

        public async Task ChangeLock(string userName)
        {
            var user = await ctx.Users
                .Include(m => m.SaveData)
                .FirstOrDefaultAsync(m => m.Username == userName);
            await ChangeLock(user);
        }

        public async Task ChangeLock(int userId)
        {
            var user = await ctx.Users
                .Include(m => m.SaveData)
                .FirstOrDefaultAsync(m => m.Id == userId);
            await ChangeLock(user);
        }

        public async Task ChangeAdmin(string userName)
        {
            var user = await ctx.Users
                .Include(m => m.SaveData)
                .FirstOrDefaultAsync(m => m.Username == userName);
            await ChangeAdmin(user);
        }

        public async Task ChangeAdmin(int userId)
        {
            var user = await ctx.Users
                .Include(m => m.SaveData)
                .FirstOrDefaultAsync(m => m.Id == userId);
            await ChangeAdmin(user);
        }

        public int Cleanup(TimeSpan maxAge)
        {
            if (maxAge > TimeSpan.Zero)
            {
                logger.LogInformation("Running user cleanup. Removing entries older than {Cutoff}", maxAge);
                var cutoff = DateTime.UtcNow.Subtract(maxAge);
                var total =
                    ctx.SaveData.Where(m => m.User.LastActivity < cutoff && !m.User.Flags.HasFlag(UserFlags.Admin) && !m.User.Flags.HasFlag(UserFlags.NoExpireUser)).ExecuteDelete();
                var users = ctx.Users.Where(m => m.LastActivity < cutoff && !m.Flags.HasFlag(UserFlags.Admin) && !m.Flags.HasFlag(UserFlags.NoExpireUser)).ToList();
                foreach (var user in users)
                {
                    ctx.Users.Remove(user);
                    cache.Remove($"user:{user.Username}");
                    cache.Remove($"user:{user.Id}");
                    ++total;
                }
                if (total > 0)
                {
                    hasAdmin = false;
                }
                return total;
            }
            else
            {
                logger.LogInformation("Skipping user cleanup. User accounts are currently set to not expire");
            }
            return 0;
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

            var user = Get(username, true);
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
                    return new(true, user);
                }
            }
            return new(false, null);
        }

        public ClaimsPrincipal GetPrincipal(User user)
        {
            List<Claim> claims = [
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("CreatedAt", DateTime.UtcNow.ToString("O"), ClaimValueTypes.DateTime)
            ];
            if (user.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, nameof(UserFlags.Admin)));
            }
            var ident = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(ident);
        }

        public async Task<bool> Ping(string username)
        {
            var user = Get(username, true);
            if (user != null)
            {
                user.LastActivity = DateTime.UtcNow;
                return 0 < await ctx.SaveChangesAsync();
            }
            return false;
        }

        public async Task<bool> SetFlags(string username, UserFlags flags)
        {
            var u = Get(username, true);
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
            var u = Get(username, true);
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
            var u = Get(username, true);
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

        public async Task ChangePassword(string username, string oldPassword, string newPassword)
        {
            var user = Get(username, true)
                ?? throw new Exception("User does not exist");

            passwordChecker.EnsureSafePassword(newPassword);

            if (!passwordService.CheckPassword(oldPassword, user.Hash, out _))
            {
                throw new Exception("Old password is incorrect");
            }
            user.Hash = passwordService.HashPassword(newPassword);
            await ctx.SaveChangesAsync();
        }

        public async Task<AccountsViewModel> GetAccounts(int page, int pageSize, string? search)
        {
            page = Math.Max(1, page);
            pageSize = Math.Max(1, pageSize);
            var users = await FilterUsers(search)
                .OrderBy(m => m.Username)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vm = new AccountsViewModel
            {
                Paging = new(page, (int)Math.Ceiling(users.Count / (double)pageSize), 2),
                Accounts = users.Select(m => new AccountViewModel(m)).ToArray()
            };
            return vm;
        }

        public void SetNewPassword(User user, string newPassword)
        {
            passwordChecker.EnsureSafePassword(newPassword);
            user.Hash = passwordService.HashPassword(newPassword);
        }

        public async Task SaveChanges(User user)
        {
            user.Validate();
            if (!ctx.ChangeTracker.Entries<User>().Any(m => m.Entity == user))
            {
                logger.LogWarning("User #{Id} was not tracked when call to SaveChanges was made", user.Id);
                ctx.Users.Update(user);
            }
            await ctx.SaveChangesAsync();
        }

        private IQueryable<User> FilterUsers(string? username)
        {
            var users = ctx.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(username))
            {
                users = users.Where(m => m.Username.Contains(username));
            }
            return users;
        }

        private async Task DeleteUser(User? user)
        {
            if (user == null)
            {
                ArgumentNullException.ThrowIfNull(user);
            }
            if (!user.CanDelete)
            {
                throw new InvalidOperationException("Cannot delete user. Admins and users without expiration cannot de deleted, and the flags need to be removed first");
            }
            if (!user.IsLocked)
            {
                throw new InvalidOperationException("To confirm account removal, the account must be locked first.");
            }
            ctx.SaveData.RemoveRange(user.SaveData);
            ctx.Users.Remove(user);
            cache.Remove($"user:{user.Username.ToLowerInvariant()}");
            cache.Remove($"user:{user.Id}");
            await ctx.SaveChangesAsync();
        }

        private async Task ChangeLock(User? user)
        {
            if (user == null)
            {
                ArgumentNullException.ThrowIfNull(user);
            }
            if (user.IsAdmin && !user.IsLocked)
            {
                throw new InvalidOperationException("Cannot lock admins. Remove the admin flag first");
            }
            user.IsLocked = !user.IsLocked;
            await SaveChanges(user);
        }

        private async Task ChangeAdmin(User? user)
        {
            if (user == null)
            {
                ArgumentNullException.ThrowIfNull(user);
            }
            if (user.IsAdmin)
            {
                if (ctx.Users.Count(m => m.Flags.HasFlag(UserFlags.Admin)) < 2)
                {
                    throw new InvalidOperationException("Cannot demote the last admin. Promote a different user to admininistator before removing the admin flag from this user");
                }
            }
            user.IsAdmin = !user.IsAdmin;
            if (user.IsAdmin)
            {
                user.IsLocked = false;
            }
            await SaveChanges(user);
        }

        /// <summary>
        /// Gets the approximate memory size of a user entry
        /// </summary>
        /// <param name="user">User entry</param>
        /// <returns>Size. 0 if null</returns>
        /// <remarks>
        /// This is not necessarily accurate,
        /// and is only relevant for purging entries
        /// </remarks>
        private static long GetSize(User? user)
        {
            if (user == null)
            {
                return 0;
            }
            return
                64 //Base size overhead estimate
                + Encoding.UTF8.GetByteCount(user.Username)
                + Encoding.UTF8.GetByteCount(user.Hash)
                + sizeof(UserFlags)
                + sizeof(long) + sizeof(DateTimeKind) //Expiration
                + sizeof(int) // Id
                ;
        }
    }
}
