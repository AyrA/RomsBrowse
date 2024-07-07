using AyrA.AutoDI;
using Microsoft.AspNetCore.Mvc;
using RomsBrowse.Data.Services;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Singleton)]
    public class SetupService(DbContextSettingsProvider cstr)
    {
        public bool IsConfigured => cstr.IsConnectionStringSet;

        public IActionResult SetupRedirect => new RedirectToActionResult("Index", "Init", null);

        public void SetConnectionString(string connStr, string dbProvider)
        {
            cstr.SetSettings(connStr, dbProvider);
        }
    }
}
