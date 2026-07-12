import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useTranslation } from 'react-i18next';
import { Eye, EyeOff, Loader2, ArrowRight } from 'lucide-react';
import { toast } from 'sonner';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import AuthLayout from '@/components/AuthLayout';
import { registerSchema, type RegisterFormValues } from '@/lib/validations';
import { setAuthFlow } from '@/lib/authFlow';
import * as authService from '@/services/auth.service';
import { getErrorMessage } from '@/services/axios/AxiosBase';

const Register = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const [showPassword, setShowPassword] = useState(false);
    const [isLoading, setIsLoading] = useState(false);

    const { register, handleSubmit, formState: { errors } } = useForm<RegisterFormValues>({
        resolver: zodResolver(registerSchema),
    });

    const onSubmit = async (values: RegisterFormValues) => {
        setIsLoading(true);
        try {
            await authService.register({
                full_name: values.fullName.trim(),
                email: values.email.trim(),
                password: values.password,
            });
            toast.success(t('auth.register.createdToast'));
            setAuthFlow('register', values.email.trim(), undefined, 'email');
            navigate('/verify-otp', { state: { email: values.email.trim(), flow: 'register' } });
        } catch (error: any) {
            toast.error(getErrorMessage(error, t('auth.register.registerFailed')), { duration: 6000 });
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <AuthLayout
            title={t('auth.register.title')}
            subtitle={<>{t('auth.register.alreadyRegistered')} <Link to="/login" className="font-semibold text-primary">{t('auth.register.signIn')}</Link></>}
        >
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                <div className="space-y-1.5">
                    <Label htmlFor="fullName">{t('auth.register.fullName')}</Label>
                    <Input id="fullName" placeholder={t('auth.register.fullNamePlaceholder')} disabled={isLoading}
                        className="h-12 rounded-xl bg-card" {...register('fullName')} />
                    {errors.fullName && <p className="text-xs text-destructive">{t(errors.fullName.message as string)}</p>}
                </div>

                <div className="space-y-1.5">
                    <Label htmlFor="email">{t('auth.register.workEmail')}</Label>
                    <Input id="email" type="email" inputMode="email" placeholder={t('auth.login.emailPlaceholder')} disabled={isLoading}
                        className="h-12 rounded-xl bg-card" {...register('email')} />
                    {errors.email && <p className="text-xs text-destructive">{t(errors.email.message as string)}</p>}
                </div>

                <div className="space-y-1.5">
                    <Label htmlFor="password">{t('auth.register.password')}</Label>
                    <div className="relative">
                        <Input id="password" type={showPassword ? 'text' : 'password'} placeholder={t('auth.register.passwordPlaceholder')}
                            disabled={isLoading} className="h-12 rounded-xl bg-card pr-12" {...register('password')} />
                        <button type="button" onClick={() => setShowPassword(v => !v)} tabIndex={-1}
                            className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground p-1">
                            {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                        </button>
                    </div>
                    {errors.password && <p className="text-xs text-destructive">{t(errors.password.message as string)}</p>}
                </div>

                <div className="space-y-1.5">
                    <Label htmlFor="confirmPassword">{t('auth.register.confirmPassword')}</Label>
                    <Input id="confirmPassword" type={showPassword ? 'text' : 'password'} placeholder={t('auth.register.confirmPasswordPlaceholder')}
                        disabled={isLoading} className="h-12 rounded-xl bg-card" {...register('confirmPassword')} />
                    {errors.confirmPassword && <p className="text-xs text-destructive">{t(errors.confirmPassword.message as string)}</p>}
                </div>

                <Button type="submit" disabled={isLoading}
                    className="w-full h-12 rounded-xl text-sm font-semibold shadow-[0_4px_14px_rgba(249,115,22,0.35)]">
                    {isLoading
                        ? (<><Loader2 className="h-4 w-4 animate-spin" /> {t('auth.register.creating')}</>)
                        : (<>{t('auth.register.create')} <ArrowRight className="h-4 w-4" /></>)}
                </Button>
            </form>
        </AuthLayout>
    );
};

export default Register;
