using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SAIMRS.Domain.Entities;

namespace SAIMRS.Infrastructure.Persistence.Configurations;

internal sealed class AdminAccountConfiguration : IEntityTypeConfiguration<AdminAccount>
{
    public void Configure(EntityTypeBuilder<AdminAccount> builder)
    {
        builder.ToTable("AdminAccounts");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FirstName).HasMaxLength(60).IsRequired();
        builder.Property(a => a.LastName).HasMaxLength(60).IsRequired();
        builder.Property(a => a.Username).HasMaxLength(50).IsRequired().UseCollation("NOCASE");
        builder.Property(a => a.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(a => a.SecurityAnswerHash).HasMaxLength(500).IsRequired();
        builder.Property(a => a.ProfilePhotoFileName).HasMaxLength(200);

        builder.HasIndex(a => a.Username).IsUnique();
    }
}
