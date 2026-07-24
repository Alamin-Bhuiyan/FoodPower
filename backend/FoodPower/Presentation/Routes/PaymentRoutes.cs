namespace FoodPower.Presentation.Routes;

public class PaymentRoutes
{
    public const string CreatePaymentMethod = "POST";
    public const string CreatePaymentName = "foodpower.payments.create";
    public const string CreatePaymentTemplate = "/api/payments";

    public const string GetMyPaymentsMethod = "GET";
    public const string GetMyPaymentsName = "foodpower.payments.my";
    public const string GetMyPaymentsTemplate = "/api/payments/my";

    public const string GetPaymentsMethod = "GET";
    public const string GetPaymentsName = "foodpower.payments.list";
    public const string GetPaymentsTemplate = "/api/payments";

    public const string CancelPaymentMethod = "DELETE";
    public const string CancelPaymentName = "foodpower.payments.cancel";
    public const string CancelPaymentTemplate = "/api/payments/{id}";

    public const string ApprovePaymentMethod = "POST";
    public const string ApprovePaymentName = "foodpower.payments.approve";
    public const string ApprovePaymentTemplate = "/api/payments/{id}/approve";

    public const string RejectPaymentMethod = "POST";
    public const string RejectPaymentName = "foodpower.payments.reject";
    public const string RejectPaymentTemplate = "/api/payments/{id}/reject";

    public const string RemindDueMethod = "POST";
    public const string RemindDueName = "foodpower.payments.remind_due";
    public const string RemindDueTemplate = "/api/payments/remind-due";
}
