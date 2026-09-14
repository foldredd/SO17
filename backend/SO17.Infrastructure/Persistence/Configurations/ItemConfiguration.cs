using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SO17.Domain.Entities;
namespace SO17.Infrastructure.Persistence.Configurations;

public sealed class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.Property(x => x.Title).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Sku).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Price).HasPrecision(10, 2);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasIndex(x => x.Sku).IsUnique();
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Items_Quantity", "[Quantity] >= 0 AND [Quantity] <= 1000000");
            table.HasCheckConstraint("CK_Items_Price", "[Price] > 0");
        });
    }
}
