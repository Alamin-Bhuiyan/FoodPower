import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Loader2, ArrowRight } from 'lucide-react';
import { toast } from 'sonner';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import AuthLayout from '@/components/AuthLayout';
import { setAuthFlow } from '@/lib/authFlow';
import * as authService from '@/services/auth.service';
import { getErrorMessage } from '@/services/axios/AxiosBase';

const ForgetPassword = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const [email, setEmail] = useState('');
    const [isLoading, setIsLoading] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!email.trim()) { toast.error(t('auth.login.emailRequired')); return; }

        setIsLoading(true);
        try {
            await authService.forgetPassword({ email: email.trim() });
            toast.success(t('auth.forgot.sentToast'));
            setAuthFlow('forgot', email.trim(), undefined, 'email');
            navigate('/verify-otp', { state: { email: email.trim(), flow: 'forgot' } });
        } catch (error: any) {
            toast.error(getErrorMessage(error, t('auth.forgot.sendFailed')), { duration: 6000 });
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <AuthLayout
            title={t('auth.forgot.title')}
            subtitle={t('auth.forgot.subtitle')}
        >
            <form onSubmit={handleSubmit} className="space-y-5">
                <div className="space-y-1.5">
                    <Label htmlFor="email">{t('auth.forgot.email')}</Label>
                    <Input
                        id="email"
                        type="email"
                        inputMode="email"
                        placeholder={t('auth.login.emailPlaceholder')}
                        value={email}
                        onChange={e => setEmail(e.target.value)}
                        disabled={isLoading}
                        className="h-12 rounded-xl bg-card"
                    />
                </div>

                <Button type="submit" disabled={isLoading}
                    className="w-full h-12 rounded-xl text-sm font-semibold shadow-[0_4px_14px_rgba(249,115,22,0.35)]">
                    {isLoading
                        ? (<><Loader2 className="h-4 w-4 animate-spin" /> {t('auth.forgot.sending')}</>)
                        : (<>{t('auth.forgot.sendCode')} <ArrowRight className="h-4 w-4" /></>)}
                </Button>

                <p className="text-center text-xs text-muted-foreground">
                    <Link to="/login" className="hover:text-foreground">{t('auth.verify.backToSignIn')}</Link>
                </p>
            </form>
        </AuthLayout>
    );
};

export default ForgetPassword;
