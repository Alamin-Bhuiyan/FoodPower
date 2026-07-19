using FoodPower.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodPower.Data.Configurations;

public class PushSubscriptionConfiguration : IEntityTypeConfiguration<PushSubscription>
{
    public void Configure(EntityTypeBuilder<PushSubscription> builder)
    {
        builder.ToTable("fp_push_subscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.Endpoint)
            .HasColumnName("endpoint")
            .HasMaxLength(500)
            .IsUnicode(true)
            .IsRequired();
        builder.Property(x => x.P256dh)
            .HasColumnName("p256dh")
            .HasMaxLength(200)
            .IsUnicode(true)
            .IsRequired();
        builder.Property(x => x.Auth)
            .HasColumnName("auth")
            .HasMaxLength(200)
            .IsUnicode(true)
            .IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Endpoint).IsUnique();
    }
}
