using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NhomBroccoli.Data.Entities
{
    [Table("Reviews")]
    public class Review
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int? ProductId { get; set; }
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public DateOnly? ReviewDate { get; set; }        
        public ApplicationUser? ApplicationUser { get; set; }
        public Product? Product { get; set; }
    }
}
