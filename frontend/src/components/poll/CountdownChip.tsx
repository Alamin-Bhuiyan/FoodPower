import { useEffect, useState } from 'react';
import { Clock } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { cn } from '@/lib/utils';

interface CountdownChipProps {
    cutoffAt: string;
    className?: string;
}

const getRemaining = (cutoffAt: string) => {
    const diff = new Date(cutoffAt).getTime() - Date.now();
    if (isNaN(diff) || diff <= 0) return null;
    const totalSec = Math.floor(diff / 1000);
    const h = Math.floor(totalSec / 3600);
    const m = Math.floor((totalSec % 3600) / 60);
    const s = totalSec % 60;
    return { h, m, s };
};

/** Live countdown to the voting cutoff. */
const CountdownChip = ({ cutoffAt, className }: CountdownChipProps) => {
    const { t } = useTranslation();
    const [remaining, setRemaining] = useState(getRemaining(cutoffAt));

    useEffect(() => {
        setRemaining(getRemaining(cutoffAt));
        const timer = setInterval(() => setRemaining(getRemaining(cutoffAt)), 1000);
        return () => clearInterval(timer);
    }, [cutoffAt]);

    if (!remaining) {
        return (
            <span className={cn('inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full bg-red-100 text-red-700 text-[11px] font-semibold', className)}>
                <Clock className="h-3 w-3" /> {t('countdown.closed')}
            </span>
        );
    }

    const urgent = remaining.h === 0 && remaining.m < 30;
    const text = remaining.h > 0
        ? t('countdown.hoursLeft', { h: remaining.h, m: remaining.m })
        : t('countdown.minutesLeft', { m: remaining.m, s: remaining.s });

    return (
        <span className={cn(
            'inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-[11px] font-semibold tabular-nums',
            urgent ? 'bg-red-100 text-red-700 animate-pulse' : 'bg-primary/10 text-primary',
            className
        )}>
            <Clock className="h-3 w-3" /> {text}
        </span>
    );
};

export default CountdownChip;
