using FoodPower.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodPower.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("fp_payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.SubmittedById).HasColumnName("submitted_by_id");
        builder.Property(x => x.TotalAmount).HasColumnName("total_amount").HasPrecision(18, 2);
        builder.Property(x => x.ScreenshotPath).HasColumnName("screenshot_path").HasMaxLength(500).IsRequired();
        builder.Property(x => x.Note).HasColumnName("note").HasMaxLength(500);
        builder.Property(x => x.Method).HasColumnName("payment_method").HasDefaultValue(Domain.Enums.PaymentMethod.Bkash);
        builder.Property(x => x.Status).HasColumnName("status");
        builder.Property(x => x.ReviewedById).HasColumnName("reviewed_by_id");
        builder.Property(x => x.ReviewedAt).HasColumnName("reviewed_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasOne(x => x.SubmittedBy)
            .WithMany()
            .HasForeignKey(x => x.SubmittedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReviewedBy)
            .WithMany()
            .HasForeignKey(x => x.ReviewedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Allocations)
            .WithOne(a => a.Payment)
            .HasForeignKey(a => a.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
