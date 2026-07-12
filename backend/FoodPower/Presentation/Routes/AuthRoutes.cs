namespace FoodPower.Presentation.Routes;

public class AuthRoutes
{
    public const string RegisterMethod = "POST";
    public const string RegisterName = "foodpower.auth.register";
    public const string RegisterTemplate = "/api/auth/register";

    public const string VerifyOtpMethod = "POST";
    public const string VerifyOtpName = "foodpower.auth.verify_otp";
    public const string VerifyOtpTemplate = "/api/auth/verify-otp";

    public const string ResendOtpMethod = "POST";
    public const string ResendOtpName = "foodpower.auth.resend_otp";
    public const string ResendOtpTemplate = "/api/auth/resend-otp";

    public const string LoginMethod = "POST";
    public const string LoginName = "foodpower.auth.login";
    public const string LoginTemplate = "/api/auth/login";

    public const string ForgetPasswordMethod = "POST";
    public const string ForgetPasswordName = "foodpower.auth.forget_password";
    public const string ForgetPasswordTemplate = "/api/auth/forget-password";

    public const string ResetPasswordMethod = "POST";
    public const string ResetPasswordName = "foodpower.auth.reset_password";
    public const string ResetPasswordTemplate = "/api/auth/reset-password";

    public const string ChangePasswordMethod = "POST";
    public const string ChangePasswordName = "foodpower.auth.change_password";
    public const string ChangePasswordTemplate = "/api/auth/change-password";
}
