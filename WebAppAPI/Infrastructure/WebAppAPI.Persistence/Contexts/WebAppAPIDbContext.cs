using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebAppAPI.Domain.Entities;
using WebAppAPI.Domain.Entities.Common;
using WebAppAPI.Domain.Entities.Identity;
using WebAppAPI.Domain.Enums;

namespace WebAppAPI.Persistence.Contexts
{
    public class WebAppAPIDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public WebAppAPIDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Domain.Entities.File> Files { get; set; }
        public DbSet<ProductImageFile> ProductImageFiles { get; set; }
        public DbSet<InvoiceFile> InvoiceFiles { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Basket> Baskets { get; set; }
        public DbSet<BasketItem> BasketItems { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<Endpoint> Endpoints { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Order>()
                .HasKey(b => b.Id);

            builder.Entity<Order>()
                .HasIndex(o => o.OrderCode)
                .IsUnique();

            builder.Entity<BasketItem>()
                .HasIndex(basketItem => new
                {
                    basketItem.BasketId,
                    basketItem.ProductId
                })
                .IsUnique();

            builder.Entity<Basket>()
                .HasOne(b => b.Order)
                .WithOne(o => o.Basket)
                .HasForeignKey<Order>(b => b.Id);

            builder.Entity<OrderStatus>()
                .HasData(Enum.GetValues(typeof(OrderStatusEnum))
                .Cast<OrderStatusEnum>()
                .Select(e => new OrderStatus
                {
                    Id = (int)e,
                    Name = e.ToString()
                })
            );

            builder.Entity<OrderStatusHistory>()
                .HasOne(h => h.PreviousStatus)
                .WithMany()
                .HasForeignKey(h => h.PreviousStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderStatusHistory>()
                .HasOne(h => h.NewStatus)
                .WithMany()
                .HasForeignKey(h => h.NewStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(builder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var data = ChangeTracker
                .Entries<BaseEntity>();

            foreach (var item in data)
            {
                switch (item.State)
                {
                    case EntityState.Added:
                        if (item.Entity.DateCreated == default)
                            item.Entity.DateCreated = DateTime.UtcNow;

                        if (item.Entity.DateUpdated == default)
                            item.Entity.DateUpdated = item.Entity.DateCreated;
                        break;
                    case EntityState.Modified:
                        item.Entity.DateUpdated = DateTime.UtcNow;
                        break;
                }
            }

            var identityEntries = ChangeTracker
                .Entries<IAuditableIdentityEntity>();

            foreach (var entry in identityEntries)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        if (!entry.Entity.DateCreated.HasValue || entry.Entity.DateCreated.Value == default)
                            entry.Entity.DateCreated = DateTime.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.DateUpdated = DateTime.UtcNow;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
