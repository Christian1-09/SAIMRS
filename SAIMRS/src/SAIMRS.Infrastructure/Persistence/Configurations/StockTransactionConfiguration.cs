using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAIMRS.Domain.Entities;

namespace SAIMRS.Infrastructure.Persistence.Configurations;

internal sealed class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> builder)
    {
        builder.ToTable("StockTransactions");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.ReferenceNo).HasMaxLength(50).IsRequired();
        builder.Property(t => t.Notes).HasMaxLength(500);
        builder.Property(t => t.PerformedBy).HasMaxLength(120).IsRequired();

        builder.HasOne(t => t.Supplier)
            .WithMany()
            .HasForeignKey(t => t.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Beneficiary)
            .WithMany()
            .HasForeignKey(t => t.BeneficiaryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.TransactionDate);
        builder.HasIndex(t => t.Type);
    }
}
