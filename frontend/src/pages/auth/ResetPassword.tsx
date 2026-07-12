import { useState } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useTranslation } from 'react-i18next';
import { Eye, EyeOff, Loader2 } from 'lucide-react';
import { toast } from 'sonner';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import AuthLayout from '@/components/AuthLayout';
import { resetPasswordSchema, type ResetPasswordFormValues } from '@/lib/validations';
import { getAuthFlow, clearAuthFlow, getResetPasswordToken, clearResetPasswordToken } from '@/lib/authFlow';
import * as authService from '@/services/auth.service';
import { getErrorMessage } from '@/services/axios/AxiosBase';

const ResetPassword = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const location = useLocation();
    const state = (location.state as any) ?? {};
    const flowState = getAuthFlow();

    const email: string | undefined = state.email ?? flowState?.email;
    const otp: string | undefined = state.otp ?? getResetPasswordToken() ?? undefined;

    const [showPassword, setShowPassword] = useState(false);
    const [isLoading, setIsLoading] = useState(false);

    const { register, handleSubmit, formState: { errors } } = useForm<ResetPasswordFormValues>({
        resolver: zodResolver(resetPasswordSchema),
    });

    const onSubmit = async (values: ResetPasswordFormValues) => {
        if (!email || !otp) {
            toast.error(t('auth.reset.sessionExpired'));
            navigate('/forget-password', { replace: true });
            return;
        }
        setIsLoading(true);
        try {
            await authService.resetPassword({ email, otp, new_password: values.password });
            toast.success(t('auth.reset.resetToast'));
            clearAuthFlow();
            clearResetPasswordToken();
            navigate('/login', { replace: true });
        } catch (error: any) {
            toast.error(getErrorMessage(error, t('auth.reset.resetFailed')), { duration: 6000 });
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <AuthLayout title={t('auth.reset.title')} subtitle={email ? t('auth.reset.forEmail', { email }) : undefined}>
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                <div className="space-y-1.5">
                    <Label htmlFor="password">{t('auth.reset.newPassword')}</Label>
                    <div className="relative">
                        <Input id="password" type={showPassword ? 'text' : 'password'} placeholder={t('auth.reset.passwordPlaceholder')}
                            disabled={isLoading} className="h-12 rounded-xl bg-card pr-12" {...register('password')} />
                        <button type="button" onClick={() => setShowPassword(v => !v)} tabIndex={-1}
                            className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground p-1">
                            {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                        </button>
                    </div>
                    {errors.password && <p className="text-xs text-destructive">{t(errors.password.message as string)}</p>}
                </div>

                <div className="space-y-1.5">
                    <Label htmlFor="confirmPassword">{t('auth.reset.confirmPassword')}</Label>
                    <Input id="confirmPassword" type={showPassword ? 'text' : 'password'} placeholder={t('auth.reset.confirmPasswordPlaceholder')}
                        disabled={isLoading} className="h-12 rounded-xl bg-card" {...register('confirmPassword')} />
                    {errors.confirmPassword && <p className="text-xs text-destructive">{t(errors.confirmPassword.message as string)}</p>}
                </div>

                <Button type="submit" disabled={isLoading}
                    className="w-full h-12 rounded-xl text-sm font-semibold shadow-[0_4px_14px_rgba(249,115,22,0.35)]">
                    {isLoading ? (<><Loader2 className="h-4 w-4 animate-spin" /> {t('auth.reset.resetting')}</>) : t('auth.reset.reset')}
                </Button>

                <p className="text-center text-xs text-muted-foreground">
                    <Link to="/login" className="hover:text-foreground">{t('auth.verify.backToSignIn')}</Link>
                </p>
            </form>
        </AuthLayout>
    );
};

export default ResetPassword;
