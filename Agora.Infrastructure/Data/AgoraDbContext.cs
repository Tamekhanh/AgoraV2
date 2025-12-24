using Agora.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Agora.Infrastructure.Data;

public class AgoraDbContext : DbContext
{
    public AgoraDbContext(DbContextOptions<AgoraDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ImageFile> ImageFiles => Set<ImageFile>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ================================
        // 1. TABLE NAMES
        // ================================
        builder.Entity<Product>().ToTable("Product");
        builder.Entity<Category>().ToTable("Category");
        builder.Entity<Shop>().ToTable("Shop");
        builder.Entity<User>().ToTable("User");
        builder.Entity<Cart>().ToTable("Cart");
        builder.Entity<CartItem>().ToTable("CartItem");
        builder.Entity<Order>().ToTable("Order");
        builder.Entity<OrderItem>().ToTable("OrderItem");
        builder.Entity<Payment>().ToTable("Payment");
        builder.Entity<Payment>()
            .HasIndex(p => p.IdempotencyKey)
            .IsUnique();
        builder.Entity<ImageFile>().ToTable("ImageFile");
        builder.Entity<ProcessedMessage>().ToTable("ProcessedMessage")
            .HasIndex(p => new { p.MessageId, p.ConsumerName })
            .IsUnique();

        // ================================
        // 2. SPECIAL COLUMN CONFIG
        // ================================
        builder.Entity<ImageFile>()
            .Property(i => i.Data)
            .HasColumnName("ImageFile") // map sang cột SQL
            .HasColumnType("varbinary(max)");

        // ================================
        // 3. RELATIONSHIPS
        // ================================

        // ---------- Product ----------
        builder.Entity<Product>(entity =>
        {
            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId);

            entity.HasOne(p => p.Shop)
                  .WithMany(s => s.Products)
                  .HasForeignKey(p => p.ShopId);

            entity.HasOne(p => p.ImageFile)
                  .WithMany(i => i.Products)
                  .HasForeignKey(p => p.ImageId);
        });

        // ---------- Shop ----------
        builder.Entity<Shop>(entity =>
        {
            entity.HasOne(s => s.User)
                  .WithMany(u => u.Shops)
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.NoAction);
            // tránh vòng lặp cascade

            entity.HasOne(s => s.ImageFile)
                  .WithMany(i => i.Shops)
                  .HasForeignKey(s => s.ImageId);
        });

        // ---------- User ----------
        builder.Entity<User>(entity =>
        {
            entity.HasOne(u => u.ImageFile)
                  .WithMany(i => i.Users)
                  .HasForeignKey(u => u.ImageId);
        });

        // ---------- Cart ----------
        builder.Entity<Cart>(entity =>
        {
            entity.HasOne(c => c.User)
                  .WithMany(u => u.Carts)
                  .HasForeignKey(c => c.UserId);
        });

        // ---------- CartItem ----------
        builder.Entity<CartItem>(entity =>
        {
            entity.HasOne(ci => ci.Cart)
                  .WithMany(c => c.CartItems)
                  .HasForeignKey(ci => ci.CartId);

            entity.HasOne(ci => ci.Product)
                  .WithMany()
                  .HasForeignKey(ci => ci.ProductId);
        });

        // ---------- Order ----------
        builder.Entity<Order>(entity =>
        {
            entity.HasOne(o => o.User)
                  .WithMany(u => u.Orders)
                  .HasForeignKey(o => o.UserId);
        });

        // ---------- OrderItem ----------
        builder.Entity<OrderItem>(entity =>
        {
            entity.HasOne(oi => oi.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(oi => oi.OrderId);

            entity.HasOne(oi => oi.Product)
                  .WithMany()
                  .HasForeignKey(oi => oi.ProductId);
        });

        // ---------- Payment ----------
        builder.Entity<Payment>(entity =>
        {
            entity.HasOne(p => p.Order)
                  .WithMany(o => o.Payments)
                  .HasForeignKey(p => p.OrderId);
        });
    }
}
