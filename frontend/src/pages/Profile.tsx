import { useEffect, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { toast } from 'sonner';
import { useTranslation } from 'react-i18next';
import { Bell, BellOff, Camera, ChevronRight, KeyRound, Loader2, LogOut, Settings as SettingsIcon, ShieldCheck } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import UserAvatar from '@/components/common/UserAvatar';
import * as authService from '@/services/auth.service';
import * as notificationsService from '@/services/notifications.service';
import * as usersService from '@/services/users.service';
import { getErrorMessage } from '@/services/axios/AxiosBase';
import { isPushSupported, getNotificationPermission, hasActiveSubscription, enablePush, disablePush } from '@/lib/push';
import { getStoredUser, setStoredUser, isAdmin } from '@/lib/auth';
import type { AuthUser } from '@/types';
import { changePasswordSchema, type ChangePasswordFormValues } from '@/lib/validations';
import { formatDateTime } from '@/lib/format';
import { cn } from '@/lib/utils';

const Profile = () => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();
    const [user, setUser] = useState<AuthUser | null>(getStoredUser());
    const admin = isAdmin();
    const [pwOpen, setPwOpen] = useState(false);
    const photoInputRef = useRef<HTMLInputElement>(null);

    // Web Push notifications
    const [pushSupported] = useState(() => isPushSupported());
    const [pushOn, setPushOn] = useState(false);
    const [pushBusy, setPushBusy] = useState(false);

    useEffect(() => {
        if (!pushSupported) return;
        hasActiveSubscription().then((active) => {
            setPushOn(active && getNotificationPermission() === 'granted');
        });
    }, [pushSupported]);

    const handleTogglePush = async (next: boolean) => {
        setPushBusy(true);
        try {
            if (next) {
                const result = await enablePush();
                if (result === 'granted') {
                    setPushOn(true);
                    toast.success(t('profile.notificationsOn'));
                } else if (result === 'denied') {
                    setPushOn(false);
                    toast.error(t('profile.notificationsBlocked'), { duration: 6000 });
                } else {
                    setPushOn(false);
                    toast.error(t('profile.notificationsUnavailable'), { duration: 6000 });
                }
            } else {
                await disablePush();
                setPushOn(false);
            }
        } catch (error: any) {
            setPushOn(false);
            toast.error(getErrorMessage(error, t('profile.notificationsFailed')), { duration: 6000 });
        } finally {
            setPushBusy(false);
        }
    };

    const persistUser = (updated: AuthUser) => {
        setStoredUser(updated);
        setUser(updated);
        queryClient.invalidateQueries({ queryKey: ['me'] });
    };

    const uploadPhotoMutation = useMutation({
        mutationFn: (imageBase64: string) => usersService.uploadMyPhoto(imageBase64),
        onSuccess: (res) => {
            persistUser(res.data);
            toast.success(t('profile.photoUpdated'));
        },
        onError: (error: any) => toast.error(getErrorMessage(error, t('profile.photoFailed')), { duration: 6000 }),
    });

    const removePhotoMutation = useMutation({
        mutationFn: () => usersService.removeMyPhoto(),
        onSuccess: (res) => {
            persistUser(res.data);
            toast.success(t('profile.photoUpdated'));
        },
        onError: (error: any) => toast.error(getErrorMessage(error, t('profile.photoFailed')), { duration: 6000 }),
    });

    const photoBusy = uploadPhotoMutation.isPending || removePhotoMutation.isPending;

    const handlePhotoFile = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;
        if (file.size > 5 * 1024 * 1024) {
            toast.error(t('profile.photoTooLarge'));
            if (photoInputRef.current) photoInputRef.current.value = '';
            return;
        }
        const reader = new FileReader();
        reader.onload = () => uploadPhotoMutation.mutate(reader.result as string);
        reader.readAsDataURL(file);
        if (photoInputRef.current) photoInputRef.current.value = '';
    };

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
                <div className="relative shrink-0">
                    <UserAvatar name={user?.full_name} imageUrl={user?.profile_picture} size="lg" />
                    <button
                        type="button"
                        onClick={() => photoInputRef.current?.click()}
                        disabled={photoBusy}
                        className="absolute -bottom-1 -right-1 w-7 h-7 rounded-full bg-primary text-primary-foreground flex items-center justify-center ring-2 ring-white shadow-sm active:scale-95 disabled:opacity-60"
                        aria-label={t('profile.changePhoto')}
                    >
                        {photoBusy
                            ? <Loader2 className="h-3.5 w-3.5 animate-spin" />
                            : <Camera className="h-3.5 w-3.5" />}
                    </button>
                    <input ref={photoInputRef} type="file" accept="image/*" className="hidden" onChange={handlePhotoFile} />
                </div>
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
                    {user?.profile_picture && (
                        <button
                            type="button"
                            onClick={() => removePhotoMutation.mutate()}
                            disabled={photoBusy}
                            className="block mt-1.5 text-[11px] font-semibold text-red-600 disabled:opacity-60"
                        >
                            {t('profile.removePhoto')}
                        </button>
                    )}
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

            {/* Notifications toggle (Web Push) — hidden when the browser/platform can't support it */}
            {pushSupported && (
                <div className="card p-4">
                    <div className="flex items-center gap-3">
                        <span className="w-9 h-9 rounded-xl bg-secondary flex items-center justify-center shrink-0">
                            <Bell className="h-4 w-4 text-foreground" />
                        </span>
                        <div className="flex-1 min-w-0">
                            <p className="text-sm font-semibold">{t('profile.notifications')}</p>
                            <p className="text-xs text-muted-foreground">{t('profile.enableNotifications')}</p>
                        </div>
                        {pushBusy ? (
                            <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />
                        ) : (
                            <Switch
                                checked={pushOn}
                                onCheckedChange={handleTogglePush}
                                aria-label={t('profile.enableNotifications')}
                            />
                        )}
                    </div>
                    <p className="mt-3 text-[11px] text-muted-foreground">{t('profile.iosInstallHint')}</p>
                </div>
            )}

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
