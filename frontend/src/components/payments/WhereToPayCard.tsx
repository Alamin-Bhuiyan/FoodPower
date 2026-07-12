import { useQuery } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { Landmark, Smartphone } from 'lucide-react';
import * as settingsService from '@/services/settings.service';
import { cn } from '@/lib/utils';

interface WhereToPayCardProps {
    className?: string;
}

/**
 * "Where to pay" info card: the admin-configured bKash (Send Money) number
 * and bank account. Renders nothing when neither is configured.
 */
const WhereToPayCard = ({ className }: WhereToPayCardProps) => {
    const { t } = useTranslation();

    const { data: settingsRes } = useQuery({
        queryKey: ['settings'],
        queryFn: settingsService.getSettings,
    });
    const bkashNumber = settingsRes?.data?.bkash_number?.trim() ?? '';
    const bankAccount = settingsRes?.data?.bank_account?.trim() ?? '';

    if (!bkashNumber && !bankAccount) return null;

    return (
        <div className={cn('rounded-xl border border-primary/20 bg-primary/5 p-4 space-y-2.5', className)}>
            <p className="text-xs font-semibold text-primary uppercase tracking-wide">{t('payments.whereToPay')}</p>
            {bkashNumber && (
                <div className="flex items-start gap-2.5">
                    <Smartphone className="h-4 w-4 text-primary mt-0.5 shrink-0" />
                    <div className="min-w-0">
                        <p className="text-sm font-semibold tabular-nums break-all">{t('payments.bkash')} — {bkashNumber}</p>
                        <p className="text-[11px] text-muted-foreground">{t('payments.sendMoneyNote')}</p>
                    </div>
                </div>
            )}
            {bankAccount && (
                <div className="flex items-start gap-2.5">
                    <Landmark className="h-4 w-4 text-primary mt-0.5 shrink-0" />
                    <div className="min-w-0">
                        <p className="text-sm font-semibold break-words">{bankAccount}</p>
                        <p className="text-[11px] text-muted-foreground">{t('payments.bankAccount')}</p>
                    </div>
                </div>
            )}
        </div>
    );
};

export default WhereToPayCard;
