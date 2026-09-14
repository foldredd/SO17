using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SO17.Domain.Entities;
namespace SO17.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Surname).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(16).IsRequired();
        builder.HasIndex(x => x.Phone).IsUnique();
        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
