using System.Collections.Generic;

namespace FoodPower.Contracts.Requests.Payments;

public class PaymentAllocationRequest
{
    public int beneficiary_user_id { get; set; }
    public int days { get; set; }
}

public class CreatePaymentRequest
{
    public string? screenshot { get; set; }
    public string? note { get; set; }
    public string? payment_method { get; set; }
    public List<PaymentAllocationRequest> allocations { get; set; } = [];
}
