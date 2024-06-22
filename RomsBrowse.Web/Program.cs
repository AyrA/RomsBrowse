/*
    RomsBrowse - A ROM file browser with built-in emulator
    Copyright (C) 2024  Kevin Gut

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.WindowsServices;
using RomsBrowse.Data;
using RomsBrowse.Web.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
});

//Register as a windows service
builder.Host.UseWindowsService(options =>
{
    options.ServiceName = "RomsBrowse";
});

#if DEBUG
AutoDIExtensions.Logger = Console.Out;
AutoDIExtensions.DebugLogging = true;
#endif

builder.Services.AddLogging(ConfigureLogging);

//Auto register services
builder.Services.AutoRegisterAllAssemblies();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.Use((context, next) =>
{
    string[] headers =
    [
        "connect-src 'self' blob: data:"
    ];
    context.Response.Headers.ContentSecurityPolicy = string.Join(";", headers);
    return next();
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles(new StaticFileOptions()
{
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

//Run helpers
SetThreadLanguage("de-ch");
await MigrateAsync();
await InitDataFields();

await app.RunAsync();

// Helper

async Task InitDataFields()
{
    using var scope = app.Services.CreateScope();
    var menuService = app.Services.GetRequiredService<MainMenuService>();
    var platformService = scope.ServiceProvider.GetRequiredService<PlatformService>();
    var counts = await platformService.GetAllRomCount();
    menuService.SetMenuItems(await platformService.GetPlatforms(false), counts);
}

async Task MigrateAsync()
{
    using var scope = app.Services.CreateScope();
    var migLog = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var ctx = scope.ServiceProvider.GetRequiredService<RomsContext>();
    var pending = (await ctx.Database.GetPendingMigrationsAsync()).ToList();
    using var logScope = migLog.BeginScope(ctx);
    if (pending.Count > 0)
    {
        migLog.LogInformation("Applying all pending migrations:");
        foreach (var mig in pending)
        {
            migLog.LogInformation("Pending: {Migration}", mig);
        }
        try
        {
            await ctx.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            migLog.LogCritical(ex, "Failed to apply migrations");
            throw;
        }
        migLog.LogInformation("Done!");
    }
    else
    {
        migLog.LogInformation("Database has no pending changes to be applied");
    }
}

static void ConfigureLogging(ILoggingBuilder builder)
{
    var isWin = OperatingSystem.IsWindows();
    //Log to console if not run as a service or not on Windows
    if (!isWin || !WindowsServiceHelpers.IsWindowsService())
    {
        builder.AddConsole();
    }

    //Log to event log on windows if run as a service
    if (isWin && WindowsServiceHelpers.IsWindowsService())
    {
        builder.AddEventLog(opt => opt.Filter = (msg, level) => level >= LogLevel.Error);
    }
}

static void SetThreadLanguage(string language)
{
    var culture = new CultureInfo(language);
    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;
}
