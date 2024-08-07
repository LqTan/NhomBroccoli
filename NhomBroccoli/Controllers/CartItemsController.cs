using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NhomBroccoli.Data.Context;
using NhomBroccoli.Data.Entities;
using NhomBroccoli.Models;
using OfficeOpenXml;

namespace NhomBroccoli.Controllers
{
    [Route("cart")]
    public class CartItemsController : Controller
    {
        private readonly StoreContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartItemsController(StoreContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        
        // GET: CartItems
        public async Task<IActionResult> Index()
        {
            var storeContext = _context.CartItems
                .Include(c => c.Order)
                .Include(c => c.Product)
                .Include(c => c.ProductSize);
            return View(await storeContext.ToListAsync());
        }
        [HttpGet("export-cartitems")]
        public async Task<IActionResult> ExportCartItems()
        {
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Include(c => c.ProductSize)
                .Include(c => c.Order)
                .ThenInclude(o => o.OrderUser)
                .ToListAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh sach don hang da mua");

                // Header row
                worksheet.Cells[1, 1].Value = "Product Name";
                worksheet.Cells[1, 2].Value = "Product Size";
                worksheet.Cells[1, 3].Value = "Quantity";
                worksheet.Cells[1, 4].Value = "Amount";
                worksheet.Cells[1, 5].Value = "Customer username";
                worksheet.Cells[1, 6].Value = "Email";
                worksheet.Cells[1, 7].Value = "Order date";

                // Data rows
                for (int i = 0; i < cartItems.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = cartItems[i].Product.Name;
                    worksheet.Cells[i + 2, 2].Value = cartItems[i].ProductSize.Size;
                    worksheet.Cells[i + 2, 3].Value = cartItems[i].Quantity;
                    worksheet.Cells[i + 2, 4].Value = cartItems[i].Amount;
                    worksheet.Cells[i + 2, 5].Value = cartItems[i].Order.OrderUser.UserName;
                    worksheet.Cells[i + 2, 6].Value = cartItems[i].Order.OrderUser.Email;

                    var orderDate = cartItems[i].Order.OrderDate;
                    worksheet.Cells[i + 2, 7].Value = orderDate.HasValue ? orderDate.Value.ToDateTime(new TimeOnly(0, 0)) : (DateTime?)null;
                    worksheet.Cells[i + 2, 7].Style.Numberformat.Format = "dd/mm/yyyy"; // Format as date
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);

                var content = stream.ToArray();

                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Broccoli_orders.xlsx");
            }
        }
        [HttpGet("item-list")]
        public async Task<IActionResult> CartView()
        {
            var token = HttpContext.Request.Cookies["SessionToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Login");
            }
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            var emailClaim = jsonToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var email = emailClaim.Value;

            var user = await _userManager.FindByEmailAsync(email);
            var existingOrder = await _context.Orders.FirstOrDefaultAsync(o => o.OrderStatus == 0 && o.UserId == user.Id);

            var existingPayment = await _context.Payments.FirstOrDefaultAsync(pm => pm.Status == 0 && pm.OrderId == existingOrder.Id);

            var cartItems = await _context.CartItems
                .Where(c => c.OrderId == existingOrder.Id)
                .Include(c => c.Order)
                .Include(c => c.ProductSize)
                .Include(c => c.Product)
                .ThenInclude(p => p.ProductImages)
                .ToListAsync();

            existingPayment.Total = 0;
            foreach (var cartItem in cartItems)
            {
                existingPayment.Total += cartItem.Amount;
            }

            var discount = await _context.Discounts.FirstOrDefaultAsync(d => d.Id == existingPayment.DiscountId);
            if (discount != null)
            {
                existingPayment.Total -= ((double)discount.DiscountPercent / 100) * existingPayment.Total;
            }

            _context.Update(existingPayment);
            await _context.SaveChangesAsync();

            var paymentAndCartItems = new PaymentAndCartItems
            {
                CartItems = cartItems,
                Payment = existingPayment
            };

            return View(paymentAndCartItems);

            //var cart = Request.Cookies["Cart"] != null
            //        ? JsonConvert.DeserializeObject<List<CartItem>>(Request.Cookies["Cart"])
            //        : new List<CartItem>();
            //var payment = JsonConvert.DeserializeObject<Payment>(Request.Cookies["Payment"]);

            //var productIds = cart.Select(c => c.ProductId).ToList();
            //var products = await _context.Products
            //    .Where(p => productIds.Contains(p.Id))
            //    .Include(p => p.ProductImages)
            //    .ToListAsync();
            //var productSizes = await _context.ProductSize
            //    .Where(ps => productIds.Contains(ps.Id))
            //    .ToListAsync();
            //foreach (var item in cart)
            //{
            //    item.Product = products.FirstOrDefault(p => p.Id == item.ProductId);
            //    item.ProductSize = productSizes.FirstOrDefault(ps => ps.Id == item.ProductId);
            //}            

            //var paymentAndCartItems = new PaymentAndCartItems
            //{
            //    CartItems = cart,
            //    Payment = payment
            //};

            //return View(paymentAndCartItems);
        }
        [HttpGet("detail/{id}")]
        // GET: CartItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cartItem = await _context.CartItems
                .Include(c => c.Order)
                .Include(c => c.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cartItem == null)
            {
                return NotFound();
            }

            return View(cartItem);
        }
        [HttpGet("create")]
        // GET: CartItems/Create
        public IActionResult Create()
        {
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id");
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name");
            return View();
        }
        
        // POST: CartItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OrderId,ProductId,Quantity,Amount")] CartItem cartItem)
        {
            if (ModelState.IsValid)
            {
                var existingCartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.ProductId == cartItem.ProductId);
                if (existingCartItem == null)
                {
                    var product = await _context.Products.FirstOrDefaultAsync(c => c.Id == cartItem.ProductId);

                    cartItem.Amount = product.Price * cartItem.Quantity;
                    _context.Add(cartItem);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(CartView));
                }
                var p = await _context.Products.FirstOrDefaultAsync(_ => _.Id == existingCartItem.ProductId);
                existingCartItem.Quantity += cartItem.Quantity;
                existingCartItem.Amount = p.Price * existingCartItem.Quantity;
                return await Edit(existingCartItem.Id, existingCartItem);
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Name", cartItem.OrderId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", cartItem.ProductId);
            return View(cartItem);
        }
        
        [HttpPost("add-to-cart")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart([Bind("Id,OrderId,ProductId,ProductSizeId,Quantity,Amount")] CartItem cartItem)
        {
            var token = HttpContext.Request.Cookies["SessionToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Login");
            }
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            var emailClaim = jsonToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var email = emailClaim.Value;

            var user = await _userManager.FindByEmailAsync(email);

            var existingOrder = await _context.Orders.FirstOrDefaultAsync(o => o.OrderStatus == 0 && o.UserId == user.Id);

            if (ModelState.IsValid)
            {
                var cart = Request.Cookies["Cart"] != null
                    ? JsonConvert.DeserializeObject<List<CartItem>>(Request.Cookies["Cart"])
                    : new List<CartItem>();

                var existingCartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.ProductId == cartItem.ProductId && c.ProductSizeId == cartItem.ProductSizeId && c.OrderId == existingOrder.Id);

                if (existingCartItem == null)
                {
                    var product = await _context.Products.FirstOrDefaultAsync(c => c.Id == cartItem.ProductId);

                    cartItem.Amount = product.Price * cartItem.Quantity;
                    cart.Add(cartItem);
                    Response.Cookies.Append("Cart", JsonConvert.SerializeObject(cart), new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                    });

                    cartItem.OrderId = existingOrder?.Id;
                    _context.Add(cartItem);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(CartView));
                }
                var p = await _context.Products.FirstOrDefaultAsync(_ => _.Id == existingCartItem.ProductId);
                existingCartItem.Quantity += cartItem.Quantity;
                existingCartItem.Amount = p.Price * existingCartItem.Quantity;


                existingCartItem.OrderId = existingOrder?.Id;

                return await Edit(existingCartItem.Id, existingCartItem);
            }

            return RedirectToAction("Index", "Shop");

            //if (ModelState.IsValid)
            //{
            //    var cart = Request.Cookies["Cart"] != null
            //        ? JsonConvert.DeserializeObject<List<CartItem>>(Request.Cookies["Cart"])
            //        : new List<CartItem>();
            //    var existingCartItem = cart.FirstOrDefault(c => c.ProductId == cartItem.ProductId && c.ProductSizeId == cartItem.ProductSizeId);
            //    var p = await _context.Products.FirstOrDefaultAsync(_ => _.Id == cartItem.ProductId);
            //    if (existingCartItem != null)
            //    {
            //        existingCartItem.Quantity += cartItem.Quantity;
            //        existingCartItem.Amount = p.Price * existingCartItem.Quantity;
            //    }
            //    else
            //    {
            //        cartItem.Amount = p.Price * cartItem.Quantity;
            //        cart.Add(cartItem);
            //    }
            //    var total = cart.Sum(item => (double)item.Amount);                
            //    var payment = JsonConvert.DeserializeObject<Payment>(Request.Cookies["Payment"]);
            //    payment.Total = total;
            //    Response.Cookies.Append("Payment", JsonConvert.SerializeObject(payment));
            //    Response.Cookies.Append("Cart", JsonConvert.SerializeObject(cart));
            //    Console.WriteLine(cart);
            //    Console.WriteLine(payment);
            //    return RedirectToAction(nameof(CartView));
            //}
            //return RedirectToAction("Index", "Shop");
        }

        [HttpGet("edit/{id}")]
        // GET: CartItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem == null)
            {
                return NotFound();
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", cartItem.OrderId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", cartItem.ProductId);
            ViewData["ProductSizeId"] = new SelectList(_context.ProductSize, "Id", "Size", cartItem.ProductSizeId);
            return View(cartItem);
        }
        
        // POST: CartItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrderId,ProductId,ProductSizeId,Quantity,Amount")] CartItem cartItem)
        {
            if (id != cartItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == cartItem.ProductId);
                    cartItem.Amount = product.Price * cartItem.Quantity;

                    _context.Update(cartItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartItemExists(cartItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(CartView));
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Name", cartItem.OrderId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", cartItem.ProductId);
            ViewData["ProductSizeId"] = new SelectList(_context.ProductSize, "Id", "Size", cartItem.ProductSizeId);
            return View(cartItem);
        }
        
        [HttpPost("update-cart")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCart(List<CartItem> CartItems)
        {
            var cart = Request.Cookies["Cart"] != null
                    ? JsonConvert.DeserializeObject<List<CartItem>>(Request.Cookies["Cart"])
                    : new List<CartItem>();
            if (ModelState.IsValid)
            {
                foreach (var cartItem in CartItems)
                {
                    var existingCartItem = await _context.CartItems.FindAsync(cartItem.Id);
                    if (existingCartItem != null)
                    {
                        existingCartItem.Quantity = cartItem.Quantity;
                        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == cartItem.ProductId);
                        existingCartItem.Amount = product.Price * existingCartItem.Quantity;

                        _context.Update(existingCartItem);
                    }
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(CartView));
            }

            // Handle invalid model state if needed.
            return View(nameof(CartView), CartItems);
        }

        [HttpGet("delete/{id}")]
        // GET: CartItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cartItem = await _context.CartItems
                .Include(c => c.Order)
                .Include(c => c.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cartItem == null)
            {
                return NotFound();
            }

            return View(cartItem);
        }
        
        // POST: CartItems/Delete/5
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("item-delete-view/{id}")]
        public async Task<IActionResult> DeleteViewHandler(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(CartView));
        }

        private bool CartItemExists(int id)
        {
            return _context.CartItems.Any(e => e.Id == id);
        }
    }
}
