using Microsoft.AspNetCore.Mvc;

namespace NhomBroccoli.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
