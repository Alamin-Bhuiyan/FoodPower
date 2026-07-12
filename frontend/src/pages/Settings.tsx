import { useEffect, useState } from 'react';
import { Navigate, Link } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Skeleton } from '@/components/ui/skeleton';
import * as settingsService from '@/services/settings.service';
import { getErrorMessage } from '@/services/axios/AxiosBase';
import { isAdmin } from '@/lib/auth';

/** Admin-only app settings: default cutoff time, price per lunch, timezone. */
const Settings = () => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();
    const [cutoff, setCutoff] = useState('');
    const [price, setPrice] = useState('');
    const [timeZone, setTimeZone] = useState('');

    const { data: settingsRes, isLoading } = useQuery({
        queryKey: ['settings'],
        queryFn: settingsService.getSettings,
    });

    useEffect(() => {
        const s = settingsRes?.data;
        if (s) {
            setCutoff(s.default_cutoff_time ?? '10:00');
            setPrice(String(s.price_per_lunch ?? '120'));
            setTimeZone(s.time_zone ?? 'Asia/Dhaka');
        }
    }, [settingsRes]);

    const saveMutation = useMutation({
        mutationFn: () => settingsService.updateSettings({
            default_cutoff_time: cutoff,
            price_per_lunch: price,
            time_zone: timeZone,
        }),
        onSuccess: () => {
            toast.success(t('toasts.updated'));
            queryClient.invalidateQueries({ queryKey: ['settings'] });
        },
        onError: (error: any) => toast.error(getErrorMessage(error, t('toasts.updateFailed')), { duration: 6000 }),
    });

    if (!isAdmin()) return <Navigate to="/profile" replace />;

    if (isLoading) {
        return (
            <div className="space-y-3">
                <Skeleton className="h-20 rounded-2xl" />
                <Skeleton className="h-20 rounded-2xl" />
                <Skeleton className="h-20 rounded-2xl" />
            </div>
        );
    }

    return (
        <div className="space-y-4">
            <Link to="/profile" className="inline-flex items-center gap-1.5 text-sm font-medium text-muted-foreground">
                <ArrowLeft className="h-4 w-4" /> {t('settings.backToProfile')}
            </Link>

            <div className="card p-4 space-y-4">
                <div className="space-y-1.5">
                    <Label>{t('settings.cutoffLabel')}</Label>
                    <Input type="time" value={cutoff} onChange={e => setCutoff(e.target.value)} className="h-11 rounded-xl" />
                    <p className="text-[11px] text-muted-foreground">
                        {t('settings.cutoffHelp')}
                    </p>
                </div>

                <div className="space-y-1.5">
                    <Label>{t('settings.priceLabel')}</Label>
                    <Input type="number" inputMode="decimal" min="1" value={price} onChange={e => setPrice(e.target.value)} className="h-11 rounded-xl" />
                    <p className="text-[11px] text-muted-foreground">
                        {t('settings.priceHelp')}
                    </p>
                </div>

                <div className="space-y-1.5">
                    <Label>{t('settings.timezone')}</Label>
                    <Input value={timeZone} onChange={e => setTimeZone(e.target.value)} placeholder="Asia/Dhaka" className="h-11 rounded-xl" />
                </div>

                <Button
                    className="w-full h-12 rounded-xl font-semibold"
                    disabled={saveMutation.isPending || !cutoff || !price || !timeZone}
                    onClick={() => saveMutation.mutate()}
                >
                    {saveMutation.isPending ? (<><Loader2 className="h-4 w-4 animate-spin" /> {t('common.saving')}</>) : t('settings.saveSettings')}
                </Button>
            </div>
        </div>
    );
};

export default Settings;
