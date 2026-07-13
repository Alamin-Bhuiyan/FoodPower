import { useEffect, useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { useTranslation } from 'react-i18next';
import { CalendarPlus, Share2, Users, Vote, Info, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import PollOptionCard from '@/components/poll/PollOptionCard';
import CountdownChip from '@/components/poll/CountdownChip';
import PublishPollDialog from '@/components/poll/PublishPollDialog';
import ManageVotesSheet from '@/components/poll/ManageVotesSheet';
import GeneralPollCard from '@/components/poll/GeneralPollCard';
import * as pollsService from '@/services/polls.service';
import * as settingsService from '@/services/settings.service';
import { getErrorMessage } from '@/services/axios/AxiosBase';
import { isAdmin, getStoredUser } from '@/lib/auth';
import { formatBDT, formatDate, pollStatusLabel } from '@/lib/format';
import { POLL_STATUS } from '@/lib/constants';

const Home = () => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();
    const admin = isAdmin();
    const user = getStoredUser();
    const [publishOpen, setPublishOpen] = useState(false);
    const [manageOpen, setManageOpen] = useState(false);
    const [, setTick] = useState(0);

    // re-render every second so voting locks exactly at cutoff
    useEffect(() => {
        const t = setInterval(() => setTick(x => x + 1), 1000);
        return () => clearInterval(t);
    }, []);

    const { data: pollRes, isLoading } = useQuery({
        queryKey: ['active-poll'],
        queryFn: pollsService.getActivePoll,
        refetchInterval: 30000, // live vote counts
    });
    const poll = pollRes?.data ?? null;

    // Open General polls (never affect dues) shown below the lunch poll.
    const { data: generalRes } = useQuery({
        queryKey: ['general-active-polls'],
        queryFn: pollsService.getActiveGeneralPolls,
        refetchInterval: 30000, // live vote counts
    });
    const generalPolls = generalRes?.data ?? [];

    // Payment details (bKash / bank) appended to the WhatsApp share message.
    const { data: settingsRes } = useQuery({
        queryKey: ['settings'],
        queryFn: settingsService.getSettings,
    });
    const settings = settingsRes?.data;

    const voteMutation = useMutation({
        mutationFn: (optionId: number) => pollsService.vote(poll!.id, optionId),
        onSuccess: () => {
            toast.success(t('home.voteRecorded'));
            queryClient.invalidateQueries({ queryKey: ['active-poll'] });
            queryClient.invalidateQueries({ queryKey: ['my-dues'] });
        },
        onError: (error: any) => toast.error(getErrorMessage(error, t('home.voteFailed')), { duration: 6000 }),
    });

    const removeVoteMutation = useMutation({
        mutationFn: () => pollsService.removeVote(poll!.id),
        onSuccess: () => {
            toast.success(t('home.voteRemoved'));
            queryClient.invalidateQueries({ queryKey: ['active-poll'] });
            queryClient.invalidateQueries({ queryKey: ['my-dues'] });
        },
        onError: (error: any) => toast.error(getErrorMessage(error, t('home.removeVoteFailed')), { duration: 6000 }),
    });

    const isOpen = poll ? pollStatusLabel(poll.status) === POLL_STATUS.OPEN : false;
    const cutoffPassed = poll ? new Date(poll.cutoff_at).getTime() <= Date.now() : false;
    const canVote = isOpen && !cutoffPassed;
    const totalVotes = poll
        ? (poll.total_votes ?? poll.options.reduce((sum, o) => sum + (o.vote_count ?? o.voters?.length ?? 0), 0))
        : 0;

    const handleShare = () => {
        if (!poll) return;
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

    if (isLoading) {
        return (
            <div className="space-y-4">
                <Skeleton className="h-24 rounded-2xl" />
                <Skeleton className="h-20 rounded-2xl" />
                <Skeleton className="h-20 rounded-2xl" />
                <Skeleton className="h-20 rounded-2xl" />
            </div>
        );
    }

    // "Other polls" section — only rendered when at least one General poll is open.
    const generalSection = generalPolls.length > 0 ? (
        <div className="space-y-3 pt-2">
            <h2 className="text-xs font-bold text-muted-foreground uppercase tracking-wide px-1">{t('home.otherPolls')}</h2>
            {generalPolls.map(gp => (
                <GeneralPollCard key={gp.id} poll={gp} admin={admin} />
            ))}
        </div>
    ) : null;

    if (!poll) {
        return (
            <div className="space-y-4">
                <div>
                    <div className="empty-state">
                        <Vote />
                        <h3>{t('home.noPollTitle')}</h3>
                        <p>{t('home.noPollBody')}</p>
                    </div>
                    {admin && (
                        <Button
                            className="w-full h-12 rounded-xl font-semibold shadow-[0_4px_14px_rgba(249,115,22,0.35)]"
                            onClick={() => setPublishOpen(true)}
                        >
                            <CalendarPlus className="h-4 w-4" /> {t('home.publishPoll')}
                        </Button>
                    )}
                    <PublishPollDialog open={publishOpen} onOpenChange={setPublishOpen} />
                </div>
                {generalSection}
            </div>
        );
    }

    return (
        <div className="space-y-4">
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
                    <CountdownChip cutoffAt={poll.cutoff_at} />
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

            {user && poll.my_vote_option_id != null && (
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

            {generalSection}

            {admin && (
                <ManageVotesSheet open={manageOpen} onOpenChange={setManageOpen} poll={poll} isPollOpen={isOpen} />
            )}
        </div>
    );
};

export default Home;
