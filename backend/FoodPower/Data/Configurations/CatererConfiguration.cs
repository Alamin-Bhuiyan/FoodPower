using FoodPower.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodPower.Data.Configurations;

public class CatererConfiguration : IEntityTypeConfiguration<Caterer>
{
    public void Configure(EntityTypeBuilder<Caterer> builder)
    {
        builder.ToTable("fp_caterers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(30);
        builder.Property(x => x.PricePerLunch).HasColumnName("price_per_lunch").HasPrecision(18, 2);
        builder.Property(x => x.IsActive).HasColumnName("is_active");

        builder.HasMany(x => x.MenuItems)
            .WithOne(m => m.Caterer)
            .HasForeignKey(m => m.CatererId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
