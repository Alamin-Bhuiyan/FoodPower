import { Link, useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { Users, Vote } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import CountdownChip from '@/components/poll/CountdownChip';
import UserAvatar from '@/components/common/UserAvatar';
import LanguageToggle from '@/components/common/LanguageToggle';
import * as pollsService from '@/services/polls.service';
import { isAuthenticated } from '@/lib/auth';
import { formatBDT, formatDate } from '@/lib/format';
import { cn } from '@/lib/utils';

/** Public shared poll page — results + login CTA (linked from WhatsApp). */
const SharedPoll = () => {
    const { t } = useTranslation();
    const { shareToken } = useParams<{ shareToken: string }>();
    const loggedIn = isAuthenticated();

    const { data: pollRes, isLoading, isError } = useQuery({
        queryKey: ['shared-poll', shareToken],
        queryFn: () => pollsService.getSharedPoll(shareToken!),
        enabled: !!shareToken,
        refetchInterval: 30000,
    });
    const poll = pollRes?.data;
    const totalVotes = poll
        ? (poll.total_votes ?? poll.options.reduce((sum, o) => sum + (o.vote_count ?? o.voters?.length ?? 0), 0))
        : 0;

    return (
        <div className="max-w-md mx-auto min-h-dvh px-4 pt-[calc(var(--safe-top)+1.5rem)] pb-[calc(var(--safe-bottom)+2rem)]">
            {/* Brand */}
            <div className="flex items-start justify-between mb-6">
                <div className="flex items-center gap-2.5">
                    <span className="w-10 h-10 rounded-2xl bg-primary text-primary-foreground flex items-center justify-center text-xl shadow-sm">🍱</span>
                    <div>
                        <div className="font-extrabold leading-tight">FoodPower</div>
                        <div className="text-[10px] text-muted-foreground">{t('app.pollTagline')}</div>
                    </div>
                </div>
                <LanguageToggle className="mt-1" />
            </div>

            {isLoading ? (
                <div className="space-y-3">
                    <Skeleton className="h-24 rounded-2xl" />
                    <Skeleton className="h-20 rounded-2xl" />
                    <Skeleton className="h-20 rounded-2xl" />
                </div>
            ) : isError || !poll ? (
                <div className="empty-state">
                    <Vote />
                    <h3>{t('sharedPoll.notFoundTitle')}</h3>
                    <p>{t('sharedPoll.notFoundBody')}</p>
                    <Button asChild className="mt-4 h-11 rounded-xl px-6 font-semibold">
                        <Link to="/login">{t('sharedPoll.goToApp')}</Link>
                    </Button>
                </div>
            ) : (
                <div className="space-y-4">
                    {/* Poll header */}
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
                            <span className="tabular-nums">{t('sharedPoll.votesSoFar', { count: totalVotes })}</span>
                        </div>
                    </div>

                    {/* Options with counts + voter names */}
                    <div className="space-y-3">
                        {poll.options.map(option => {
                            const count = option.vote_count ?? option.voters?.length ?? 0;
                            const pct = totalVotes > 0 ? Math.round((count / totalVotes) * 100) : 0;
                            return (
                                <div key={option.id} className="relative card p-4 overflow-hidden">
                                    <div className="absolute inset-y-0 left-0 bg-secondary/60" style={{ width: `${pct}%` }} />
                                    <div className="relative">
                                        <div className="flex items-center justify-between gap-2">
                                            <p className="text-[15px] font-semibold truncate">{option.name}</p>
                                            <span className="text-xs font-bold text-muted-foreground tabular-nums shrink-0">
                                                {count} · {pct}%
                                            </span>
                                        </div>
                                        {(option.voters?.length ?? 0) > 0 && (
                                            <div className="flex flex-wrap items-center gap-1.5 mt-2">
                                                {option.voters!.map(v => (
                                                    <span key={v.user_id} className="inline-flex items-center gap-1 pr-2 rounded-full bg-secondary/70 text-[11px] font-medium">
                                                        <UserAvatar name={v.full_name} size="xs" />
                                                        {v.full_name}
                                                    </span>
                                                ))}
                                            </div>
                                        )}
                                    </div>
                                </div>
                            );
                        })}
                    </div>

                    {/* CTA */}
                    <div className={cn('card p-4 text-center space-y-2')}>
                        <p className="text-sm font-semibold">{t('sharedPoll.wantLunch')}</p>
                        <p className="text-xs text-muted-foreground">
                            {loggedIn ? t('sharedPoll.openToVote') : t('sharedPoll.signInToVote')}
                        </p>
                        {loggedIn ? (
                            <Button asChild className="w-full h-12 rounded-xl font-semibold shadow-[0_4px_14px_rgba(249,115,22,0.35)]">
                                <Link to="/">{t('sharedPoll.openAndVote')}</Link>
                            </Button>
                        ) : (
                            <div className="grid grid-cols-2 gap-2">
                                <Button asChild variant="secondary" className="h-12 rounded-xl font-semibold">
                                    <Link to="/register">{t('sharedPoll.register')}</Link>
                                </Button>
                                <Button asChild className="h-12 ro