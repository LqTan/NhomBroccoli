using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NhomBroccoli.Data.Entities
{
    [Table("Orders")]
    public class Order
    {
        [Key]       
        public int Id { get; set; }
        public string? UserId { get; set; }        
        public DateOnly? OrderDate { get; set; }
        public int? OrderStatus { get; set; } = 0;
        public ApplicationUser? OrderUser { get; set; }
        public Payment? Payment { get; set; }
        public Shipment? Shipment { get; set; }
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();       
    }
}
