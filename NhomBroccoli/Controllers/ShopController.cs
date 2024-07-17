using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NhomBroccoli.Data.Context;
using NhomBroccoli.Data.Entities;
using NhomBroccoli.Models;

namespace NhomBroccoli.Controllers
{
    public class ShopController : Controller
    {
        private readonly StoreContext _storeContext;
        public ShopController(StoreContext storeContext)
        {
            _storeContext = storeContext;
        }
        public async Task<IActionResult> Index(int? subCategoryId)
        {
            var categories = await _storeContext.Categories
                .Include(p => p.SubCategories)
                .ToListAsync();

            IQueryable<Product> productQuery = _storeContext.Products
                .Include(p => p.ProductImages)
                .Include(p => p.SubCategory);

            if (subCategoryId.HasValue)
            {
                productQuery = productQuery.Where(p => p.SubCategoryId == subCategoryId.Value);
            }

            var products = await productQuery.ToListAsync();

            //var products = await _storeContext.Products
            //    .Include(p => p.ProductImages)
            //    .Include(p => p.SubCategory)
            //    .ToListAsync();

            var viewModel = new CategoriesAndProducts
            {
                Categories = categories,
                Products = products
            };
            return View(viewModel);
        }
    }
}
