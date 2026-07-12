using FoodPower.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodPower.Data.Configurations;

public class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("fp_settings");

        builder.HasKey(x => x.Key);

        builder.Property(x => x.Key).HasColumnName("key").HasMaxLength(100);
        builder.Property(x => x.Value).HasColumnName("value").HasMaxLength(500).IsRequired();
    }
}
