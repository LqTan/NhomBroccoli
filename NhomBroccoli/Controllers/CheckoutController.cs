using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NhomBroccoli.Data.Context;
using NhomBroccoli.Data.Entities;
using NhomBroccoli.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using NhomBroccoli.Services;

namespace NhomBroccoli.Controllers
{
    [Route("checkout")]
    public class CheckoutController : Controller
    {
        private readonly StoreContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVnPayService _vnPayService;
        private readonly IEmailService _emailService;
        public CheckoutController(StoreContext storeContext, UserManager<ApplicationUser> userManager, IVnPayService vnPayService, IEmailService emailService)
        {
            _context = storeContext;
            _userManager = userManager;
            _vnPayService = vnPayService;
            _emailService = emailService;
        }
        public async Task<IActionResult> Index()
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

            var existingPayment = await _context.Payments
                .Include(pm => pm.Order)
                .FirstOrDefaultAsync(pm => pm.Status == 0 && pm.OrderId == existingOrder.Id);

            var cartItems = await _context.CartItems
                .Where(c => c.OrderId == existingOrder.Id)
                .Include(c => c.Order)
                .Include(c => c.ProductSize)
                .Include(c => c.Product)
                .ThenInclude(p => p.ProductImages)
                .ToListAsync();

            var cartItemsPaymentUser = new CartItemsPaymentUser
            {
                CartItems = cartItems,
                Payment = existingPayment,
                User = user
            };

            return View(cartItemsPaymentUser);
        }

        [HttpPost("complete")]
        public async Task<IActionResult> CompleteCheckout([Bind("Id,OrderId,Address,DeliveryDate")] Shipment shipment, string? PayPalId, int? PaymentId)
        {
            if (ModelState.IsValid)
            {
                var payment = await _context.Payments.FirstOrDefaultAsync(pm => pm.Id == PaymentId);
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == payment.OrderId);
                var user = await _userManager.FindByIdAsync(order.UserId);
                if (payment != null)
                {
                    if (!string.IsNullOrEmpty(PayPalId))
                    {
                        payment.Method = "Paypal";
                    }
                    else
                    {
                        payment.Method = "Cash";
                    }
                    payment.PaymentDate = shipment.DeliveryDate;
                    payment.Status = 1;
                    _context.Update(payment);
                    await _context.SaveChangesAsync();

                    order.OrderDate = shipment.DeliveryDate;
                    order.OrderStatus = 1;
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }

                var _order = new Order
                {
                    UserId = order.UserId,
                    OrderDate = DateOnly.FromDateTime(DateTime.Now),
                    OrderStatus = 0
                };
                _context.Add(_order);
                await _context.SaveChangesAsync();
                var _payment = new Payment
                {
                    OrderId = _order.Id,
                    Method = "Unknown",
                    Total = 0,
                    PaymentDate = DateOnly.FromDateTime(DateTime.Now),
                    Status = 0
                };
                _context.Add(_payment);
                await _context.SaveChangesAsync();

                _context.Add(shipment);
                await _context.SaveChangesAsync();

                var cartItems = await _context.CartItems
                    .Where(c => c.OrderId == order.Id)
                    .Include(c => c.Order)
                    .Include(c => c.ProductSize)
                    .Include(c => c.Product)
                    .ThenInclude(p => p.ProductImages)
                    .ToListAsync();

                var cartItemsPaymentUser = new CartItemsPaymentUser
                {
                    CartItems = cartItems,
                    Payment = payment,
                    User = user
                };

                _emailService.SendEmailAsync(cartItemsPaymentUser);

                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Index", "Checkout");
        }

        [HttpPost("create-vnpay-url")]
        public async Task<IActionResult> CreatePaymentUrl([Bind("Id,OrderId,Address,DeliveryDate")] Shipment shipment, int? PaymentId)
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
            var payment = await _context.Payments.FirstOrDefaultAsync(pm => pm.Id == PaymentId);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == payment.OrderId);

            //-------------------------------------------------------------------------
            

            _context.Add(shipment);
            await _context.SaveChangesAsync();

            var model = new PaymentInformationModel
            {
                OrderType = "shopping",
                Amount = (double)payment.Total,
                OrderDescription = $"VNPAY payments by {user.UserName}",
                Name = $"{order.Id},"
            };
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            return Redirect(url);
        }

        [HttpGet("vnpay-payment-callback")]
        public async Task<IActionResult> PaymentCallback()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            var orderDescription = response.OrderDescription.Split(",");
            var orderId = int.Parse(orderDescription[0]);

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            var payment = await _context.Payments.FirstOrDefaultAsync(pm => pm.OrderId == order.Id);
            var user = await _userManager.FindByIdAsync(order.UserId);

            payment.PaymentDate = DateOnly.FromDateTime(DateTime.Now);
            payment.Status = 1;
            payment.Method = response.PaymentMethod;
            _context.Update(payment);
            await _context.SaveChangesAsync();

            order.OrderDate = DateOnly.FromDateTime(DateTime.Now);
            order.OrderStatus = 1;
            _context.Update(order);
            await _context.SaveChangesAsync();

            var _order = new Order
            {
                UserId = order.UserId,
                OrderDate = DateOnly.FromDateTime(DateTime.Now),
                OrderStatus = 0
            };
            _context.Add(_order);
            await _context.SaveChangesAsync();
            var _payment = new Payment
            {
                OrderId = _order.Id,
                Method = "Unknown",
                Total = 0,
                PaymentDate = DateOnly.FromDateTime(DateTime.Now),
                Status = 0
            };
            _context.Add(_payment);
            await _context.SaveChangesAsync();

            var cartItems = await _context.CartItems
                    .Where(c => c.OrderId == order.Id)
                    .Include(c => c.Order)
                    .Include(c => c.ProductSize)
                    .Include(c => c.Product)
                    .ThenInclude(p => p.ProductImages)
                    .ToListAsync();

            var cartItemsPaymentUser = new CartItemsPaymentUser
            {
                CartItems = cartItems,
                Payment = payment,
                User = user
            };

            _emailService.SendEmailAsync(cartItemsPaymentUser);

            //return Json(response);
            return RedirectToAction("Index", "Home");
        }
    }
}
