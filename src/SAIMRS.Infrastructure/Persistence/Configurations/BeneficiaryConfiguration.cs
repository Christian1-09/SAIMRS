using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAIMRS.Domain.Entities;

namespace SAIMRS.Infrastructure.Persistence.Configurations;

internal sealed class BeneficiaryConfiguration : IEntityTypeConfiguration<Beneficiary>
{
    public void Configure(EntityTypeBuilder<Beneficiary> builder)
    {
        builder.ToTable("Beneficiaries");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Code).HasMaxLength(20).IsRequired();
        builder.Property(b => b.FullName).HasMaxLength(120).IsRequired().UseCollation("NOCASE");
        builder.Property(b => b.Barangay).HasMaxLength(80).IsRequired().UseCollation("NOCASE");
        builder.Property(b => b.ContactNumber).HasMaxLength(30);

        builder.HasIndex(b => b.Code).IsUnique();
        builder.HasIndex(b => b.Barangay);
        builder.HasIndex(b => b.IsArchived);
    }
}
