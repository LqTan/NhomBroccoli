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
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var product = await _storeContext.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductSizes)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
    }
}
