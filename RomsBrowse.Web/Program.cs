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
using Microsoft.Extensions.Hosting.WindowsServices;
using RomsBrowse.Common.Services;
using RomsBrowse.Data;
using RomsBrowse.Web.Controllers;
using RomsBrowse.Web.Services;
using Serilog;
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

//Logging
#if DEBUG
AutoDIExtensions.Logger = Console.Out;
AutoDIExtensions.DebugLogging = true;
#endif
builder.Services.AddSerilog((opt) => ConfigureLogging(opt, builder.Configuration));

//Auto register services
builder.Services.AutoRegisterAllAssemblies();

//.NET services
builder.Services.AddControllersWithViews();
builder.Services
    .AddAuthentication()
    .AddCookie(opts =>
{
    opts.ExpireTimeSpan = TimeSpan.FromDays(30);
    opts.Cookie.HttpOnly = true;
    opts.Cookie.IsEssential = true;
    opts.Cookie.Name = "Session";
    opts.Cookie.SameSite = SameSiteMode.Strict;
    opts.Cookie.MaxAge = TimeSpan.FromDays(30);
    opts.LoginPath = "/Account/Login";
    opts.LogoutPath = "/Account/Logout";
    opts.ReturnUrlParameter = "returnUrl";
});

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

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.UseStaticFiles(new StaticFileOptions()
{
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

SetThreadLanguage("de-ch");
if (IsDbConfigured())
{
    using var scope = app.Services.CreateScope();
    var ss = scope.ServiceProvider.GetRequiredService<SetupService>();
    await ss.FullInit();
}
InitDefaultControllerParams();

await app.RunAsync();

// Helper

void InitDefaultControllerParams()
{
    var enc = app.Services.GetRequiredService<ITempEncryptionService>();
    var ss = app.Services.GetRequiredService<SetupService>();
    BaseController.SetStaticServices(enc, ss);
}


bool IsDbConfigured()
{
    using var scope = app.Services.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
    return ctx.IsConfigured;
}

static void ConfigureLogging(LoggerConfiguration logger, IConfiguration config)
{
    logger.ReadFrom.Configuration(config);
}

static void SetThreadLanguage(string language)
{
    var culture = new CultureInfo(language);
    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;
}
