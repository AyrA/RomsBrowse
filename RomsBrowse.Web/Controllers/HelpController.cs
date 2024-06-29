using Microsoft.AspNetCore.Mvc;
using RomsBrowse.Web.Services;

namespace RomsBrowse.Web.Controllers
{
    public class HelpController(UserService us) : BaseController(us)
    {
        public IActionResult Saves() => View();
    }
}
