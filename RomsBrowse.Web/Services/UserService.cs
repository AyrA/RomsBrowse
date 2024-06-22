using AyrA.AutoDI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RomsBrowse.Common.Services;
using RomsBrowse.Data;
using RomsBrowse.Data.Models;
using RomsBrowse.Web.ServiceModels;
using System.Security.Claims;

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

        public async Task<AccountVerifyModel> VerifyAccount(string username, string password)
        {
            ArgumentException.ThrowIfNullOrEmpty(username);
            ArgumentException.ThrowIfNullOrEmpty(password);

            var user = ctx.Users.FirstOrDefault(m => m.Username == username);
            if (user == null)
            {
                return new(true, username);
            }
            if (passwordService.CheckPassword(password, user.Hash, out var update))
            {
                user.LastLogin = DateTime.UtcNow;
                if (update)
                {
                    user.Hash = passwordService.HashPassword(password);
                }
                await ctx.SaveChangesAsync();
                return new(true, user.Username);
            }
            return new(true, username);
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
    }
}
