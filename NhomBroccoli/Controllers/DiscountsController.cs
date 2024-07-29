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

namespace NhomBroccoli.Controllers
{
    [Route("discount")]
    public class DiscountsController : Controller
    {
        private readonly StoreContext _context;

        public DiscountsController(StoreContext context)
        {
            _context = context;
        }

        // GET: Discounts
        public async Task<IActionResult> Index()
        {
            return View(await _context.Discounts.ToListAsync());
        }

        [HttpGet("detail/{id}")]
        // GET: Discounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discount = await _context.Discounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (discount == null)
            {
                return NotFound();
            }

            return View(discount);
        }

        [HttpGet("create")]
        // GET: Discounts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Discounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DiscountCode,DiscountEffectiveStart,DiscountEffectiveEnd,DiscountPercent")] Discount discount)
        {
            if (ModelState.IsValid)
            {
                _context.Add(discount);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(discount);
        }

        [HttpGet("edit/{id}")]
        // GET: Discounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
            {
                return NotFound();
            }
            return View(discount);
        }

        // POST: Discounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DiscountCode,DiscountEffectiveStart,DiscountEffectiveEnd,DiscountPercent")] Discount discount)
        {
            if (id != discount.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(discount);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DiscountExists(discount.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(discount);
        }

        [HttpGet("delete/{id}")]
        // GET: Discounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discount = await _context.Discounts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (discount == null)
            {
                return NotFound();
            }

            return View(discount);
        }

        // POST: Discounts/Delete/5
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount != null)
            {
                _context.Discounts.Remove(discount);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("check-discount")]
        public async Task<IActionResult> CheckDiscount(string code)
        {
            var discount = await _context.Discounts.FirstOrDefaultAsync(d => d.DiscountCode == code);
            return View(discount);
        }

        [HttpPost("apply-discount")]
        public async Task<IActionResult> ApplyDiscount(int OrderId, int PaymentId, string CouponCode)
        {
            var existingOrder = await _context.Orders.FirstOrDefaultAsync(o => o.Id == OrderId);
            var existingPayment = await _context.Payments.FirstOrDefaultAsync(pm => pm.Id == PaymentId);
            if (existingOrder != null)
            {
                var discount = await _context.Discounts.FirstOrDefaultAsync(d => d.DiscountCode == CouponCode);
                if (discount != null)
                {
                    if (discount.DiscountEffectiveEnd >= DateOnly.FromDateTime(DateTime.Now))
                    {
                        if (existingPayment.DiscountId == null)
                        {
                            existingPayment.DiscountId = discount.Id;
                        }
                    }
                }
                else
                {
                    return RedirectToAction("CartView", "CartItems");
                }
            }
            _context.Update(existingPayment);
            await _context.SaveChangesAsync();
            return RedirectToAction("CartView", "CartItems");
        }

        private bool DiscountExists(int id)
        {
            return _context.Discounts.Any(e => e.Id == id);
        }
    }
}
