using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NhomBroccoli.Data.Context;

namespace NhomBroccoli.Controllers
{
    public class ProductDetailController : Controller
    {
        private readonly StoreContext _storeContext;
        public ProductDetailController(StoreContext storeContext)
        {
            _storeContext = storeContext;
        }
        [Route("{subCategory}/{productName}")]
        public async Task<IActionResult> Index(string? subCategory, string? productName)
        {
            if (string.IsNullOrEmpty(productName))
            {
                return NotFound();
            }
            var product = await _storeContext.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductSizes)
                .FirstOrDefaultAsync(p => p.Name.ToLower().Replace(" ", "-") == productName);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
    }
}
