import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { Landmark, Smartphone, Copy, Check } from 'lucide-react';
import { toast } from 'sonner';
import * as settingsService from '@/services/settings.service';
import { cn } from '@/lib/utils';

interface WhereToPayCardProps {
    className?: string;
}

/**
 * "Where to pay" info card: the admin-configured bKash (Send Money) number
 * and bank account, each with a one-tap Copy button (mobile text selection
 * is unreliable). Renders nothing when neither is configured.
 */
const WhereToPayCard = ({ className }: WhereToPayCardProps) => {
    const { t } = useTranslation();
    const [copied, setCopied] = useState<string | null>(null);

    const { data: settingsRes } = useQuery({
        queryKey: ['settings'],
        queryFn: settingsService.getSettings,
    });
    const bkashNumber = settingsRes?.data?.bkash_number?.trim() ?? '';
    const bankAccount = settingsRes?.data?.bank_account?.trim() ?? '';

    if (!bkashNumber && !bankAccount) return null;

    const copy = async (value: string, key: string) => {
        try {
            await navigator.clipboard.writeText(value);
            setCopied(key);
            toast.success(t('payments.copied'));
            setTimeout(() => setCopied(c => (c === key ? null : c)), 2000);
        } catch {
            toast.error(t('payments.copyFailed'));
        }
    };

    return (
        <div className={cn('rounded-xl border border-primary/20 bg-primary/5 p-4 space-y-2.5', className)}>
            <p className="text-xs font-semibold text-primary uppercase tracking-wide">{t('payments.whereToPay')}</p>
            {bkashNumber && (
                <div className="flex items-start gap-2.5">
                    <Smartphone className="h-4 w-4 text-primary mt-0.5 shrink-0" />
                    <div className="min-w-0 flex-1">
                        <p className="text-[11px] text-muted-foreground">{t('payments.bkash')} · {t('payments.sendMoneyNote')}</p>
                        <p className="text-sm font-semibold tabular-nums break-all select-all">{bkashNumber}</p>
                    </div>
                    <button
                        type="button"
                        onClick={() => copy(bkashNumber, 'bkash')}
                        aria-label={t('payments.copy')}
                        className="shrink-0 rounded-lg p-2 text-primary hover:bg-primary/10 active:scale-95 transition"
                    >
                        {copied === 'bkash' ? <Check className="h-4 w-4" /> : <Copy className="h-4 w-4" />}
                    </button>
                </div>
            )}
            {bankAccount && (
                <div className="flex items-start gap-2.5">
                    <Landmark className="h-4 w-4 text-primary mt-0.5 shrink-0" />
                    <div className="min-w-0 flex-1">
                        <p className="text-[11px] text-muted-foreground">{t('payments.bankAccount')}</p>
                        <p className="text-sm font-semibold break-words select-all">{bankAccount}</p>
                    </div>
                    <button
                        type="button"
                        onClick={() => copy(bankAccount, 'bank')}
                        aria-label={t('payments.copy')}
                        className="shrink-0 rounded-lg p-2 text-primary hover:bg-primary/10 active:scale-95 transition"
                    >
                        {copied === 'bank' ? <Check className="h-4 w-4" /> : <Copy className="h-4 w-4" />}
                    </button>
                </div>
            )}
        </div>
    );
};

export default WhereToPayCard;
