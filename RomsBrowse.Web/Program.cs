using AyrA.AutoDI;
using Microsoft.EntityFrameworkCore;
using RomsBrowse.Data;
using RomsBrowse.Web.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
AutoDIExtensions.Logger = Console.Out;
AutoDIExtensions.DebugLogging = true;
#endif

builder.Services.AddLogging(opt => opt.AddConsole());

//Auto register services
builder.Services.AutoRegisterCurrentAssembly();
builder.Services.AutoRegisterFromAssembly(typeof(RomsContext).Assembly);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//Run helpers
SetThreadLanguage("de-ch");
await MigrateAsync();
await UpdateRomDirectory();

await app.RunAsync();

// Helper

async Task UpdateRomDirectory()
{
    using var scope = app.Services.CreateScope();
    var rgs = scope.ServiceProvider.GetRequiredService<RomGatherService>();
    await rgs.GatherRoms();
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

static void SetThreadLanguage(string language)
{
    var culture = new CultureInfo(language);
    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;
}
