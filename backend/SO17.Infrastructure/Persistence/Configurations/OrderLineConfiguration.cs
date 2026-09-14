using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SO17.Domain.Entities;
namespace SO17.Infrastructure.Persistence.Configurations;

public sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.Property(x => x.Title).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Sku).HasMaxLength(50).IsRequired();
        builder.Property(x => x.UnitPrice).HasPrecision(10, 2);
        builder.HasIndex(x => new { x.OrderId, x.ItemId }).IsUnique();
        builder.HasOne(x => x.Item).WithMany().HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Restrict);
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_OrderLines_Quantity", "[Quantity] > 0 AND [Quantity] <= 1000000");
            table.HasCheckConstraint("CK_OrderLines_Price", "[UnitPrice] > 0");
        });
    }
}
