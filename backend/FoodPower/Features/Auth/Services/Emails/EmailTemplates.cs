namespace FoodPower.Features.Auth.Services.Emails;

public static class EmailTemplates
{
    public static string OtpEmailSubject(string purpose) =>
        $"FoodPower - {purpose} verification code";

    public static string OtpEmailBody(string fullName, string code, string purpose, int lifetimeMinutes = 10) =>
        $"""
         <div style="font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto;">
             <h2 style="color: #16a34a;">FoodPower</h2>
             <p>Hi {fullName},</p>
             <p>Your {purpose.ToLowerInvariant()} verification code is:</p>
             <p style="font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #111827;">{code}</p>
             <p>This code expires in {lifetimeMinutes} minutes.</p>
             <p>If you did not request this, you can safely ignore this email.</p>
             <p style="color: #6b7280; font-size: 12px;">FoodPower - office lunch management</p>
         </div>
         """;
}
