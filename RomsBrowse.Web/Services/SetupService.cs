using AyrA.AutoDI;
using Microsoft.AspNetCore.Mvc;
using RomsBrowse.Data.Services;

namespace RomsBrowse.Web.Services
{
    [AutoDIRegister(AutoDIType.Singleton)]
    public class SetupService(ConnectionStringProvider cstr)
    {
        public bool IsConfigured => cstr.IsSet;

        public IActionResult SetupRedirect => new RedirectToActionResult("Index", "Init", null);

        public void SetConnectionString(string connStr)
        {
            cstr.SetConnectionString(connStr);
        }
    }
}
