using Microsoft.AspNetCore.Mvc;
using NhomBroccoli.Models;
using NhomBroccoli.Services;

namespace NhomBroccoli.Controllers
{
    public class EmailController : Controller
    {
        private readonly IEmailService _emailService;
        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }
        [HttpGet]
        public IActionResult SendEmail()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SendEmail(CartItemsPaymentUser model)
        {
            if (ModelState.IsValid)
            {
                await _emailService.SendEmailAsync(model);
                ViewBag.Message = "Email sent successfully!";
            }
            return View(model);
        }
    }
}
