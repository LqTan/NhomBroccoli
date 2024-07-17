using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NhomBroccoli.Data.Entities
{
    [Table("SubCategories")]
    public class SubCategory
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
