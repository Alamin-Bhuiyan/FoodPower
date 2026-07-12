import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useQuery, useMutation } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { toast } from 'sonner';
import { useTranslation } from 'react-i18next';
import { BellOff, ChevronRight, KeyRound, Loader2, LogOut, Settings as SettingsIcon, ShieldCheck } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import UserAvatar from '@/components/common/UserAvatar';
import * as authService from '@/services/auth.service';
import * as notificationsService from '@/services/notifications.service';
import { getErrorMessage } from '@/services/axios/AxiosBase';
import { getStoredUser, isAdmin } from '@/lib/auth';
import { changePasswordSchema, type ChangePasswordFormValues } from '@/lib/validations';
import { formatDateTime } from '@/lib/format';
import { cn } from '@/lib/utils';

const Profile = () => {
    const { t } = useTranslation();
    const user = getStoredUser();
    const admin = isAdmin();
    const [pwOpen, setPwOpen] = useState(false);

    const { data: notifRes } = useQuery({
        queryKey: ['notifications'],
        queryFn: notificationsService.getNotifications,
    });
    const notifications = (notifRes?.data ?? []).slice(0, 10);

    const { register, handleSubmit, reset, formState: { errors } } = useForm<ChangePasswordFormValues>({
        resolver: zodResolver(changePasswordSchema),
    });

    const changePwMutation = useMutation({
        mutationFn: (values: ChangePasswordFormValues) => authService.changePassword({
            old_password: values.oldPassword,
            new_password: values.newPassword,
        }),
        onSuccess: () => {
            toast.success(t('profile.passwordChanged'));
            setPwOpen(false);
            reset();
        },
        onError: (error: any) => toast.error(getErrorMessage(error, t('profile.passwordChangeFailed')), { duration: 6000 }),
    });

    return (
        <div className="space-y-4">
            {/* User card */}
            <div className="card p-5 flex items-center gap-4">
                <UserAvatar name={user?.full_name} size="lg" />
                <div className="min-w-0">
                    <h2 className="text-lg font-bold truncate">{user?.full_name || t('profile.userFallback')}</h2>
                    <p className="text-xs text-muted-foreground truncate">{user?.email}</p>
                    <span className={cn(
                        'inline-flex items-center gap-1 mt-1.5 px-2 py-0.5 rounded-full text-[10px] font-bold',
                        admin ? 'bg-primary/10 text-primary' : 'bg-secondary text-secondary-foreground'
                    )}>
                        {admin && <ShieldCheck className="h-3 w-3" />}
                        {admin ? t('roles.admin') : t('roles.user')}
                    </span>
                </div>
            </div>

            {/* Actions */}
            <div className="card divide-y overflow-hidden">
                <button className="w-full flex items-center gap-3 px-4 py-3.5 text-left active:bg-secondary/50"
                    onClick={() => setPwOpen(true)}>
                    <span className="w-9 h-9 rounded-xl bg-secondary flex items-center justify-center shrink-0">
                        <KeyRound className="h-4 w-4 text-foreground" />
                    </span>
                    <span className="flex-1 text-sm font-semibold">{t('profile.changePassword')}</span>
                    <ChevronRight className="h-4 w-4 text-muted-foreground" />
                </button>
                {admin && (
                    <Link to="/settings" className="w-full flex items-center gap-3 px-4 py-3.5 active:bg-secondary/50">
                        <span className="w-9 h-9 rounded-xl bg-secondary flex items-center justify-center shrink-0">
                            <SettingsIcon className="h-4 w-4 text-foreground" />
                        </span>
                        <span className="flex-1 text-sm font-semibold">{t('profile.appSettings')}</span>
                        <ChevronRight className="h-4 w-4 text-muted-foreground" />
                    </Link>
                )}
            </div>

            {/* Recent notifications */}
            <div>
                <h3 className="text-sm font-bold mb-2">{t('profile.recentNotifications')}</h3>
                {notifications.length === 0 ? (
                    <div className="empty-state py-8">
                        <BellOff />
                        <h3>{t('notificationsSheet.emptyTitle')}</h3>
                        <p>{t('notificationsSheet.emptyBody')}</p>
                    </div>
                ) : (
                    <ul className="card divide-y overflow-hidden">
                        {notifications.map(n => (
                            <li key={n.id} className={cn('px-4 py-3', !n.is_read && 'bg-primary/5')}>
                                <p className="text-sm font-semibold">{n.title}</p>
                                {n.body && <p className="text-xs text-muted-foreground mt-0.5">{n.body}</p>}
                                <p className="text-[11px] text-muted-foreground mt-1">{formatDateTime(n.created_at)}</p>
                            </li>
                        ))}
                    </ul>
                )}
            </div>

            <Button
                variant="outline"
                className="w-full h-12 rounded-xl font-semibold text-red-600 border-red-200 hover:bg-red-50 bg-card"
                onClick={() => authService.logout()}
            >
                <LogOut className="h-4 w-4" /> {t('profile.logOut')}
            </Button>

            {/* Change password dialog */}
            <Dialog open={pwOpen} onOpenChange={setPwOpen}>
                <DialogContent className="max-w-md rounded-2xl">
                    <DialogHeader className="text-left">
                        <DialogTitle>{t('profile.changePassword')}</DialogTitle>
                    </DialogHeader>
                    <form onSubmit={handleSubmit(values => changePwMutation.mutate(values))} className="space-y-4">
                        <div className="space-y-1.5">
                            <Label htmlFor="oldPassword">{t('profile.currentPassword')}</Label>
                            <Input id="oldPassword" type="password" className="h-11 rounded-xl" {...register('oldPassword')} />
                            {errors.oldPassword && <p className="text-xs text-destructive">{t(errors.oldPassword.message as string)}</p>}
                        </div>
                        <div className="space-y-1.5">
                            <Label htmlFor="newPassword">{t('profile.newPassword')}</Label>
                            <Input id="newPassword" type="password" className="h-11 rounded-xl" {...register('newPassword')} />
                            {errors.newPassword && <p className="text-xs text-destructive">{t(errors.newPassword.message as string)}</p>}
                        </div>
                        <div className="space-y-1.5">
                            <Label htmlFor="confirmPassword">{t('profile.confirmNewPassword')}</Label>
                            <Input id="confirmPassword" type="password" className="h-11 rounded-xl" {...register('confirmPassword')} />
                            {errors.confirmPassword && <p className="text-xs text-destructive">{t(errors.confirmPassword.message as string)}</p>}
                        </div>
                        <Button type="submit" disabled={changePwMutation.isPending} className="w-full h-11 rounded-xl font-semibold">
                            {changePwMutation.isPending
                                ? (<><Loader2 className="h-4 w-4 animate-spin" /> {t('common.saving')}</>)
                                : t('profile.changePassword')}
                        </Button>
                    </form>
                </DialogContent>
            </Dialog>
        </div>
    );
};

export default Profile;
