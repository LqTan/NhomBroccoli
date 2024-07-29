using Microsoft.AspNetCore.Mvc;

namespace NhomBroccoli.Controllers
{
    [Route("login")]
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
