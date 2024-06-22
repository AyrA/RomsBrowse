using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using RomsBrowse.Common.Services;
using RomsBrowse.Data;
using RomsBrowse.Data.Models;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Scoped)]
    public class UserService(RomsContext ctx, IPasswordCheckerService passwordChecker, PasswordService passwordService, ILogger<UserService> logger)
    {
        public async Task<bool> Exists(string username)
        {
            return !string.IsNullOrEmpty(username) && await ctx.Users.AnyAsync(m => m.Username == username);
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
                LastLogin = DateTime.UtcNow
            };
            ctx.Users.Add(u);
            try
            {
                await ctx.SaveChangesAsync();
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
                .FirstOrDefaultAsync(m => m.Username == userName);
            if (user != null)
            {
                await ctx.SaveStates.Where(m => m.UserId == user.Id).ExecuteDeleteAsync();
                ctx.Users.Remove(user);
                await ctx.SaveChangesAsync();
            }
        }

        public int Cleanup(TimeSpan maxAge)
        {
            logger.LogInformation("Running User cleanup. Removing entries older than {Cutoff}", maxAge);
            var cutoff = DateTime.UtcNow.Subtract(maxAge);
            return
                ctx.SaveStates.Where(m => m.User.LastLogin < cutoff).ExecuteDelete() +
                ctx.Users.Where(m => m.LastLogin < cutoff).ExecuteDelete();
        }

        public async Task<bool> VerifyAccount(string username, string password)
        {
            ArgumentException.ThrowIfNullOrEmpty(username);
            ArgumentException.ThrowIfNullOrEmpty(password);

            var user = ctx.Users.FirstOrDefault(m => m.Username == username);
            if (user == null)
            {
                return false;
            }
            if (passwordService.CheckPassword(password, user.Hash, out var update))
            {
                user.LastLogin = DateTime.UtcNow;
                if (update)
                {
                    user.Hash = passwordService.HashPassword(password);
                }
                await ctx.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
