using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NhomBroccoli.Data.Entities
{
    [Table("Products")]
    public class Product
    {
        [Key]        
        public int Id { get; set; }        
        public string? Name { get; set; }        
        public double? Price { get; set; }
        public double? Rate { get; set; }
        public int? SubCategoryId { get; set; }
        public int? BrandId { get; set; }
        [MaxLength]
        public string? Description { get; set; }
        public SubCategory? SubCategory { get; set; }
        public Brand? Brand { get; set; }
        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();
    }
}
