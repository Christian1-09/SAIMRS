using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAIMRS.Domain.Entities;

namespace SAIMRS.Infrastructure.Persistence.Configurations;

internal sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(120).IsRequired().UseCollation("NOCASE");
        builder.Property(s => s.ContactPerson).HasMaxLength(120);
        builder.Property(s => s.Phone).HasMaxLength(30);
        builder.Property(s => s.ItemsSupplied).HasMaxLength(300);
        builder.HasIndex(s => s.Name).IsUnique();
        builder.HasIndex(s => s.IsArchived);
    }
}
