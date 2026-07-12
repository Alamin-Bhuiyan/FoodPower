import { useEffect, useMemo, useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { useTranslation } from 'react-i18next';
import { Loader2, Plus, X } from 'lucide-react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import * as menuService from '@/services/menu.service';
import * as pollsService from '@/services/polls.service';
import * as settingsService from '@/services/settings.service';
import { getErrorMessage } from '@/services/axios/AxiosBase';
import { toDateInputValue } from '@/lib/format';

interface PublishPollDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

const tomorrow = () => {
    const d = new Date();
    d.setDate(d.getDate() + 1);
    return toDateInputValue(d);
};

const PublishPollDialog = ({ open, onOpenChange }: PublishPollDialogProps) => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();
    const [lunchDate, setLunchDate] = useState(tomorrow());
    const [catererId, setCatererId] = useState<string>('');
    const [selectedMenuIds, setSelectedMenuIds] = useState<number[]>([]);
    const [customOptions, setCustomOptions] = useState<string[]>([]);
    const [customInput, setCustomInput] = useState('');
    const [customCutoff, setCustomCutoff] = useState('10:00');
    const [cutoffTouched, setCutoffTouched] = useState(false);

    const dayOfWeek = useMemo(() => new Date(lunchDate + 'T00:00:00').getDay(), [lunchDate]);

    const { data: caterersRes } = useQuery({
        queryKey: ['caterers'],
        queryFn: menuService.getCaterers,
        enabled: open,
    });
    const caterers = caterersRes?.data ?? [];

    const { data: settingsRes } = useQuery({
        queryKey: ['settings'],
        queryFn: settingsService.getSettings,
        enabled: open,
    });
    const defaultCutoff = settingsRes?.data?.default_cutoff_time ?? '10:00';

    // Pre-select the cutoff time (10:00 by default, or the admin-configured
    // default once settings load) unless the user has changed it.
    useEffect(() => {
        if (!cutoffTouched) setCustomCutoff(defaultCutoff);
    }, [defaultCutoff, cutoffTouched]);

    const { data: menuRes, isLoading: menuLoading } = useQuery({
        queryKey: ['menu-items', catererId, dayOfWeek],
        queryFn: () => menuService.getMenuItems({ catererId: Number(catererId), day: dayOfWeek }),
        enabled: open && !!catererId,
    });
    const menuItems = (menuRes?.data ?? []).filter(m => m.is_active !== false);

    const createMutation = useMutation({
        mutationFn: pollsService.createPoll,
        onSuccess: () => {
            toast.success(t('publishPoll.publishedToast'));
            queryClient.invalidateQueries({ queryKey: ['active-poll'] });
            onOpenChange(false);
            setSelectedMenuIds([]);
            setCustomOptions([]);
            setCustomCutoff(defaultCutoff);
            setCutoffTouched(false);
        },
        onError: (error: any) => toast.error(getErrorMessage(error, t('publishPoll.publishFailed')), { duration: 6000 }),
    });

    const toggleMenuItem = (id: number) => {
        setSelectedMenuIds(prev => prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]);
    };

    const addCustomOption = () => {
        const name = customInput.trim();
        if (!name) return;
        if (customOptions.some(o => o.toLowerCase() === name.toLowerCase())) {
            toast.error(t('publishPoll.optionExists'));
            return;
        }
        setCustomOptions(prev => [...prev, name]);
        setCustomInput('');
    };

    const handleSubmit = () => {
        if (!lunchDate) { toast.error(t('publishPoll.pickDate')); return; }
        const options: pollsService.CreatePollOption[] = [
            ...selectedMenuIds.map(id => ({ menu_item_id: id })),
            ...customOptions.map(name => ({ custom_name: name })),
        ];
        if (options.length < 1) { toast.error(t('publishPoll.addOneOption')); return; }

        createMutation.mutate({
            lunch_date: lunchDate,
            caterer_id: catererId ? Number(catererId) : null,
            options,
            cutoff_at: customCutoff ? `${lunchDate}T${customCutoff}:00` : null,
        });
    };

    const dayLabel = t(`days.${dayOfWeek}`);

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-md rounded-2xl max-h-[90dvh] overflow-y-auto">
                <DialogHeader className="text-left">
                    <DialogTitle>{t('publishPoll.title')}</DialogTitle>
                    <DialogDescription>
                        {t('publishPoll.subtitle', { time: defaultCutoff })}
                    </DialogDescription>
                </DialogHeader>

                <div className="space-y-4">
                    <div className="space-y-1.5">
                        <Label>{t('publishPoll.lunchDate')} <span className="text-muted-foreground font-normal">({dayLabel})</span></Label>
                        <Input type="date" value={lunchDate} onChange={e => setLunchDate(e.target.value)} className="h-11 rounded-xl" />
                    </div>

                    <div className="space-y-1.5">
                        <Label>{t('publishPoll.caterer')}</Label>
                        <Select value={catererId} onValueChange={v => { setCatererId(v); setSelectedMenuIds([]); }}>
                            <SelectTrigger className="h-11 rounded-xl">
                                <SelectValue placeholder={t('publishPoll.selectCaterer')} />
                            </SelectTrigger>
                            <SelectContent>
                                {caterers.map(c => (
                                    <SelectItem key={c.id} value={String(c.id)}>{c.name}</SelectItem>
                                ))}
                            </SelectContent>
                        </Select>
                    </div>

                    {catererId && (
                        <div className="space-y-1.5">
                            <Label>{t('publishPoll.setMenusFor', { day: dayLabel })}</Label>
                            {menuLoading ? (
                                <p className="text-xs text-muted-foreground">{t('publishPoll.loadingMenu')}</p>
                            ) : menuItems.length === 0 ? (
                                <p className="text-xs text-muted-foreground">{t('publishPoll.noMenusHint')}</p>
                            ) : (
                                <div className="space-y-2">
                                    {menuItems.map(item => (
                                        <label key={item.id}
                                            className="flex items-center gap-3 p-3 rounded-xl border bg-card cursor-pointer active:bg-secondary/50">
                                            <Checkbox
                                                checked={selectedMenuIds.includes(item.id)}
                                                onCheckedChange={() => toggleMenuItem(item.id)}
                                            />
                                            <span className="min-w-0">
                                                <span className="block text-sm font-medium">{item.name}</span>
                                                {item.description && <span className="block text-xs text-muted-foreground truncate">{item.description}</span>}
                                            </span>
                                        </label>
                                    ))}
                                </div>
                            )}
                        </div>
                    )}

                    <div className="space-y-1.5">
                        <Label>{t('publishPoll.customOption')}</Label>
                        <div className="flex gap-2">
                            <Input
                                placeholder={t('publishPoll.customOptionPlaceholder')}
                                value={customInput}
                                onChange={e => setCustomInput(e.target.value)}
                                onKeyDown={e => { if (e.key === 'Enter') { e.preventDefault(); addCustomOption(); } }}
                                className="h-11 rounded-xl"
                            />
                            <Button type="button" variant="secondary" className="h-11 rounded-xl px-3" onClick={addCustomOption}>
                                <Plus className="h-4 w-4" />
                            </Button>
                        </div>
                        {customOptions.length > 0 && (
                            <div className="flex flex-wrap gap-1.5 pt-1">
                                {customOptions.map(name => (
                                    <span key={name} className="inline-flex items-center gap-1 pl-2.5 pr-1 py-1 rounded-full bg-primary/10 text-primary text-xs font-medium">
                                        {name}
                                        <button type="button" onClick={() => setCustomOptions(prev => prev.filter(o => o !== name))}
                                            className="p-0.5 rounded-full hover:bg-primary/20">
                                            <X className="h-3 w-3" />
                                        </button>
                                    </span>
                                ))}
                            </div>
                        )}
                    </div>

                    <div className="space-y-1.5">
                        <Label>{t('publishPoll.customCutoff')} <span className="text-muted-foreground font-normal">{t('publishPoll.customCutoffHint', { time: defaultCutoff })}</span></Label>
                        <Input type="time" value={customCutoff} onChange={e => { setCustomCutoff(e.target.value); setCutoffTouched(true); }} className="h-11 rounded-xl" />
                    </div>

                    <Button
                        className="w-full h-12 rounded-xl font-semibold"
                        onClick={handleSubmit}
                        disabled={createMutation.isPending}
                    >
                        {createMutation.isPending
                            ? (<><Loader2 className="h-4 w-4 animate-spin" /> {t('publishPoll.publishing')}</>)
                            : t('publishPoll.publish')}
                    </Button>
                </div>
            </DialogContent>
        </Dialog>
    );
};

export default PublishPollDialog;
