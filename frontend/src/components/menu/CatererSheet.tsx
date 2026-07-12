import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { useTranslation } from 'react-i18next';
import { Loader2, Pencil, Plus, Store } from 'lucide-react';
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetDescription } from '@/components/ui/sheet';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import * as menuService from '@/services/menu.service';
import { getErrorMessage } from '@/services/axios/AxiosBase';
import { formatBDT } from '@/lib/format';
import type { Caterer } from '@/types';

interface CatererSheetProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    caterers: Caterer[];
}

/** Admin: manage caterers and price per lunch. */
const CatererSheet = ({ open, onOpenChange, caterers }: CatererSheetProps) => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();
    const [editing, setEditing] = useState<Caterer | null>(null);
    const [creating, setCreating] = useState(false);
    const [name, setName] = useState('');
    const [phone, setPhone] = useState('');
    const [price, setPrice] = useState('');

    const formOpen = creating || editing !== null;

    const startCreate = () => {
        setEditing(null);
        setName(''); setPhone(''); setPrice('');
        setCreating(true);
    };

    const startEdit = (c: Caterer) => {
        setCreating(false);
        setEditing(c);
        setName(c.name);
        setPhone(c.phone ?? '');
        setPrice(String(c.price_per_lunch));
    };

    const saveMutation = useMutation({
        mutationFn: async () => {
            const payload = { name: name.trim(), phone: phone.trim() || undefined, price_per_lunch: Number(price) };
            if (editing) return menuService.updateCaterer(editing.id, { ...payload, is_active: editing.is_active });
            return menuService.createCaterer(payload);
        },
        onSuccess: () => {
            toast.success(editing ? t('toasts.updated') : t('toasts.created'));
            queryClient.invalidateQueries({ queryKey: ['caterers'] });
            setEditing(null);
            setCreating(false);
        },
        onError: (error: any) => toast.error(getErrorMessage(error, t('toasts.genericError')), { duration: 6000 }),
    });

    const valid = name.trim().length > 0 && Number(price) > 0;

    return (
        <Sheet open={open} onOpenChange={onOpenChange}>
            <SheetContent side="bottom" className="rounded-t-3xl max-h-[88dvh] overflow-y-auto p-0">
                <SheetHeader className="px-5 pt-5 pb-3 text-left">
                    <SheetTitle>{t('caterer.title')}</SheetTitle>
                    <SheetDescription>{t('caterer.subtitle')}</SheetDescription>
                </SheetHeader>

                <div className="px-5 pb-8 space-y-4">
                    <ul className="space-y-2">
                        {caterers.map(c => (
                            <li key={c.id} className="flex items-center gap-3 p-3 rounded-xl border bg-card">
                                <span className="w-9 h-9 rounded-lg bg-primary/10 text-primary flex items-center justify-center shrink-0">
                                    <Store className="h-4 w-4" />
                                </span>
                                <div className="flex-1 min-w-0">
                                    <p className="text-sm font-semibold truncate">
                                        {c.name}
                                        {!c.is_active && <span className="ml-2 text-[10px] px-1.5 py-0.5 rounded-full bg-secondary text-muted-foreground">{t('caterer.inactive')}</span>}
                                    </p>
                                    <p className="text-xs text-muted-foreground">
                                        {t('caterer.pricePerLunchShort', { price: formatBDT(c.price_per_lunch) })}{c.phone ? ` · ${c.phone}` : ''}
                                    </p>
                                </div>
                                <Button variant="ghost" size="icon" className="h-9 w-9 shrink-0" onClick={() => startEdit(c)}>
                                    <Pencil className="h-4 w-4" />
                                </Button>
                            </li>
                        ))}
                    </ul>

                    {!formOpen && (
                        <Button variant="secondary" className="w-full h-11 rounded-xl font-semibold" onClick={startCreate}>
                            <Plus className="h-4 w-4" /> {t('caterer.addCaterer')}
                        </Button>
                    )}

                    {formOpen && (
                        <div className="rounded-xl border bg-card p-4 space-y-3">
                            <h3 className="text-sm font-semibold">{editing ? t('caterer.editCaterer', { name: editing.name }) : t('caterer.newCaterer')}</h3>
                            <div className="space-y-1.5">
                                <Label>{t('caterer.name')}</Label>
                                <Input value={name} onChange={e => setName(e.target.value)} placeholder={t('caterer.namePlaceholder')} className="h-11 rounded-xl" />
                            </div>
                            <div className="space-y-1.5">
                                <Label>{t('caterer.phone')} <span className="text-muted-foreground font-normal">{t('common.optional')}</span></Label>
                                <Input value={phone} onChange={e => setPhone(e.target.value)} inputMode="tel" placeholder={t('caterer.phonePlaceholder')} className="h-11 rounded-xl" />
                            </div>
                            <div className="space-y-1.5">
                                <Label>{t('caterer.pricePerLunchBdt')}</Label>
                                <Input value={price} onChange={e => setPrice(e.target.value)} inputMode="decimal" type="number" min="1" placeholder="120" className="h-11 rounded-xl" />
                            </div>
                            <div className="flex gap-2">
                                <Button variant="secondary" className="flex-1 h-11 rounded-xl"
                                    onClick={() => { setEditing(null); setCreating(false); }}>
                                    {t('common.cancel')}
                                </Button>
                                <Button className="flex-1 h-11 rounded-xl font-semibold" disabled={!valid || saveMutation.isPending}
                                    onClick={() => saveMutation.mutate()}>
                                    {saveMutation.isPending ? (<><Loader2 className="h-4 w-4 animate-spin" /> {t('common.saving')}</>) : t('common.save')}
                                </Button>
                            </div>
                        </div>
                    )}
                </div>
            </SheetContent>
        </Sheet>
    );
};

export default CatererSheet;
