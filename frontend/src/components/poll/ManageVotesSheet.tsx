import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { useTranslation } from 'react-i18next';
import { Loader2, UserPlus, Lock } from 'lucide-react';
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetDescription } from '@/components/ui/sheet';
import { Button } from '@/components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import UserAvatar from '@/components/common/UserAvatar';
import * as usersService from '@/services/users.service';
import * as pollsService from '@/services/polls.service';
import { getErrorMessage } from '@/services/axios/AxiosBase';
import type { Poll } from '@/types';

interface ManageVotesSheetProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    poll: Poll;
    isPollOpen: boolean;
}

/** Admin sheet: per-option voter lists, manual votes, close poll. */
const ManageVotesSheet = ({ open, onOpenChange, poll, isPollOpen }: ManageVotesSheetProps) => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();
    const [manualUserId, setManualUserId] = useState<string>('');
    const [manualOptionId, setManualOptionId] = useState<string>('');

    const { data: usersRes } = useQuery({
        queryKey: ['users'],
        queryFn: usersService.getUsers,
        enabled: open,
    });
    const users = usersRes?.data ?? [];

    const votedUserIds = new Set(
        poll.options.flatMap(o => o.voters?.map(v => v.user_id) ?? [])
    );
    const nonVoters = users.filter(u => !votedUserIds.has(u.id));

    const invalidate = () => {
        queryClient.invalidateQueries({ queryKey: ['active-poll'] });
        queryClient.invalidateQueries({ queryKey: ['poll-results', poll.id] });
    };

    const manualVoteMutation = useMutation({
        mutationFn: () => pollsService.addManualVote(poll.id, {
            user_id: Number(manualUserId),
            poll_option_id: Number(manualOptionId),
        }),
        onSuccess: () => {
            toast.success(t('manageVotes.voteAddedToast'));
            setManualUserId('');
            setManualOptionId('');
            invalidate();
        },
        onError: (error: any) => toast.error(getErrorMessage(error, t('manageVotes.addVoteFailed')), { duration: 6000 }),
    });

    const closeMutation = useMutation({
        mutationFn: () => pollsService.closePoll(poll.id),
        onSuccess: () => {
            toast.success(t('manageVotes.pollClosedToast'));
            invalidate();
            onOpenChange(false);
        },
        onError: (error: any) => toast.error(getErrorMessage(error, t('manageVotes.closeFailed')), { duration: 6000 }),
    });

    return (
        <Sheet open={open} onOpenChange={onOpenChange}>
            <SheetContent side="bottom" className="rounded-t-3xl max-h-[88dvh] overflow-y-auto p-0">
                <SheetHeader className="px-5 pt-5 pb-3 text-left">
                    <SheetTitle>{t('manageVotes.title')}</SheetTitle>
                    <SheetDescription>{t('manageVotes.subtitle')}</SheetDescription>
                </SheetHeader>

                <div className="px-5 pb-8 space-y-5">
                    {/* Voter lists */}
                    <div className="space-y-3">
                        {poll.options.map(option => (
                            <div key={option.id} className="rounded-xl border bg-card p-3">
                                <div className="flex items-center justify-between mb-2">
                                    <p className="text-sm font-semibold">{option.name}</p>
                                    <span className="text-xs text-muted-foreground font-medium tabular-nums">
                                        {t('poll.votes', { count: option.vote_count ?? option.voters?.length ?? 0 })}
                                    </span>
                                </div>
                                {(option.voters?.length ?? 0) === 0 ? (
                                    <p className="text-xs text-muted-foreground">{t('manageVotes.noVotesYet')}</p>
                                ) : (
                                    <ul className="space-y-1.5">
                                        {option.voters!.map(v => (
                                            <li key={v.user_id} className="flex items-center gap-2">
                                                <UserAvatar name={v.full_name} size="xs" />
                                                <span className="text-xs text-foreground">{v.full_name}</span>
                                                {v.is_manual && (
                                                    <span className="text-[10px] px-1.5 py-0.5 rounded-full bg-amber-100 text-amber-700 font-semibold">{t('manageVotes.manual')}</span>
                                                )}
                                            </li>
                                        ))}
                                    </ul>
                                )}
                            </div>
                        ))}
                    </div>

                    {/* Add manual vote */}
                    {isPollOpen && (
                        <div className="rounded-xl border bg-card p-4 space-y-3">
                            <div className="flex items-center gap-2">
                                <UserPlus className="h-4 w-4 text-primary" />
                                <h3 className="text-sm font-semibold">{t('manageVotes.addVoteForUser')}</h3>
                            </div>
                            <p className="text-xs text-muted-foreground">
                                {t('manageVotes.addVoteHint')}
                            </p>
                            <Select value={manualUserId} onValueChange={setManualUserId}>
                                <SelectTrigger className="h-11 rounded-xl">
                                    <SelectValue placeholder={t('manageVotes.selectUser')} />
                                </SelectTrigger>
                                <SelectContent>
                                    {(nonVoters.length > 0 ? nonVoters : users).map(u => (
                                        <SelectItem key={u.id} value={String(u.id)}>{u.full_name}</SelectItem>
                                    ))}
                                </SelectContent>
                            </Select>
                            <Select value={manualOptionId} onValueChange={setManualOptionId}>
                                <SelectTrigger className="h-11 rounded-xl">
                                    <SelectValue placeholder={t('manageVotes.selectOption')} />
                                </SelectTrigger>
                                <SelectContent>
                                    {poll.options.map(o => (
                                        <SelectItem key={o.id} value={String(o.id)}>{o.name}</SelectItem>
                                    ))}
                                </SelectContent>
                            </Select>
                            <Button
                                className="w-full h-11 rounded-xl font-semibold"
                                disabled={!manualUserId || !manualOptionId || manualVoteMutation.isPending}
                                onClick={() => manualVoteMutation.mutate()}
                            >
                                {manualVoteMutation.isPending
                                    ? (<><Loader2 className="h-4 w-4 animate-spin" /> {t('manageVotes.adding')}</>)
                                    : t('manageVotes.addVote')}
                            </Button>
                        </div>
                    )}

                    {/* Close poll */}
                    {isPollOpen && (
                        <Button
                            variant="destructive"
                            className="w-full h-11 rounded-xl font-semibold"
                            disabled={closeMutation.isPending}
                            onClick={() => closeMutation.mutate()}
                        >
                            {closeMutation.isPending
                                ? (<><Loader2 className="h-4 w-4 animate-spin" /> {t('manageVotes.closing')}</>)
                                : (<><Lock className="h-4 w-4" /> {t('manageVotes.closePoll')}</>)}
                        </Button>
                    )}
                </div>
            </SheetContent>
        </Sheet>
    );
};

export default ManageVotesSheet;
