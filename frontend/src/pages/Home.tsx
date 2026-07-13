import { useEffect, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { CalendarPlus, ChevronDown, Plus, Users, Vote } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import LunchPollCard from '@/components/poll/LunchPollCard';
import PublishPollDialog from '@/components/poll/PublishPollDialog';
import GeneralPollCard from '@/components/poll/GeneralPollCard';
import * as pollsService from '@/services/polls.service';
import * as settingsService from '@/services/settings.service';
import { isAdmin } from '@/lib/auth';
import { formatDate, pollStatusLabel } from '@/lib/format';
import { POLL_STATUS } from '@/lib/constants';
import { cn } from '@/lib/utils';
import type { AppSettings, Poll } from '@/types';

interface LunchAccordionItemProps {
    poll: Poll;
    admin: boolean;
    settings?: AppSettings;
    expanded: boolean;
    onToggle: () => void;
}

/** One row of the lunch-poll accordion: a compact summary that expands to the full card. */
const LunchAccordionItem = ({ poll, admin, settings, expanded, onToggle }: LunchAccordionItemProps) => {
    const { t } = useTranslation();
    const isOpen = pollStatusLabel(poll.status) === POLL_STATUS.OPEN;
    const totalVotes = poll.total_votes
        ?? poll.options.reduce((sum, o) => sum + (o.vote_count ?? o.voters?.length ?? 0), 0);
    const myChoice = poll.my_vote_option_id != null
        ? poll.options.find(o => o.id === poll.my_vote_option_id)?.name
        : null;

    return (
        <div className="rounded-2xl border bg-card overflow-hidden">
            <button
                type="button"
                onClick={onToggle}
                aria-expanded={expanded}
                aria-label={expanded ? t('home.collapsePoll') : t('home.expandPoll')}
                className="w-full flex items-center gap-3 p-4 text-left active:bg-secondary/40 transition-colors"
            >
                <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 flex-wrap">
                        <span className="text-sm font-bold">{formatDate(poll.lunch_date)}</span>
                        <span className={cn(
                            'inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-semibold',
                            isOpen ? 'bg-emerald-100 text-emerald-700' : 'bg-muted text-muted-foreground'
                        )}>
                            {isOpen ? t('home.statusOpen') : t('home.statusClosed')}
                        </span>
                    </div>
                    <div className="flex items-center gap-2 mt-1 text-[11px] text-muted-foreground">
                        <span className="inline-flex items-center gap-1 tabular-nums">
                            <Users className="h-3 w-3" /> {t('home.votedCount', { count: totalVotes })}
                        </span>
                        {myChoice && <span className="truncate">· {t('home.votedFor', { name: myChoice })}</span>}
                    </div>
                </div>
                <ChevronDown className={cn('h-5 w-5 text-muted-foreground shrink-0 transition-transform', expanded && 'rotate-180')} />
            </button>
            {expanded && (
                <div className="px-4 pb-4">
                    <LunchPollCard poll={poll} admin={admin} settings={settings} />
                </div>
            )}
        </div>
    );
};

const Home = () => {
    const { t } = useTranslation();
    const admin = isAdmin();
    const [publishOpen, setPublishOpen] = useState(false);
    const [expanded, setExpanded] = useState<Set<number>>(new Set());
    const [seeded, setSeeded] = useState(false);
    const [, setTick] = useState(0);

    // re-render every second so voting locks exactly at cutoff (per card)
    useEffect(() => {
        const timer = setInterval(() => setTick(x => x + 1), 1000);
        return () => clearInterval(timer);
    }, []);

    // The most recent 10 lunch polls, newest first — each a full poll.
    const { data: lunchRes, isLoading } = useQuery({
        queryKey: ['lunch-recent'],
        queryFn: pollsService.getRecentLunchPolls,
        refetchInterval: 30000, // live vote counts
    });
    const lunchPolls = lunchRes?.data ?? [];

    // Open General polls (never affect dues) shown below the lunch list.
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

    // Expand the newest poll by default, once the list first loads.
    useEffect(() => {
        if (!seeded && lunchPolls.length > 0) {
            setExpanded(new Set([lunchPolls[0].id]));
            setSeeded(true);
        }
    }, [seeded, lunchPolls]);

    const toggle = (id: number) => {
        setExpanded(prev => {
            const next = new Set(prev);
            if (next.has(id)) next.delete(id);
            else next.add(id);
            return next;
        });
    };

    // "Other polls" section — only rendered when at least one General poll is open.
    const generalSection = generalPolls.length > 0 ? (
        <div className="space-y-3 pt-2">
            <h2 className="text-xs font-bold text-muted-foreground uppercase tracking-wide px-1">{t('home.otherPolls')}</h2>
            {generalPolls.map(gp => (
                <GeneralPollCard key={gp.id} poll={gp} admin={admin} />
            ))}
        </div>
    ) : null;

    // Floating action button — admin only — to publish a new lunch poll.
    const fab = admin ? (
        <button
            type="button"
            onClick={() => setPublishOpen(true)}
            aria-label={t('home.addPoll')}
            className="fixed bottom-20 right-4 z-30 h-14 w-14 rounded-full bg-primary text-primary-foreground shadow-[0_6px_20px_rgba(249,115,22,0.45)] flex items-center justify-center active:scale-95 transition-transform"
        >
            <Plus className="h-6 w-6" strokeWidth={2.5} />
        </button>
    ) : null;

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

    if (lunchPolls.length === 0) {
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
                </div>
                {generalSection}
                {fab}
                <PublishPollDialog open={publishOpen} onOpenChange={setPublishOpen} />
            </div>
        );
    }

    return (
        <div className="space-y-4">
            <div className="space-y-3">
                {lunchPolls.map(poll => (
                    <LunchAccordionItem
                        key={poll.id}
                        poll={poll}
                        admin={admin}
                        settings={settings}
                        expanded={expanded.has(poll.id)}
                        onToggle={() => toggle(poll.id)}
                    />
                ))}
            </div>

            {generalSection}
            {fab}
            <PublishPollDialog open={publishOpen} onOpenChange={setPublishOpen} />
        </div>
    );
};

export default Home;
