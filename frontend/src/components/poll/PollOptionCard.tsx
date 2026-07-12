import { Check } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import UserAvatar from '@/components/common/UserAvatar';
import { cn } from '@/lib/utils';
import type { PollOption } from '@/types';

interface PollOptionCardProps {
    option: PollOption;
    totalVotes: number;
    selected?: boolean;
    disabled?: boolean;
    onSelect?: () => void;
    onShowVoters?: () => void;
}

/** Large tappable poll option card with live counts and voter avatars. */
const PollOptionCard = ({ option, totalVotes, selected, disabled, onSelect, onShowVoters }: PollOptionCardProps) => {
    const { t } = useTranslation();
    const count = option.vote_count ?? option.voters?.length ?? 0;
    const pct = totalVotes > 0 ? Math.round((count / totalVotes) * 100) : 0;
    const voters = option.voters ?? [];

    return (
        <button
            type="button"
            onClick={onSelect}
            disabled={disabled}
            className={cn(
                'relative w-full text-left rounded-2xl border-2 p-4 bg-card overflow-hidden transition-all',
                selected
                    ? 'border-primary shadow-[0_4px_14px_rgba(249,115,22,0.25)]'
                    : 'border-border shadow-sm',
                disabled ? 'opacity-90 cursor-default' : 'active:scale-[0.985]'
            )}
        >
            {/* progress fill */}
            <div
                className={cn('absolute inset-y-0 left-0 transition-all duration-500', selected ? 'bg-primary/10' : 'bg-secondary/60')}
                style={{ width: `${pct}%` }}
            />
            <div className="relative flex items-center gap-3">
                <span className={cn(
                    'w-6 h-6 rounded-full border-2 flex items-center justify-center shrink-0',
                    selected ? 'border-primary bg-primary text-white' : 'border-muted-foreground/40 bg-white'
                )}>
                    {selected && <Check className="h-3.5 w-3.5" strokeWidth={3} />}
                </span>
                <div className="flex-1 min-w-0">
                    <p className="text-[15px] font-semibold text-foreground truncate">{option.name}</p>
                    <div className="flex items-center gap-2 mt-1">
                        <span className="text-xs text-muted-foreground font-medium tabular-nums">
                            {t('poll.votesPct', { count, pct })}
                        </span>
                    </div>
                </div>
                {voters.length > 0 && (
                    <span
                        role="button"
                        tabIndex={0}
                        className="flex -space-x-2 shrink-0 cursor-pointer"
                        onClick={(e) => { e.stopPropagation(); onShowVoters?.(); }}
                        onKeyDown={(e) => { if (e.key === 'Enter') { e.stopPropagation(); onShowVoters?.(); } }}
                    >
                        {voters.slice(0, 4).map(v => (
                            <UserAvatar key={v.user_id} name={v.full_name} size="sm" />
                        ))}
                        {voters.length > 4 && (
                            <span className="w-7 h-7 rounded-full bg-secondary text-secondary-foreground text-[10px] font-bold flex items-center justify-center ring-2 ring-white">
                                +{voters.length - 4}
                            </span>
                        )}
                    </span>
                )}
            </div>
        </button>
    );
};

export default PollOptionCard;
