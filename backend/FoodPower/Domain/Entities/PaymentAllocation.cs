namespace FoodPower.Domain.Entities;

public class PaymentAllocation
{
    public int Id { get; set; }
    public int PaymentId { get; set; }
    public int BeneficiaryUserId { get; set; }
    public int Days { get; set; }
    public decimal Amount { get; set; }

    public Payment? Payment { get; set; }
    public AppUser? Beneficiary { get; set; }

    public PaymentAllocation()
    {
    }

    public PaymentAllocation(int beneficiaryUserId, int days, decimal amount)
    {
        BeneficiaryUserId = beneficiaryUserId;
        Days = days;
        Amount = amount;
    }
}
