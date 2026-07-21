import { useRef, useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { useTranslation } from 'react-i18next';
import { Banknote, ImagePlus, Landmark, Loader2, Minus, Plus, Smartphone, X } from 'lucide-react';
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetDescription } from '@/components/ui/sheet';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import UserAvatar from '@/components/common/UserAvatar';
import WhereToPayCard from '@/components/payments/WhereToPayCard';
import * as usersService from '@/services/users.service';
import * as paymentsService from '@/services/payments.service';
import * as settingsService from '@/services/settings.service';
import { getErrorMessage } from '@/services/axios/AxiosBase';
import { getStoredUser } from '@/lib/auth';
import { formatBDT } from '@/lib/format';
import { cn } from '@/lib/utils';
import type { PaymentMethod } from '@/services/payments.service';

interface SubmitPaymentSheetProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
}

interface BeneficiaryRow {
    userId: number;
    name: string;
    picture?: string | null;
    days: number;
    isMe: boolean;
}

const SubmitPaymentSheet = ({ open, onOpenChange }: SubmitPaymentSheetProps) => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();
    const me = getStoredUser();
    const fileRef = useRef<HTMLInputElement>(null);

    const [beneficiaries, setBeneficiaries] = useState<BeneficiaryRow[]>(
        me ? [{ userId: me.id, name: me.full_name || 'Me', picture: me.profile_picture, days: 4, isMe: true }] : []
    );
    const [pickUserId, setPickUserId] = useState('');
    const [method, setMethod] = useState<PaymentMethod>('bkash');
    const [screenshot, setScreenshot] = useState<string | null>(null);
    const [note, setNote] = useState('');

    const isCash = method === 'cash';

    const { data: usersRes } = useQuery({
        queryKey: ['users'],
        queryFn: usersService.getUsers,
        enabled: open,
    });
    const users = usersRes?.data ?? [];

    const { data: settingsRes } = useQuery({
        queryKey: ['settings'],
        queryFn: settingsService.getSettings,
        enabled: open,
    });
    const price = Number(settingsRes?.data?.price_per_lunch ?? 120);

    const availableUsers = users.filter(u => !beneficiaries.some(b => b.userId === u.id));
    const totalDays = beneficiaries.reduce((sum, b) => sum + b.days, 0);
    const totalAmount = totalDays * price;

    const addBeneficiary = (userIdStr: string) => {
        const user = users.find(u => u.id === Number(userIdStr));
        if (!user) return;
        setBeneficiaries(prev => [...prev, { userId: user.id, name: user.full_name, picture: user.profile_picture, days: 4, isMe: user.id === me?.id}]);
        setPickUserId('');
    };

    const setDays = (userId: number, delta: number) => {
        setBeneficiaries(prev => prev.map(b =>
            b.userId === userId ? { ...b, days: Math.max(1, Math.min(31, b.days + delta)) } : b
        ));
    };

    const removeBeneficiary = (userId: number) => {
        setBeneficiaries(prev => prev.filter(b => b.userId !== userId));
    };

    const handleFile = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;
        if (file.size > 5 * 1024 * 1024) { toast.error(t('submitPayment.screenshotTooLarge')); return; }
        const reader = new FileReader();
        reader.onload = () => setScreenshot(reader.result as string);
        reader.readAsDataURL(file);
    };

    const submitMutation = useMutation({
        mutationFn: () => paymentsService.submitPayment({
            screenshot: screenshot ?? undefined,
            note: note.trim() || undefined,
            payment_method: method,
            allocations: beneficiaries.map(b => ({ beneficiary_user_id: b.userId, days: b.days })),
        }),
        onSuccess: () => {
            toast.success(t('submitPayment.submittedToast'));
            queryClient.invalidateQueries({ queryKey: ['my-payments'] });
            queryClient.invalidateQueries({ queryKey: ['payments'] });
            onOpenChange(false);
            setScreenshot(null);
            setNote('');
            setMethod('bkash');
            if (me) setBeneficiaries([{ userId: me.id, name: me.full_name || 'Me', days: 4, isMe: true }]);
        },
        onError: (error: any) => toast.error(getErrorMessage(error, t('submitPayment.submitFailed')), { duration: 6000 }),
    });

    // Screenshot is proof for bKash / bank transfer; cash is handed over in person.
    const canSubmit = beneficiaries.length > 0 && (isCash || !!screenshot);

    const methods: { key: PaymentMethod; label: string; Icon: typeof Banknote }[] = [
        { key: 'bkash', label: t('submitPayment.methodBkash'), Icon: Smartphone },
        { key: 'bank_transfer', label: t('submitPayment.methodBankTransfer'), Icon: Landmark },
        { key: 'cash', label: t('submitPayment.methodCash'), Icon: Banknote },
    ];

    return (
        <Sheet open={open} onOpenChange={onOpenChange}>
            <SheetContent side="bottom" className="rounded-t-3xl max-h-[92dvh] overflow-y-auto p-0">
                <SheetHeader className="px-5 pt-5 pb-3 text-left">
                    <SheetTitle>{t('submitPayment.title')}</SheetTitle>
                    <SheetDescription>
                        {t('submitPayment.subtitle', { price: formatBDT(price) })}
                    </SheetDescription>
                </SheetHeader>

                <div className="px-5 pb-8 space-y-5">
                    {/* Beneficiaries */}
                    <div className="space-y-2">
                        <Label>{t('submitPayment.whoFor')}</Label>
                        {beneficiaries.map(b => (
                            <div key={b.userId} className="flex items-center gap-3 p-3 rounded-xl border bg-card">
                                <UserAvatar name={b.name} imageUrl={b.picture} size="md" />
                                <div className="flex-1 min-w-0">
                                    <p className="text-sm font-semibold truncate">
                                        {b.isMe ? t('submitPayment.forMe') : b.name}
                                    </p>
                                    <p className="text-xs text-muted-foreground tabular-nums">
                                        {t('submitPayment.calc', { count: b.days, price: formatBDT(price), total: formatBDT(b.days * price) })}
                                    </p>
                                </div>
                                <div className="flex items-center gap-1 shrink-0">
                                    <button className="w-8 h-8 rounded-lg bg-secondary flex items-center justify-center active:scale-95"
                                        onClick={() => setDays(b.userId, -1)} aria-label={t('submitPayment.fewerDays')}>
                                        <Minus className="h-3.5 w-3.5" />
                                    </button>
                                    <span className="w-7 text-center text-sm font-bold tabular-nums">{b.days}</span>
                                    <button className="w-8 h-8 rounded-lg bg-secondary flex items-center justify-center active:scale-95"
                                        onClick={() => setDays(b.userId, 1)} aria-label={t('submitPayment.moreDays')}>
                                        <Plus className="h-3.5 w-3.5" />
                                    </button>
                                </div>
                                {!b.isMe && (
                                    <button className="p-1.5 text-muted-foreground hover:text-destructive"
                                        onClick={() => removeBeneficiary(b.userId)} aria-label={t('submitPayment.remove')}>
                                        <X className="h-4 w-4" />
                                    </button>
                                )}
                            </div>
                        ))}

                        {availableUsers.length > 0 && (
                            <Select value={pickUserId} onValueChange={addBeneficiary}>
                                <SelectTrigger className="h-11 rounded-xl border-dashed text-muted-foreground">
                                    <SelectValue placeholder={t('submitPayment.addCoworker')} />
                                </SelectTrigger>
                                <SelectContent>
                                    {availableUsers.map(u => (
                                        <SelectItem key={u.id} value={String(u.id)}>{u.full_name}</SelectItem>
                                    ))}
                                </SelectContent>
                            </Select>
                        )}
                    </div>

                    {/* Total */}
                    <div className="flex items-center justify-between p-4 rounded-xl bg-primary/10 border border-primary/20">
                        <span className="text-sm font-semibold text-foreground">{t('submitPayment.totalToPay')}</span>
                        <span className="text-lg font-extrabold text-primary tabular-nums">{formatBDT(totalAmount)}</span>
                    </div>

                    {/* Payment method */}
                    <div className="space-y-2">
                        <Label>{t('submitPayment.paymentMethod')}</Label>
                        <div className="grid grid-cols-3 gap-2" role="radiogroup" aria-label={t('submitPayment.paymentMethod')}>
                            {methods.map(({ key, label, Icon }) => (
                                <button
                                    key={key}
                                    type="button"
                                    role="radio"
                                    aria-checked={method === key}
                                    onClick={() => setMethod(key)}
                                    className={cn(
                                        'flex flex-col items-center justify-center gap-1.5 h-20 rounded-xl border-2 transition active:scale-95',
                                        method === key
                                            ? 'border-primary bg-primary/10 text-primary'
                                            : 'border-border bg-card text-muted-foreground'
                                    )}
                                >
                                    <Icon className="h-5 w-5" />
                                    <span className="text-xs font-semibold">{label}</span>
                                </button>
                            ))}
                        </div>
                    </div>

                    {/* Where to pay — only relevant for bKash / bank transfer */}
                    {!isCash && <WhereToPayCard />}

                    {/* Cash: hand over in person, screenshot optional */}
                    {isCash && (
                        <div className="rounded-xl border border-primary/20 bg-primary/5 p-3.5">
                            <p className="text-xs text-muted-foreground">{t('submitPayment.cashHint')}</p>
                        </div>
                    )}

                    {/* Screenshot */}
                    <div className="space-y-2">
                        <Label>
                            {t('submitPayment.screenshotLabel')}
                            {isCash && <span className="text-muted-foreground font-normal"> {t('common.optional')}</span>}
                        </Label>
                        <input ref={fileRef} type="file" accept="image/*" className="hidden" onChange={handleFile} />
                        {screenshot ? (
                            <div className="relative">
                                <img src={screenshot} alt={t('payments.screenshotAlt')} className="w-full max-h-64 object-contain rounded-xl border bg-card" />
                                <button
                                    className="absolute top-2 right-2 w-8 h-8 rounded-full bg-black/60 text-white flex items-center justify-center"
                                    onClick={() => { setScreenshot(null); if (fileRef.current) fileRef.current.value = ''; }}
                                    aria-label={t('submitPayment.removeScreenshot')}
                                >
                                    <X className="h-4 w-4" />
                                </button>
                            </div>
                        ) : (
                            <button
                                type="button"
                                onClick={() => fileRef.current?.click()}
                                className="w-full h-28 rounded-xl border-2 border-dashed border-border bg-card flex flex-col items-center justify-center gap-1.5 text-muted-foreground active:bg-secondary/40"
                            >
                                <ImagePlus className="h-6 w-6" />
                                <span className="text-xs font-medium">
                                    {isCash ? t('submitPayment.uploadHintCash') : t('submitPayment.uploadHint')}
                                </span>
                            </button>
                        )}
                    </div>

                    {/* Note */}
                    <div className="space-y-1.5">
                        <Label>{t('submitPayment.note')} <span className="text-muted-foreground font-normal">{t('common.optional')}</span></Label>
                        <Textarea value={note} onChange={e => setNote(e.target.value)} rows={2}
                            placeholder={t('submitPayment.notePlaceholder')} className="rounded-xl" />
                    </div>

                    <Button
                        className="w-full h-12 rounded-xl font-semibold shadow-[0_4px_14px_rgba(249,115,22,0.35)]"
                        disabled={!canSubmit || submitMutation.isPending}
                        onClick={() => submitMutation.mutate()}
                    >
                        {submitMutation.isPending
                            ? (<><Loader2 className="h-4 w-4 animate-spin" /> {t('submitPayment.submitting')}</>)
                            : t('submitPayment.submitForApproval', { amount: formatBDT(totalAmount) })}
                    </Button>
                </div>
            </SheetContent>
        </Sheet>
    );
};

export default SubmitPaymentSheet;
