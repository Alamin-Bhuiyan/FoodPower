using FoodPower.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodPower.Data.Configurations;

public class PaymentAllocationConfiguration : IEntityTypeConfiguration<PaymentAllocation>
{
    public void Configure(EntityTypeBuilder<PaymentAllocation> builder)
    {
        builder.ToTable("fp_payment_allocations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.PaymentId).HasColumnName("payment_id");
        builder.Property(x => x.BeneficiaryUserId).HasColumnName("beneficiary_user_id");
        builder.Property(x => x.Days).HasColumnName("days");
        builder.Property(x => x.Amount).HasColumnName("amount").HasPrecision(18, 2);

        builder.HasOne(x => x.Beneficiary)
            .WithMany()
            .HasForeignKey(x => x.BeneficiaryUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
