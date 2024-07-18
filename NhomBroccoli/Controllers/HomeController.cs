using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NhomBroccoli.Data.Context;
using NhomBroccoli.Data.Entities;
using NhomBroccoli.Models;
using System.Diagnostics;

namespace NhomBroccoli.Controllers
{    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly StoreContext _storeContext;

        public HomeController(ILogger<HomeController> logger, StoreContext storeContext)
        {
            _logger = logger;
            _storeContext = storeContext;
        }

        public async Task<IActionResult> Index()
        {
            //var payment = Request.Cookies["Payment"] != null 
            //    ? JsonConvert.DeserializeObject<Payment>(Request.Cookies["Payment"])
            //    : new Payment { Total = 0 };
            //Response.Cookies.Append("Payment", JsonConvert.SerializeObject(payment));
            //Console.WriteLine($"Total: {payment.Total}");

            var products = await _storeContext.Products
                .Include(p => p.ProductImages)
                .Include(p => p.SubCategory)
                .ThenInclude(sc => sc.Category)
                .ToListAsync();
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
