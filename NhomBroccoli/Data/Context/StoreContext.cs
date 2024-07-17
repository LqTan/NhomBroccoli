using NhomBroccoli.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace NhomBroccoli.Data.Context
{
    public class StoreContext : IdentityDbContext<ApplicationUser>
    {
        public StoreContext(DbContextOptions<StoreContext> options) : base(options) { }             
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Review> Reviews { get; set; } 
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Shipment> Shipments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //builder.Entity<Order>()
            //    .HasOne(o => o.Payment)
            //    .WithOne(p => p.Order)
            //    .HasForeignKey<Payment>(p => p.OrderId)
            //    .OnDelete(DeleteBehavior.SetNull);
            builder.Entity<SubCategory>(entity =>
            {
                entity.HasOne(sub => sub.Category)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(sub => sub.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
            });
            builder.Entity<Product>(entity =>
            {
                entity.HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.SubCategory)
                .WithMany(sub => sub.Products)
                .HasForeignKey(p => p.SubCategoryId)
                .OnDelete(DeleteBehavior.SetNull);
            });
            builder.Entity<ProductImage>(entity =>
            {
                entity.HasOne(img => img.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(img => img.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
            });
            builder.Entity<Inventory>(entity =>
            {
                entity.HasOne(i => i.Product)
                .WithMany(p => p.Inventories)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
            });
            builder.Entity<CartItem>(entity =>
            {
                entity.HasOne(cart => cart.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(cart => cart.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(cart => cart.Order)
                .WithMany(o => o.CartItems)
                .HasForeignKey(cart => cart.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(cart => cart.ProductSize)
                .WithMany(ps => ps.CartItems)
                .HasForeignKey(cart => cart.ProductSizeId)
                .OnDelete(DeleteBehavior.SetNull);
            });
            builder.Entity<Review>(entity =>
            {
                entity.HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(r => r.ApplicationUser)
                .WithMany(user => user.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.SetNull);
            });
            builder.Entity<Payment>(entity =>
            {
                entity.HasOne(pay => pay.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(pay => pay.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(pay => pay.Discount)
                .WithMany(d => d.Payments)
                .HasForeignKey(pay => pay.DiscountId)
                .OnDelete(DeleteBehavior.SetNull);
            });
            builder.Entity<Order>(entity =>
            {
                entity.HasOne(o => o.OrderUser)
                .WithMany(user => user.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.SetNull);
            });
            builder.Entity<ProductSize>(entity =>
            {
                entity.HasOne(ps => ps.Product)
                .WithMany(p => p.ProductSizes)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
            });
            builder.Entity<Shipment>(entity =>
            {
                entity.HasOne(sm => sm.Order)
                .WithOne(o => o.Shipment)
                .HasForeignKey<Shipment>(sm => sm.OrderId)
                .OnDelete(DeleteBehavior.SetNull);
            });
        }        
        public DbSet<NhomBroccoli.Data.Entities.ProductSize> ProductSize { get; set; } = default!;
    }
}
