using FoodPower.Contracts.Requests.Auth;
using FoodPower.Features.Auth.AuthHandlers;
using Mapster;

namespace FoodPower.Presentation.Mappings;

public class AuthMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<RegisterRequest, RegisterCommand>()
            .ConstructUsing(src => new RegisterCommand(
                src.full_name,
                src.email,
                src.password
            ));

        config.NewConfig<LoginRequest, LoginCommand>()
            .ConstructUsing(src => new LoginCommand(
                src.email,
                src.password
            ));

        config.NewConfig<VerifyOtpRequest, VerifyOtpCommand>()
            .ConstructUsing(src => new VerifyOtpCommand(
                src.email,
                src.otp
            ));

        config.NewConfig<ResendOtpRequest, ResendOtpCommand>()
            .ConstructUsing(src => new ResendOtpCommand(
                src.email,
                src.purpose
            ));

        config.NewConfig<ForgetPasswordRequest, ForgetPasswordCommand>()
            .ConstructUsing(src => new ForgetPasswordCommand(
                src.email
            ));

        config.NewConfig<ResetPasswordRequest, ResetPasswordCommand>()
            .ConstructUsing(src => new ResetPasswordCommand(
                src.email,
                src.otp,
                src.new_password
            ));

        config.NewConfig<ChangePasswordRequest, ChangePasswordCommand>()
            .ConstructUsing(src => new ChangePasswordCommand(
                src.old_password,
                src.new_password,
                0
            ));
    }
}
