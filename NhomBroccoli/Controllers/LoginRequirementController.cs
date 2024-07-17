using Microsoft.AspNetCore.Mvc;

namespace NhomBroccoli.Controllers
{
    public class LoginRequirementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
