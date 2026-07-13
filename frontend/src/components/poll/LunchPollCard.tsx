import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { useTranslation } from 'react-i18next';
import { Info, Share2, Trash2, Users } from 'lucide-react';
import { Button } from '@/components/ui/button';
import PollOptionCard from '@/components/poll/PollOptionCard';
import CountdownChip from '@/components/poll/CountdownChip';
import ManageVotesSheet from '@/components/poll/ManageVotesSheet';
import * as pollsService from '@/services/polls.service';
import { getErrorMessage } from '@/services/axios/AxiosBase';
import { formatBDT, formatDate, pollStatusLabel } from '@/lib/format';
import { POLL_STATUS } from '@/lib/constants';
import type { AppSettings, Poll } from '@/types';

interface LunchPollCardProps {
    poll: Poll;
    admin: boolean;
    /** Payment details appended to the WhatsApp share message. */
    settings?: AppSettings;
}

/**
 * One Lunch poll's full interactive UI: header card, option voting, after-cutoff
 * note, WhatsApp share (with payment reminder), admin manage, and your-vote /
 * remove-vote block. Each card operates on its own poll id and invalidates the
 * recent-lunch list and the viewer's dues on any change.
 */
const LunchPollCard = ({ poll, admin, settings }: LunchPollCardProps) => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();
    const [manageOpen, setManageOpen] = useState(false);

    const invalidate = () => {
        queryClient.invalidateQueries({ queryKey: ['lunch-recent'] });
        queryClient.invalidateQueries({ queryKey: ['my-dues'] });
    };

    const voteMutation = useMutation({
        mutationFn: (optionId: number) => pollsService.vote(poll.id, optionId),
        onSuccess: () => {
            toast.success(t('home.voteRecorded'));
            invalidate();
        },
        onError: (error: any) => toast.error(getErrorMessage(error, t('home.voteFailed')), { duration: 6000 }),
    });

    const removeVoteMutation = useMutation({
        mutationFn: () => pollsService.removeVote(poll.id),
        onSuccess: () => {
            toast.success(t('home.voteRemoved'));
            invalidate();
        },
        onError: (error: any) => toast.error(getErrorMessage(error, t('home.removeVoteFailed')), { duration: 6000 }),
    });

    const isOpen = pollStatusLabel(poll.status) === POLL_STATUS.OPEN;
    const cutoffPassed = new Date(poll.cutoff_at).getTime() <= Date.now();
    const canVote = isOpen && !cutoffPassed;
    const totalVotes = poll.total_votes
        ?? poll.options.reduce((sum, o) => sum + (o.vote_count ?? o.voters?.length ?? 0), 0);

    const handleShare = () => {
        const pollUrl = `${window.location.origin}/poll/${poll.share_token}`;
        const lines = [t('home.shareText', { date: formatDate(poll.lunch_date), url: pollUrl })];

        const bkash = settings?.bkash_number?.trim();
        const bank = settings?.bank_account?.trim();
        if (bkash || bank) {
            lines.push('', t('home.sharePaymentReminder'));
            if (bkash) lines.push(t('home.shareBkash', { number: bkash }));
            if (bank) lines.push(t('home.shareBank', { account: bank }));
        }

        window.open(`https://wa.me/?text=${encodeURIComponent(lines.join('\n'))}`, '_blank');
    };

    return (
        <div className="space-y-3">
            {/* Poll header card */}
            <div className="card p-4">
                <div className="flex items-start justify-between gap-2">
                    <div>
                        <p className="text-xs font-semibold text-primary uppercase tracking-wide">{t('home.lunchPoll')}</p>
                        <h2 className="text-lg font-bold mt-0.5">{formatDate(poll.lunch_date)}</h2>
                        <p className="text-xs text-muted-foreground mt-0.5">
                            {poll.caterer_name ? `${poll.caterer_name} · ` : ''}{t('home.perLunch', { price: formatBDT(poll.price_per_lunch) })}
                        </p>
                    </div>
                    <CountdownChip cutoffAt={poll.cutoff_at} className="shrink-0" />
                </div>
                <div className="flex items-center gap-2 mt-3 text-xs text-muted-foreground">
                    <Users className="h-3.5 w-3.5" />
                    <span className="tabular-nums">{t('home.votedCount', { count: totalVotes })}</span>
                </div>
            </div>

            {/* Options */}
            <div className="space-y-3">
                {poll.options.map(option => (
                    <PollOptionCard
                        key={option.id}
                        option={option}
                        totalVotes={totalVotes}
                        selected={poll.my_vote_option_id === option.id}
                        disabled={!canVote || voteMutation.isPending || removeVoteMutation.isPending}
                        onSelect={() => {
                            if (!canVote) return;
                            if (poll.my_vote_option_id === option.id) return;
                            voteMutation.mutate(option.id);
                        }}
                        onShowVoters={admin ? () => setManageOpen(true) : undefined}
                    />
                ))}
            </div>

            {/* After-cutoff note */}
            {!canVote && (
                <div className="flex items-start gap-2.5 p-3.5 rounded-xl bg-amber-50 border border-amber-200">
                    <Info className="h-4 w-4 text-amber-600 mt-0.5 shrink-0" />
                    <p className="text-xs text-amber-800">
                        {isOpen ? t('home.cutoffPassedNote') : t('home.closedByAdminNote')}
                    </p>
                </div>
            )}

            {/* Actions */}
            <div className="grid grid-cols-1 gap-2">
                <Button variant="outline" className="h-12 rounded-xl font-semibold bg-card" onClick={handleShare}>
                    <Share2 className="h-4 w-4 text-green-600" /> {t('home.shareWhatsApp')}
                </Button>
                {admin && (
                    <Button variant="secondary" className="h-12 rounded-xl font-semibold" onClick={() => setManageOpen(true)}>
                        <Users className="h-4 w-4" /> {t('home.manageVotes')} {isOpen ? t('home.closePollSuffix') : ''}
                    </Button>
                )}
            </div>

            {poll.my_vote_option_id != null && (
                <div className="space-y-2 pt-1">
                    <p className="text-center text-xs text-muted-foreground">
                        {t('home.yourVote')} <span className="font-semibold text-foreground">
                            {poll.options.find(o => o.id === poll.my_vote_option_id)?.name}
                        </span>
                        {canVote && t('home.tapToChange')}
                    </p>
                    {canVote && (
                        <Button
                            variant="ghost"
                            className="w-full h-10 rounded-xl text-destructive hover:text-destructive hover:bg-destructive/10 text-sm font-medium"
                            onClick={() => removeVoteMutation.mutate()}
                            disabled={removeVoteMutation.isPending || voteMutation.isPending}
                        >
                            <Trash2 className="h-4 w-4" /> {t('home.removeVote')}
                        </Button>
                    )}
                </div>
            )}

            {admin && (
                <ManageVotesSheet open={manageOpen} onOpenChange={setManageOpen} poll={poll} isPollOpen={isOpen} />
            )}
        </div>
    );
};

export default LunchPollCard;
