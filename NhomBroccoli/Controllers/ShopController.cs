using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NhomBroccoli.Data.Context;
using NhomBroccoli.Data.Entities;
using NhomBroccoli.Models;

namespace NhomBroccoli.Controllers
{
    [Route("shop")]
    public class ShopController : Controller
    {
        private readonly StoreContext _storeContext;
        public ShopController(StoreContext storeContext)
        {
            _storeContext = storeContext;
        }
        //[HttpGet("{subCategoryName}")]
        public async Task<IActionResult> Index(string? subCategoryName)
        {
            var categories = await _storeContext.Categories
                .Include(p => p.SubCategories)
                .ToListAsync();

            IQueryable<Product> productQuery = _storeContext.Products
                .Include(p => p.ProductImages)
                .Include(p => p.SubCategory);

            if (!string.IsNullOrEmpty(subCategoryName))
            {
                productQuery = productQuery.Where(p => p.SubCategory.Name == subCategoryName);
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

        [HttpGet("{subCategoryName}")]
        public async Task<IActionResult> IndexWithSubcategory(string? subCategoryName)
        {
            var categories = await _storeContext.Categories
                .Include(p => p.SubCategories)
                .ToListAsync();

            IQueryable<Product> productQuery = _storeContext.Products
                .Include(p => p.ProductImages)
                .Include(p => p.SubCategory);

            if (!string.IsNullOrEmpty(subCategoryName))
            {
                productQuery = productQuery.Where(p => p.SubCategory.Name.ToLower().Replace(" ", "-").Replace("\'s", "") == subCategoryName);
            }

            var products = await productQuery.ToListAsync();            

            var viewModel = new CategoriesAndProducts
            {
                Categories = categories,
                Products = products
            };            
            return View(viewModel);
        }
    }
}
