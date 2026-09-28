using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAIMRS.Domain.Entities;

namespace SAIMRS.Infrastructure.Persistence.Configurations;

internal sealed class PerishableBatchConfiguration : IEntityTypeConfiguration<PerishableBatch>
{
    public void Configure(EntityTypeBuilder<PerishableBatch> builder)
    {
        builder.ToTable("PerishableBatches");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ItemName).HasMaxLength(150).IsRequired();
        builder.Property(p => p.BatchNo).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Unit).HasMaxLength(20).IsRequired();
        builder.Property(p => p.Notes).HasMaxLength(500);

        builder.HasIndex(p => p.ExpiryDate);
    }
}
