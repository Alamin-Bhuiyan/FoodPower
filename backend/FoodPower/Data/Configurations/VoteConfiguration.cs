using FoodPower.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodPower.Data.Configurations;

public class VoteConfiguration : IEntityTypeConfiguration<Vote>
{
    public void Configure(EntityTypeBuilder<Vote> builder)
    {
        builder.ToTable("fp_votes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PollId).HasColumnName("poll_id");
        builder.Property(x => x.PollOptionId).HasColumnName("poll_option_id");
        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.IsManual).HasColumnName("is_manual");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.CreatedById).HasColumnName("created_by_id");

        builder.HasIndex(x => new { x.PollId, x.UserId }).IsUnique();

        builder.HasOne(x => x.Poll)
            .WithMany()
            .HasForeignKey(x => x.PollId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Option)
            .WithMany()
            .HasForeignKey(x => x.PollOptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
