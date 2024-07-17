using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NhomBroccoli.Data.Entities
{
    [Table("CartItems")]
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        public int? OrderId { get; set; }
        public int? ProductId { get; set; }
        [Required]
        public int? ProductSizeId { get; set; }
        public int? Quantity { get; set; }
        public double? Amount { get; set; }
        public Order? Order { get; set; }
        public Product? Product { get; set; }
        public ProductSize? ProductSize { get; set; }
    }
}
