using NhomBroccoli.Data.Entities;

namespace NhomBroccoli.Models
{
    public class PaymentAndCartItems
    {
        public List<CartItem> CartItems { get; set; }
        public Payment Payment { get; set; }
    }
}
