import { useState } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Eye, EyeOff, Loader2, ArrowRight } from 'lucide-react';
import { toast } from 'sonner';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import AuthLayout from '@/components/AuthLayout';
import { setSession } from '@/lib/auth';
import { setAuthFlow } from '@/lib/authFlow';
import * as authService from '@/services/auth.service';
import { getErrorMessage } from '@/services/axios/AxiosBase';

const Login = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const location = useLocation();
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [showPassword, setShowPassword] = useState(false);
    const [isLoading, setIsLoading] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!email.trim()) { toast.error(t('auth.login.emailRequired')); return; }
        if (!password) { toast.error(t('auth.login.passwordRequired')); return; }

        setIsLoading(true);
        try {
            const result = await authService.login({ email: email.trim(), password });
            const { token, user } = authService.extractLoginResult(result.data);
            if (!token) throw new Error('Invalid login response');
            setSession(token, user);
            toast.success(t('auth.login.loggedIn'));
            const from = (location.state as any)?.from;
            window.location.href = from && from !== '/login' ? from : '/';
        } catch (error: any) {
            const message = getErrorMessage(error, t('auth.login.loginFailed'));
            if (message?.toLowerCase().includes('not verified')) {
                toast.error(t('auth.login.notVerified'), { duration: 8000 });
                setAuthFlow('register', email.trim(), undefined, 'email');
                navigate('/verify-otp', { state: { email: email.trim(), flow: 'register' } });
            } else {
                toast.error(message || t('auth.login.loginFailed'), { duration: 6000 });
            }
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <AuthLayout
            title={t('auth.login.title')}
            subtitle={<>{t('auth.login.newHere')} <Link to="/register" className="font-semibold text-primary">{t('auth.login.createAccount')}</Link></>}
        >
            <form onSubmit={handleSubmit} className="space-y-5">
                <div className="space-y-1.5">
                    <Label htmlFor="email">{t('auth.login.email')}</Label>
                    <Input
                        id="email"
                        type="email"
                        inputMode="email"
                        autoComplete="email"
                        placeholder={t('auth.login.emailPlaceholder')}
                        value={email}
                        onChange={e => setEmail(e.target.value)}
                        disabled={isLoading}
                        className="h-12 rounded-xl bg-card"
                    />
                </div>

                <div className="space-y-1.5">
                    <div className="flex items-center justify-between">
                        <Label htmlFor="password">{t('auth.login.password')}</Label>
                        <Link to="/forget-password" className="text-xs font-medium text-primary">{t('auth.login.forgotPassword')}</Link>
                    </div>
                    <div className="relative">
                        <Input
                            id="password"
                            type={showPassword ? 'text' : 'password'}
                            autoComplete="current-password"
                            placeholder="••••••••"
                            value={password}
                            onChange={e => setPassword(e.target.value)}
                            disabled={isLoading}
                            className="h-12 rounded-xl bg-card pr-12"
                        />
                        <button
                            type="button"
                            onClick={() => setShowPassword(v => !v)}
                            className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground p-1"
                            tabIndex={-1}
                        >
                            {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                        </button>
                    </div>
                </div>

                <Button
                    type="submit"
                    disabled={isLoading}
                    className="w-full h-12 rounded-xl text-sm font-semibold shadow-[0_4px_14px_rgba(249,115,22,0.35)]"
                >
                    {isLoading
                        ? (<><Loader2 className="h-4 w-4 animate-spin" /> {t('auth.login.signingIn')}</>)
                        : (<>{t('auth.login.signIn')} <ArrowRight className="h-4 w-4" /></>)}
                </Button>
            </form>
        </AuthLayout>
    );
};

export default Login;
