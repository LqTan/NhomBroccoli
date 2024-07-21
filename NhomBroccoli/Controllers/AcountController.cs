using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NhomBroccoli.Data.Context;
using NhomBroccoli.Data.Entities;
using NhomBroccoli.Helpers;
using NhomBroccoli.Models;
using OfficeOpenXml;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace NhomBroccoli.Controllers
{
    public class AcountController : Controller
    {
        private readonly StoreContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AcountController(
            StoreContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager
            )
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _userManager.Users.ToListAsync());
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        public IActionResult Create()
        {
            return View();
        }

        // POST: ApplicationUser/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserName,Email,FirstName,LastName,Password,Address,Phone")] ApplicationUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address,
                    PhoneNumber = model.Phone
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //if (!await _roleManager.RoleExistsAsync(AppRole.Customer))
                    //{
                    //    await _roleManager.CreateAsync(new IdentityRole(AppRole.Customer));
                    //}
                    //await _userManager.AddToRoleAsync(user, AppRole.Customer);
                    return RedirectToAction(nameof(Index));
                }
                AddErrors(result);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp([Bind("UserName,Email,FirstName,LastName,Password,Address,Phone")] ApplicationUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address,
                    PhoneNumber = model.Phone
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {                    
                    return RedirectToAction("Index", "Login");
                }
                AddErrors(result);
            }

            return RedirectToAction("Index", "Register");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn([Bind("Email,Password")] SignInModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
                if (user == null || !passwordValid)
                {
                    return View(model);
                }
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, model.Email),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
                var authenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddDays(1),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authenKey, SecurityAlgorithms.HmacSha512Signature)
                );
                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                HttpContext.Response.Cookies.Append("SessionToken", tokenString, new CookieOptions
                {
                    HttpOnly = true, // Ngăn chặn truy cập cookie từ script phía client.
                    Secure = true, // Đặt là true nếu sử dụng HTTPS.
                    //SameSite = SameSiteMode.Strict // Điều chỉnh theo nhu cầu của bạn.
                                                   // Không thiết lập Expires để tạo session cookie.
                });

                var existingOrder = await _context.Orders.FirstOrDefaultAsync(o => o.UserId == user.Id && o.OrderStatus == 0);
                if (existingOrder == null)
                {
                    var order = new Order
                    {
                        UserId = user.Id,
                        OrderDate = DateOnly.FromDateTime(DateTime.Now),
                        OrderStatus = 0
                    };
                    _context.Add(order);
                    await _context.SaveChangesAsync();
                    var payment = new Payment
                    {
                        OrderId = order.Id,
                        Method = "Unknown",
                        Total = 0,
                        PaymentDate = DateOnly.FromDateTime(DateTime.Now),
                        Status = 0
                    };
                    _context.Add(payment);
                    await _context.SaveChangesAsync();
                }                

                Console.WriteLine(tokenString.ToString());

                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Index", "Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var token = HttpContext.Request.Cookies["SessionToken"];
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            var emailClaim = jsonToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            var email = emailClaim.Value;

            var user = await _userManager.FindByEmailAsync(email);
            var order = await _context.Orders.FirstOrDefaultAsync(o => (o.UserId == user.Id && o.OrderStatus == 0));
            var payment = await _context.Payments.FirstOrDefaultAsync(py => (py.OrderId == order.Id && py.Status == 0));

            if (order != null && payment != null)
            {
                _context.Orders.Remove(order);
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
            }

            HttpContext.Response.Cookies.Delete("SessionToken");
            return RedirectToAction("Index", "Home");
        }

        // GET: ApplicationUser/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ApplicationUserViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                Phone = user.PhoneNumber
            };
            return View(model);
        }

        // POST: ApplicationUser/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("UserName,Email,FirstName,LastName,Address,Phone")] ApplicationUserViewModel model)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            
            user.UserName = model.UserName;
            user.Email = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Address = model.Address;
            user.PhoneNumber = model.Phone;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: ApplicationUser/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: ApplicationUser/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
                AddErrors(result);
            }
            return RedirectToAction(nameof(Index));
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        public async Task<IActionResult> ExportUsersToCsv()
        {
            var users = await _userManager.Users.ToListAsync(); // Giả sử trả về danh sách người dùng

            //using (var package = new ExcelPackage())
            //{
            //    var worksheet = package.Workbook.Worksheets.Add("Users");
            //    worksheet.Cells[1, 1].Value = "Email";

            //    for (int i = 0; i < users.Count; i++)
            //    {
            //        worksheet.Cells[i + 2, 1].Value = users[i].Email;
            //    }

            //    var stream = new MemoryStream(package.GetAsByteArray());
            //    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "users.xlsx");
            //}            

            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Email");

            foreach (var user in users)
            {
                csvBuilder.AppendLine(user.Email);
            }

            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", "users.csv");
        }
    }
}
