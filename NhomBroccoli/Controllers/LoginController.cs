using Microsoft.AspNetCore.Mvc;

namespace NhomBroccoli.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
