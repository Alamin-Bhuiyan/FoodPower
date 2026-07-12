import { useTranslation } from 'react-i18next';
import { cn } from '@/lib/utils';
import { PAYMENT_STATUS } from '@/lib/constants';
import { paymentStatusLabel } from '@/lib/format';

interface StatusBadgeProps {
    status: string | number;
    className?: string;
}

const StatusBadge = ({ status, className }: StatusBadgeProps) => {
    const { t } = useTranslation();
    const label = paymentStatusLabel(status);
    const styles: Record<string, string> = {
        [PAYMENT_STATUS.PENDING]: 'bg-amber-100 text-amber-700 border-amber-200',
        [PAYMENT_STATUS.APPROVED]: 'bg-green-100 text-green-700 border-green-200',
        [PAYMENT_STATUS.REJECTED]: 'bg-red-100 text-red-700 border-red-200',
    };
    return (
        <span className={cn(
            'inline-flex items-center px-2 py-0.5 rounded-full border text-[11px] font-semibold',
            styles[label] ?? 'bg-secondary text-secondary-foreground border-border',
            className
        )}>
            {t(`status.${label.toLowerCase()}`, { defaultValue: label })}
        </span>
    );
};

export default StatusBadge;
