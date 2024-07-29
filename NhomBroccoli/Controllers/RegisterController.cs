using Microsoft.AspNetCore.Mvc;

namespace NhomBroccoli.Controllers
{
    [Route("register")]
    public class RegisterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
