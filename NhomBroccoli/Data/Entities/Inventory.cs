using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NhomBroccoli.Data.Entities
{
    [Table("Inventories")]
    public class Inventory
    {
        [Key]        
        public int Id { get; set; }
        public int? ProductId { get; set; }        
        public int? Quantity { get; set; } = 0;
        public Product? Product { get; set; }
    }
}
