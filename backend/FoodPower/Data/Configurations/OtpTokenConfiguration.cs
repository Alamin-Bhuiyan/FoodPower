using FoodPower.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodPower.Data.Configurations;

public class OtpTokenConfiguration : IEntityTypeConfiguration<OtpToken>
{
    public void Configure(EntityTypeBuilder<OtpToken> builder)
    {
        builder.ToTable("fp_otp_tokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(6).IsRequired();
        builder.Property(x => x.Purpose).HasColumnName("purpose");
        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at");
        builder.Property(x => x.ConsumedAt).HasColumnName("consumed_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.Ignore(x => x.IsValid);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.Purpose });
    }
}
