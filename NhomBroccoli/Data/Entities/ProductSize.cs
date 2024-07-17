using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NhomBroccoli.Data.Entities
{
    [Table("ProductSizes")]
    public class ProductSize
    {
        [Key]
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public string? Size { get; set; }
        public Product? Product { get; set; }
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
