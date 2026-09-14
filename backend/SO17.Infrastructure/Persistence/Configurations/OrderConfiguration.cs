using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SO17.Domain.Entities;
using SO17.Infrastructure.Identity;
namespace SO17.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(x => x.Number).HasMaxLength(32).IsRequired();
        builder.HasIndex(x => x.Number).IsUnique();
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.Status);
        builder.Property(x => x.CustomerName).HasMaxLength(161).IsRequired();
        builder.Property(x => x.CustomerPhone).HasMaxLength(16).IsRequired();
        builder.Property(x => x.CreatedById).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.Ignore(x => x.Total);
        builder.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.ToTable(table => table.HasCheckConstraint("CK_Orders_Status",
            "[Status] IN ('Pending', 'Processing', 'Completed', 'Cancelled')"));
    }
}
