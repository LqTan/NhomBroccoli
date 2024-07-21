using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using NhomBroccoli.Models;

namespace NhomBroccoli.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IRazorViewToStringRenderer _razorViewToStringRenderer;
        private readonly IWebHostEnvironment _env;
        public EmailService(IConfiguration configuration, IRazorViewToStringRenderer razorViewToStringRenderer, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _razorViewToStringRenderer = razorViewToStringRenderer;
            _env = env;
        }
        public async Task SendEmailAsync(CartItemsPaymentUser model)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");

            var htmlContent = await _razorViewToStringRenderer.RenderViewToStringAsync("Email/SendEmail", model);

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(emailSettings["SenderEmail"]));
            email.To.Add(MailboxAddress.Parse(model.User.Email));
            email.Subject = "Broccoli Notification";

            //var bodyBuilder = new BodyBuilder { HtmlBody = htmlContent };
            //foreach (var item in model.CartItems)
            //{
            //    var image = item.Product.ProductImages.ElementAt(0).Img;
            //    var newImage = image.Replace("~/Assets/img/product/", "");
            //    //newImage = Path.GetFileNameWithoutExtension(newImage);
            //    var assetsPath = Path.Combine(_env.ContentRootPath, "wwwroot","Assets", "img", "product");
            //    var imagePath = Path.Combine(assetsPath, newImage);
            //    if (File.Exists(imagePath))
            //    {
            //        var cid = Guid.NewGuid().ToString();
            //        bodyBuilder.LinkedResources.Add(imagePath).ContentId = cid;

            //        htmlContent = htmlContent.Replace(image, $"cid:{cid}");
            //    }
            //    else
            //    {
            //        // Log or handle the error when the image does not exist
            //        throw new FileNotFoundException($"The file {imagePath} does not exist.");
            //    }
            //}

            //email.Body = bodyBuilder.ToMessageBody();

            foreach (var item in model.CartItems)
            {
                htmlContent = htmlContent.Replace("~", "https://broccolistore.somee.com");
            }
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlContent };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]), MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(emailSettings["Username"], emailSettings["Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
