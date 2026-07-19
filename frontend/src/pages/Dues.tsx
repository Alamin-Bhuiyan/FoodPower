import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { CheckCircle2, ChevronLeft, ChevronRight, ChevronRight as Chevron, Receipt, UtensilsCrossed, Wallet, XCircle } from 'lucide-react';
import { Skeleton } from '@/components/ui/skeleton';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Sheet, SheetContent, SheetHeader, SheetTitle } from '@/components/ui/sheet';
import UserAvatar from '@/components/common/UserAvatar';
import * as duesService from '@/services/dues.service';
import { isAdmin } from '@/lib/auth';
import { formatBDT, formatDate, formatDateShort, formatDateTime, currentWeekStart, addDays } from '@/lib/format';
import { cn } from '@/lib/utils';
import type { MyDues, UserDues } from '@/types';

const BalanceCard = ({ balance, lunchCount, loading }: { balance: number; lunchCount: number; loading?: boolean }) => {
    const { t } = useTranslation();
    if (loading) return <Skeleton className="h-32 rounded-2xl" />;
    const isDue = balance < 0;
    return (
        <div className={cn(
            'rounded-2xl p-5 text-white shadow-lg',
            isDue
                ? 'bg-gradient-to-br from-red-500 to-rose-600'
                : 'bg-gradient-to-br from-green-500 to-emerald-600'
        )}>
            <p className="text-xs font-semibold uppercase tracking-wider opacity-90">
                {isDue ? t('dues.amountDue') : t('dues.advanceBalance')}
            </p>
            <p className="text-3xl font-extrabold mt-1 tabular-nums">{formatBDT(Math.abs(balance))}</p>
            <div className="flex items-center gap-2 mt-3 text-xs opacity-90">
                <UtensilsCrossed className="h-3.5 w-3.5" />
                <span>{t('dues.lunchesSoFar', { count: lunchCount })}</span>
            </div>
            <p className="text-[11px] mt-2 opacity-75">
                {isDue ? t('dues.dueNote') : t('dues.advanceNote')}
            </p>
        </div>
    );
};

/** Balance card + chronological history — shared by "my dues" and the admin per-user detail. */
const DuesDetail = ({ dues, isLoading }: { dues?: MyDues; isLoading?: boolean }) => {
    const { t } = useTranslation();
    const history = dues?.history ?? [];

    return (
        <div className="space-y-4">
            <BalanceCard balance={dues?.balance ?? 0} lunchCount={dues?.lunch_count ?? 0} loading={isLoading} />

            <div>
                <h3 className="text-sm font-bold mb-2">{t('dues.history')}</h3>
                {isLoading ? (
                    <div className="space-y-2">{[1, 2, 3, 4].map(i => <Skeleton key={i} className="h-14 rounded-xl" />)}</div>
                ) : history.length === 0 ? (
                    <div className="empty-state">
                        <Receipt />
                        <h3>{t('dues.emptyTitle')}</h3>
                        <p>{t('dues.emptyBody')}</p>
                    </div>
                ) : (
                    <ul className="space-y-2">
                        {history.map((h, i) => {
                            const isCredit = h.amount > 0 || (h.type ?? '').toLowerCase().includes('payment');
                            return (
                                <li key={i} className="card px-3.5 py-3 flex items-center gap-3">
                                    <span className={cn(
                                        'w-9 h-9 rounded-xl flex items-center justify-center shrink-0',
                                        isCredit ? 'bg-green-100 text-green-600' : 'bg-orange-100 text-orange-600'
                                    )}>
                                        {isCredit ? <Wallet className="h-4 w-4" /> : <UtensilsCrossed className="h-4 w-4" />}
                                    </span>
                                    <div className="flex-1 min-w-0">
                                        <p className="text-sm font-semibold truncate">
                                            {h.description || (isCredit ? t('dues.paymentApproved') : t('dues.lunch'))}
                                        </p>
                                        <p className="text-[11px] text-muted-foreground">{isCredit ? formatDateTime(h.date) : formatDate(h.date)}</p>
                                    </div>
                                    <span className={cn(
                                        'text-sm font-bold tabular-nums shrink-0',
                                        isCredit ? 'text-green-600' : 'text-red-500'
                                    )}>
                                        {isCredit ? '+' : '−'}{formatBDT(Math.abs(h.amount))}
                                    </span>
                                </li>
                            );
                        })}
                    </ul>
                )}
            </div>
        </div>
    );
};

const MyDuesView = () => {
    const { data: duesRes, isLoading } = useQuery({
        queryKey: ['my-dues'],
        queryFn: duesService.getMyDues,
    });

    return <DuesDetail dues={duesRes?.data} isLoading={isLoading} />;
};

/** Admin: full dues breakdown + history for a selected user, in a side sheet. */
const UserDuesSheet = ({ user, onClose }: { user: UserDues | null; onClose: () => void }) => {
    const { data, isLoading } = useQuery({
        queryKey: ['user-dues', user?.user_id],
        queryFn: () => duesService.getUserDues(user!.user_id),
        enabled: !!user,
    });

    return (
        <Sheet open={!!user} onOpenChange={(o) => { if (!o) onClose(); }}>
            <SheetContent side="right" className="w-full sm:max-w-md overflow-y-auto">
                <SheetHeader className="text-left">
                    <SheetTitle className="flex items-center gap-2.5">
                        {user && <UserAvatar name={user.full_name} size="md" />}
                        {user?.full_name}
                    </SheetTitle>
                </SheetHeader>
                <div className="mt-4">
                    <DuesDetail dues={data?.data} isLoading={isLoading} />
                </div>
            </SheetContent>
        </Sheet>
    );
};

const AllUsersView = () => {
    const { t } = useTranslation();
    const [selected, setSelected] = useState<UserDues | null>(null);
    const { data: allRes, isLoading } = useQuery({
        queryKey: ['all-dues'],
        queryFn: duesService.getAllDues,
    });
    const rows = allRes?.data ?? [];

    if (isLoading) {
        return <div className="space-y-2">{[1, 2, 3, 4].map(i => <Skeleton key={i} className="h-16 rounded-xl" />)}</div>;
    }

    return (
        <>
            <ul className="space-y-2">
                {rows.map(u => (
                    <li key={u.user_id}>
                        <button
                            type="button"
                            onClick={() => setSelected(u)}
                            className="card w-full px-3.5 py-3 flex items-center gap-3 text-left active:scale-[0.99] transition"
                        >
                            <UserAvatar name={u.full_name} size="md" />
                            <div className="flex-1 min-w-0">
                                <p className="text-sm font-semibold truncate">{u.full_name}</p>
                                <p className="text-[11px] text-muted-foreground tabular-nums">
                                    {t('dues.lunchesPaid', { count: u.lunch_count, amount: formatBDT(u.total_paid) })}
                                </p>
                            </div>
                            <div className="text-right shrink-0">
                                <p className={cn('text-sm font-bold tabular-nums', u.balance < 0 ? 'text-red-500' : 'text-green-600')}>
                                    {formatBDT(u.balance)}
                                </p>
                                <p className="text-[10px] text-muted-foreground">{u.balance < 0 ? t('dues.due') : t('dues.advance')}</p>
                            </div>
                            <Chevron className="h-4 w-4 text-muted-foreground shrink-0" />
                        </button>
                    </li>
                ))}
            </ul>
            <UserDuesSheet user={selected} onClose={() => setSelected(null)} />
        </>
    );
};

const WeeklySummaryView = () => {
    const { t } = useTranslation();
    const [weekStart, setWeekStart] = useState(currentWeekStart());

    const { data: weekRes, isLoading } = useQuery({
        queryKey: ['weekly-summary', weekStart],
        queryFn: () => duesService.getWeeklySummary(weekStart),
    });
    const summary = weekRes?.data;
    const rows = summary?.rows ?? [];
    // The lunch week runs Monday–Friday; Friday = Monday + 4.
    const weekEnd = addDays(weekStart, 4);

    return (
        <div className="space-y-3">
            {/* Week picker */}
            <div className="card px-3 py-2.5 flex items-center justify-between">
                <button className="w-9 h-9 rounded-lg hover:bg-secondary flex items-center justify-center"
                    onClick={() => setWeekStart(w => addDays(w, -7))} aria-label={t('dues.previousWeek')}>
                    <ChevronLeft className="h-4 w-4" />
                </button>
                <div className="text-center">
                    <p className="text-sm font-bold tabular-nums">{formatDateShort(weekStart)} – {formatDateShort(weekEnd)}</p>
                    <p className="text-[10px] text-muted-foreground">{t('dues.lunchWeek')}</p>
                </div>
                <button className="w-9 h-9 rounded-lg hover:bg-secondary flex items-center justify-center"
                    onClick={() => setWeekStart(w => addDays(w, 7))} aria-label={t('dues.nextWeek')}>
                    <ChevronRight className="h-4 w-4" />
                </button>
            </div>

            {isLoading ? (
                <div className="space-y-2">{[1, 2, 3].map(i => <Skeleton key={i} className="h-14 rounded-xl" />)}</div>
            ) : rows.length === 0 ? (
                <div className="empty-state">
                    <Receipt />
                    <h3>{t('dues.noLunchesTitle')}</h3>
                    <p>{t('dues.noLunchesBody')}</p>
                </div>
            ) : (
                <ul className="space-y-2">
                    {rows.map(r => (
                        <li key={r.user_id} className="card px-3.5 py-3 flex items-center gap-3">
                            <UserAvatar name={r.full_name} size="md" />
                            <div className="flex-1 min-w-0">
                                <p className="text-sm font-semibold truncate">{r.full_name}</p>
                                <p className="text-[11px] text-muted-foreground tabular-nums">
                                    {t('dues.weeklyCalc', { count: r.lunch_count, amount: formatBDT(r.amount) })}
                                </p>
                            </div>
                            {r.paid ? (
                                <span className="inline-flex items-center gap-1 text-green-600 text-xs font-semibold shrink-0">
                                    <CheckCircle2 className="h-4 w-4" /> {t('dues.paid')}
                                </span>
                            ) : (
                                <span className="inline-flex items-center gap-1 text-red-500 text-xs font-semibold shrink-0">
                                    <XCircle className="h-4 w-4" /> {t('dues.unpaid')}
                                </span>
                            )}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
};

const Dues = () => {
    const { t } = useTranslation();
    const admin = isAdmin();

    if (!admin) return <MyDuesView />;

    return (
        <Tabs defaultValue="my">
            <TabsList className="grid grid-cols-3 w-full h-11 rounded-xl">
                <TabsTrigger value="my" className="rounded-lg">{t('dues.mine')}</TabsTrigger>
                <TabsTrigger value="all" className="rounded-lg">{t('dues.everyone')}</TabsTrigger>
                <TabsTrigger value="weekly" className="rounded-lg">{t('dues.weekly')}</TabsTrigger>
            </TabsList>
            <TabsContent value="my" className="mt-4"><MyDuesView /></TabsContent>
            <TabsContent value="all" className="mt-4"><AllUsersView /></TabsContent>
            <TabsContent value="weekly" className="mt-4"><WeeklySummaryView /></TabsContent>
        </Tabs>
    );
};

export default Dues;
