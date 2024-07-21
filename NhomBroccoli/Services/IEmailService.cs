using NhomBroccoli.Models;

namespace NhomBroccoli.Services
{
    public interface IEmailService
    {
        public Task SendEmailAsync(CartItemsPaymentUser model);
    }
}
