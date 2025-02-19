﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NhomBroccoli.Data.Entities
{
    [Table("Shipments")]
    public class Shipment
    {
        [Key]
        public int Id { get; set; }
        public int? OrderId { get; set; }
        [MaxLength]
        public string? Address { get; set; }
        public DateOnly? DeliveryDate { get; set; }        
        public Order? Order { get; set; }
    }
}
