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
using NhomBroccoli.Data.Context;
using NhomBroccoli.Data.Entities;
using NhomBroccoli.Models;

namespace NhomBroccoli.Controllers
{    
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
            var storeContext = _context.CartItems.Include(c => c.Order).Include(c => c.Product);
            return View(await storeContext.ToListAsync());
        }

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
            foreach(var cartItem in cartItems)
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
        }

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

        // GET: CartItems/Create
        public IActionResult Create()
        {
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Name");
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name");
            return View();
        }

        // POST: CartItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
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

        [HttpPost]
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

                var existingCartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.ProductId == cartItem.ProductId && c.ProductSizeId == cartItem.ProductSizeId && c.OrderId == existingOrder.Id);                                

                if (existingCartItem == null)
                {
                    var product = await _context.Products.FirstOrDefaultAsync(c => c.Id == cartItem.ProductId);    
                    
                    cartItem.Amount = product.Price * cartItem.Quantity;

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
        }

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
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Name", cartItem.OrderId);
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", cartItem.ProductId);
            return View(cartItem);
        }

        // POST: CartItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrderId,ProductId,Quantity,Amount")] CartItem cartItem)
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
            return View(cartItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCart(List<CartItem> CartItems)
        {
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
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(CartView));
        }
           
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
