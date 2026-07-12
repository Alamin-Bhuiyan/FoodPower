import { useEffect, useMemo, useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2, Store, UtensilsCrossed, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import CatererSheet from '@/components/menu/CatererSheet';
import * as menuService from '@/services/menu.service';
import { getErrorMessage } from '@/services/axios/AxiosBase';
import { isAdmin } from '@/lib/auth';
import { formatBDT } from '@/lib/format';
import { DAYS_OF_WEEK, WORK_DAYS } from '@/lib/constants';
import { cn } from '@/lib/utils';
import type { MenuItem } from '@/types';

interface EditState {
    open: boolean;
    day: number;
    item?: MenuItem; // undefined = create
}

const Menu = () => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();
    const admin = isAdmin();
    const [catererId, setCatererId] = useState<number | null>(null);
    const [catererSheetOpen, setCatererSheetOpen] = useState(false);
    const [edit, setEdit] = useState<EditState>({ open: false, day: 0 });
    const [name, setName] = useState('');
    const [description, setDescription] = useState('');

    const { data: caterersRes, isLoading: caterersLoading } = useQuery({
        queryKey: ['caterers'],
        queryFn: menuService.getCaterers,
    });
    const caterers = caterersRes?.data ?? [];

    useEffect(() => {
        if (catererId == null && caterers.length > 0) {
            const active = caterers.find(c => c.is_active) ?? caterers[0];
            setCatererId(active.id);
        }
    }, [caterers, catererId]);

    const selectedCaterer = caterers.find(c => c.id === catererId) ?? null;

    const { data: menuRes, isLoading: menuLoading } = useQuery({
        queryKey: ['menu-items', catererId],
        queryFn: () => menuService.getMenuItems({ catererId: catererId! }),
        enabled: catererId != null,
    });
    const menuItems = menuRes?.data ?? [];

    const itemsByDay = useMemo(() => {
        const map = new Map<number, MenuItem[]>();
        for (const item of menuItems) {
            const list = map.get(item.day_of_week) ?? [];
            list.push(item);
            map.set(item.day_of_week, list);
        }
        return map;
    }, [menuItems]);

    const invalidate = () => queryClient.invalidateQueries({ queryKey: ['menu-items'] });

    const saveMutation = useMutation({
        mutationFn: async () => {
            if (edit.item) {
                return menuService.updateMenuItem(edit.item.id, { name: name.trim(), description: description.trim() || undefined });
            }
            return menuService.createMenuItems({
                caterer_id: catererId!,
                day_of_week: edit.day,
                items: [{ name: name.trim(), description: description.trim() || undefined }],
            });
        },
        onSuccess: () => {
            toast.success(edit.item ? t('toasts.updated') : t('toasts.created'));
            setEdit({ open: false, day: 0 });
            invalidate();
        },
        onError: (error: any) => toast.error(getErrorMessage(error, t('toasts.genericError')), { duration: 6000 }),
    });

    const deleteMutation = useMutation({
        mutationFn: (id: number) => menuService.deleteMenuItem(id),
        onSuccess: () => {
            toast.success(t('toasts.deleted'));
            invalidate();
        },
        onError: (error: any) => toast.error(getErrorMessage(error, t('toasts.deleteFailed')), { duration: 6000 }),
    });

    const openEditor = (day: number, item?: MenuItem) => {
        setName(item?.name ?? '');
        setDescription(item?.description ?? '');
        setEdit({ open: true, day, item });
    };

    const today = new Date().getDay();
    const weekendDays = DAYS_OF_WEEK.filter(d => !WORK_DAYS.includes(d.value));

    if (caterersLoading) {
        return (
            <div className="space-y-4">
                <Skeleton className="h-16 rounded-2xl" />
                {[1, 2, 3].map(i => <Skeleton key={i} className="h-28 rounded-2xl" />)}
            </div>
        );
    }

    const renderDay = (day: { value: number; label: string }, prominent: boolean) => {
        const items = itemsByDay.get(day.value) ?? [];
        if (!prominent && items.length === 0 && !admin) return null;
        return (
            <section key={day.value} className={cn('card p-4', day.value === today && 'border-primary/50')}>
                <div className="flex items-center justify-between mb-2">
                    <h3 className={cn('text-sm font-bold', day.value === today && 'text-primary')}>
                        {t(`days.${day.value}`)}
                        {day.value === today && <span className="ml-2 text-[10px] px-1.5 py-0.5 rounded-full bg-primary/10 text-primary">{t('common.today')}</span>}
                    </h3>
                    {admin && (
                        <Button variant="ghost" size="sm" className="h-8 px-2 text-primary" onClick={() => openEditor(day.value)}>
                            <Plus className="h-4 w-4" /> {t('menu.addItem')}
                        </Button>
                    )}
                </div>
                {menuLoading ? (
                    <Skeleton className="h-12 rounded-xl" />
                ) : items.length === 0 ? (
                    <p className="text-xs text-muted-foreground py-1.5">{t('menu.noMenusForDay')}</p>
                ) : (
                    <ul className="space-y-2">
                        {items.map(item => (
                            <li key={item.id} className="flex items-center gap-3 p-3 rounded-xl bg-secondary/50">
                                <span className="text-lg">🍛</span>
                                <div className="flex-1 min-w-0">
                                    <p className="text-sm font-semibold truncate">{item.name}</p>
                                    {item.description && <p className="text-xs text-muted-foreground truncate">{item.description}</p>}
                                </div>
                                {admin && (
                                    <div className="flex gap-1 shrink-0">
                                        <button className="p-2 rounded-lg hover:bg-secondary text-muted-foreground"
                                            onClick={() => openEditor(day.value, item)} aria-label={t('common.edit')}>
                                            <Pencil className="h-3.5 w-3.5" />
                                        </button>
                                        <button className="p-2 rounded-lg hover:bg-red-50 text-red-500"
                                            onClick={() => { if (confirm(t('menu.deleteConfirm', { name: item.name }))) deleteMutation.mutate(item.id); }}
                                            aria-label={t('common.delete')}>
                                            <Trash2 className="h-3.5 w-3.5" />
                                        </button>
                                    </div>
                                )}
                            </li>
                        ))}
                    </ul>
                )}
            </section>
        );
    };

    return (
        <div className="space-y-4">
            {/* Caterer bar */}
            <div className="card p-4 flex items-center gap-3">
                <span className="w-10 h-10 rounded-xl bg-primary/10 text-primary flex items-center justify-center shrink-0">
                    <Store className="h-5 w-5" />
                </span>
                <div className="flex-1 min-w-0">
                    {caterers.length === 0 ? (
                        <p className="text-sm font-semibold">{t('menu.noCaterer')}</p>
                    ) : (
                        <>
                            <p className="text-sm font-bold truncate">{selectedCaterer?.name}</p>
                            <p className="text-xs text-muted-foreground">
                                {t('menu.perLunch', { price: formatBDT(selectedCaterer?.price_per_lunch ?? 0) })}
                                {selectedCaterer?.phone ? ` · ${selectedCaterer.phone}` : ''}
                            </p>
                        </>
                    )}
                </div>
                {admin && (
                    <Button variant="secondary" size="sm" className="rounded-xl h-9" onClick={() => setCatererSheetOpen(true)}>
                        {t('menu.manage')}
                    </Button>
                )}
            </div>

            {/* Caterer switcher when multiple */}
            {caterers.length > 1 && (
                <div className="flex gap-2 overflow-x-auto no-scrollbar -mx-1 px-1">
                    {caterers.map(c => (
                        <button
                            key={c.id}
                            onClick={() => setCatererId(c.id)}
                            className={cn(
                                'px-3.5 py-1.5 rounded-full text-xs font-semibold whitespace-nowrap border',
                                c.id === catererId ? 'bg-primary text-primary-foreground border-primary' : 'bg-card text-muted-foreground border-border'
                            )}
                        >
                            {c.name}
                        </button>
                    ))}
                </div>
            )}

            {caterers.length === 0 && !admin && (
                <div className="empty-state">
                    <UtensilsCrossed />
                    <h3>{t('menu.notPublishedTitle')}</h3>
                    <p>{t('menu.notPublishedBody')}</p>
                </div>
            )}

            {/* Work days Sun–Thu prominent */}
            {selectedCaterer && (
                <>
                    <div className="space-y-3">
                        {DAYS_OF_WEEK.filter(d => WORK_DAYS.includes(d.value)).map(d => renderDay(d, true))}
                    </div>
                    {(admin || weekendDays.some(d => (itemsByDay.get(d.value) ?? []).length > 0)) && (
                        <div className="space-y-3">
                            <p className="text-[11px] font-semibold uppercase tracking-wider text-muted-foreground pt-1">{t('menu.weekend')}</p>
                            {weekendDays.map(d => renderDay(d, false))}
                        </div>
                    )}
                </>
            )}

            {/* Add/Edit dialog */}
            <Dialog open={edit.open} onOpenChange={(o) => setEdit(prev => ({ ...prev, open: o }))}>
                <DialogContent className="max-w-md rounded-2xl">
                    <DialogHeader className="text-left">
                        <DialogTitle>
                            {edit.item ? t('menu.editSetMenu') : t('menu.addSetMenu', { day: t(`days.${edit.day}`) })}
                        </DialogTitle>
                    </DialogHeader>
                    <div className="space-y-4">
                        <div className="space-y-1.5">
                            <Label>{t('menu.menuName')}</Label>
                            <Input value={name} onChange={e => setName(e.target.value)} placeholder={t('menu.menuNamePlaceholder')} className="h-11 rounded-xl" />
                        </div>
                        <div className="space-y-1.5">
                            <Label>{t('menu.description')} <span className="text-muted-foreground font-normal">{t('common.optional')}</span></Label>
                            <Textarea value={description} onChange={e => setDescription(e.target.value)}
                                placeholder={t('menu.descriptionPlaceholder')} className="rounded-xl" rows={3} />
                        </div>
                        <Button
                            className="w-full h-11 rounded-xl font-semibold"
                            disabled={!name.trim() || saveMutation.isPending}
                            onClick={() => saveMutation.mutate()}
                        >
                            {saveMutation.isPending
                                ? (<><Loader2 className="h-4 w-4 animate-spin" /> {t('common.saving')}</>)
                                : edit.item ? t('menu.saveChanges') : t('menu.addMenu')}
                        </Button>
                    </div>
                </DialogContent>
            </Dialog>

            {admin && <CatererSheet open={catererSheetOpen} onOpenChange={setCatererSheetOpen} caterers={caterers} />}
        </div>
    );
};

export default Menu;
