import { useEffect, useState } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Loader2 } from 'lucide-react';
import { toast } from 'sonner';
import { InputOTP, InputOTPGroup, InputOTPSlot } from '@/components/ui/input-otp';
import { Button } from '@/components/ui/button';
import AuthLayout from '@/components/AuthLayout';
import { getAuthFlow, clearAuthFlow, setResetPasswordToken } from '@/lib/authFlow';
import * as authService from '@/services/auth.service';
import { getErrorMessage } from '@/services/axios/AxiosBase';

const RESEND_COOLDOWN = 60;

const VerifyOtp = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const location = useLocation();
    const state = (location.state as any) ?? {};
    const flowState = getAuthFlow();

    const email: string | undefined = state.email ?? flowState?.email;
    const flow: string = state.flow ?? flowState?.flow ?? 'register';
    const isForgotFlow = flow === 'forgot' || flow === 'forgot-password';

    const [otp, setOtp] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const [cooldown, setCooldown] = useState(RESEND_COOLDOWN);

    useEffect(() => {
        if (!email) navigate(isForgotFlow ? '/forget-password' : '/register', { replace: true });
    }, [email, isForgotFlow, navigate]);

    useEffect(() => {
        if (cooldown <= 0) return;
        const t = setTimeout(() => setCooldown(c => c - 1), 1000);
        return () => clearTimeout(t);
    }, [cooldown]);

    const handleVerify = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!email) return;
        if (otp.length < 6) { toast.error(t('auth.verify.enterCode')); return; }

        if (isForgotFlow) {
            // OTP is verified server-side together with the new password on /reset-password
            setResetPasswordToken(otp);
            navigate('/reset-password', { state: { email, otp } });
            return;
        }

        setIsLoading(true);
        try {
            await authService.verifyOtp({ email, otp, purpose: 'Register' });
            toast.success(t('auth.verify.verifiedToast'));
            clearAuthFlow();
            navigate('/login', { replace: true });
        } catch (error: any) {
            toast.error(getErrorMessage(error, t('auth.verify.verifyFailed')), { duration: 6000 });
        } finally {
            setIsLoading(false);
        }
    };

    const handleResend = async () => {
        if (!email || cooldown > 0) return;
        try {
            if (isForgotFlow) {
                await authService.forgetPassword({ email });
            } else {
                await authService.resendOtp({ email, purpose: 'Register' });
            }
            toast.success(t('auth.verify.resentToast'));
            setCooldown(RESEND_COOLDOWN);
        } catch (error: any) {
            toast.error(getErrorMessage(error, t('auth.verify.resendFailed')));
        }
    };

    return (
        <AuthLayout
            title={t('auth.verify.title')}
            subtitle={<>{t('auth.verify.sentTo')} <span className="font-semibold text-foreground">{email}</span>{t('auth.verify.expires')}</>}
        >
            <form onSubmit={handleVerify} className="space-y-6">
                <div className="flex justify-center">
                    <InputOTP value={otp} onChange={setOtp} maxLength={6} disabled={isLoading}>
                        <InputOTPGroup className="gap-2">
                            {Array.from({ length: 6 }).map((_, index) => (
                                <InputOTPSlot
                                    key={index}
                                    index={index}
                                    className="w-11 h-[52px] text-lg rounded-xl border bg-card"
                                />
                            ))}
                        </InputOTPGroup>
                    </InputOTP>
                </div>

                <Button type="submit" disabled={isLoading || otp.length < 6}
                    className="w-full h-12 rounded-xl text-sm font-semibold shadow-[0_4px_14px_rgba(249,115,22,0.35)]">
                    {isLoading ? (<><Loader2 className="h-4 w-4 animate-spin" /> {t('auth.verify.verifying')}</>) : t('auth.verify.verify')}
                </Button>

                <div className="text-center text-sm text-muted-foreground">
                    {t('auth.verify.didntGetCode')}{' '}
                    <button
                        type="button"
                        onClick={handleResend}
                        disabled={cooldown > 0}
                        className="font-semibold text-primary disabled:text-muted-foreground"
                    >
                        {cooldown > 0 ? t('auth.verify.resendIn', { s: cooldown }) : t('auth.verify.resendCode')}
                    </button>
                </div>

                <p className="text-center text-xs text-muted-foreground">
                    <Link to="/login" className="hover:text-foreground">{t('auth.verify.backToSignIn')}</Link>
                </p>
            </form>
        </AuthLayout>
    );
};

export default VerifyOtp;
