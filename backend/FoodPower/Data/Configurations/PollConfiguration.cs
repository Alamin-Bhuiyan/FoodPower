using FoodPower.Domain.Entities;
using FoodPower.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodPower.Data.Configurations;

public class PollConfiguration : IEntityTypeConfiguration<Poll>
{
    public void Configure(EntityTypeBuilder<Poll> builder)
    {
        builder.ToTable("fp_polls");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.LunchDate).HasColumnName("lunch_date").HasColumnType("date");
        builder.Property(x => x.CatererId).HasColumnName("caterer_id");
        builder.Property(x => x.PricePerLunch).HasColumnName("price_per_lunch").HasPrecision(18, 2);
        builder.Property(x => x.CutoffAt).HasColumnName("cutoff_at");
        builder.Property(x => x.Type)
            .HasColumnName("poll_type")
            .HasConversion<int>()
            .HasDefaultValue(PollType.Lunch);
        builder.Property(x => x.Status).HasColumnName("status");
        builder.Property(x => x.ShareToken).HasColumnName("share_token");
        builder.Property(x => x.Question).HasColumnName("question").HasMaxLength(300).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.CreatedById).HasColumnName("created_by_id");

        builder.Ignore(x => x.IsCutoffPassed);

        builder.HasIndex(x => x.ShareToken).IsUnique();

        builder.HasOne(x => x.Caterer)
            .WithMany()
            .HasForeignKey(x => x.CatererId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Options)
            .WithOne(o => o.Poll)
            .HasForeignKey(o => o.PollId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
