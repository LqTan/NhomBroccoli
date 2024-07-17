using NhomBroccoli.Data.Entities;

namespace NhomBroccoli.Models
{
    public class CartItemsPaymentUser
    {
        public List<CartItem> CartItems { get; set; }
        public Payment Payment { get; set; }
        public ApplicationUser User { get; set; }
    }
}
