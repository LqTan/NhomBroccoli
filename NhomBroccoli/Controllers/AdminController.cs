using Microsoft.AspNetCore.Mvc;

namespace NhomBroccoli.Controllers
{
    [Route("admin")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
