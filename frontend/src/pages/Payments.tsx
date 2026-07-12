import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { useTranslation } from 'react-i18next';
import { Check, Loader2, Plus, Wallet, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Textarea } from '@/components/ui/textarea';
import StatusBadge from '@/components/common/StatusBadge';
import SubmitPaymentSheet from '@/components/payments/SubmitPaymentSheet';
import UserAvatar from '@/components/common/UserAvatar';
import * as paymentsService from '@/services/payments.service';
import { getErrorMessage } from '@/services/axios/AxiosBase';
import { BASE_URL } from '@/lib/config';
import { isAdmin } from '@/lib/auth';
import { formatBDT, formatDateTime, paymentStatusLabel } from '@/lib/format';
import { PAYMENT_STATUS } from '@/lib/constants';
import type { Payment } from '@/types';

const screenshotUrl = (path?: string | null): string | null => {
    if (!path) return null;
    if (path.startsWith('http') || path.startsWith('data:')) return path;
    return `${BASE_URL}${path.startsWith('/') ? '' : '/'}${path}`;
};

const PaymentCard = ({ payment, adminView, onApprove, onReject, busy }: {
    payment: Payment;
    adminView?: boolean;
    onApprove?: () => void;
    onReject?: () => void;
    busy?: boolean;
}) => {
    const { t } = useTranslation();
    const [previewOpen, setPreviewOpen] = useState(false);
    const url = screenshotUrl(payment.screenshot_path);
    const isPending = paymentStatusLabel(payment.status) === PAYMENT_STATUS.PENDING;

    return (
        <div className="card p-4 space-y-3">
            <div className="flex items-start justify-between gap-2">
                <div className="min-w-0">
                    {adminView && (
                        <div className="flex items-center gap-2 mb-1">
                            <UserAvatar name={payment.submitted_by_name} size="xs" />
                            <p className="text-xs font-semibold truncate">{payment.submitted_by_name}</p>
                        </div>
                    )}
                    <p className="text-lg font-extrabold tabular-nums">{formatBDT(payment.total_amount)}</p>
                    <p className="text-[11px] text-muted-foreground">{formatDateTime(payment.created_at)}</p>
                </div>
                <StatusBadge status={payment.status} />
            </div>

            {payment.allocations?.length > 0 && (
                <ul className="space-y-1">
                    {payment.allocations.map((a, i) => (
                        <li key={a.id ?? i} className="flex items-center justify-between text-xs bg-secondary/50 rounded-lg px-2.5 py-1.5">
                            <span className="font-medium truncate">{a.beneficiary_name ?? t('payments.userNumber', { id: a.beneficiary_user_id })}</span>
                            <span className="text-muted-foreground tabular-nums shrink-0">
                                {t('payments.days', { count: a.days })}{a.amount != null ? ` · ${formatBDT(a.amount)}` : ''}
                            </span>
                        </li>
                    ))}
                </ul>
            )}

            {payment.note && <p className="text-xs text-muted-foreground italic">"{payment.note}"</p>}

            <div className="flex items-center gap-2">
                {url && (
                    <>
                        <button onClick={() => setPreviewOpen(true)} className="shrink-0">
                            <img src={url} alt={t('payments.screenshotAlt')} className="w-14 h-14 object-cover rounded-lg border" />
                        </button>
                        <Dialog open={previewOpen} onOpenChange={setPreviewOpen}>
                            <DialogContent className="max-w-md rounded-2xl p-2">
                                <img src={url} alt={t('payments.screenshotAlt')} className="w-full rounded-xl" />
                            </DialogContent>
                        </Dialog>
                    </>
                )}
                {adminView && isPending && (
                    <div className="flex gap-2 flex-1 justify-end">
                        <Button variant="outline" size="sm" className="h-10 rounded-xl border-red-200 text-red-600 hover:bg-red-50"
                            disabled={busy} onClick={onReject}>
                            <X className="h-4 w-4" /> {t('payments.reject')}
                        </Button>
                        <Button size="sm" className="h-10 rounded-xl bg-green-600 hover:bg-green-700"
                            disabled={busy} onClick={onApprove}>
                            <Check className="h-4 w-4" /> {t('payments.approve')}
                        </Button>
                    </div>
                )}
            </div>
        </div>
    );
};

const Payments = () => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();
    const admin = isAdmin();
    const [submitOpen, setSubmitOpen] = useState(false);
    const [rejectTarget, setRejectTarget] = useState<Payment | null>(null);
    const [rejectReason, setRejectReason] = useState('');

    const { data: myRes, isLoading: myLoading } = useQuery({
        queryKey: ['my-payments'],
        queryFn: paymentsService.getMyPayments,
    });
    const myPayments = myRes?.data ?? [];

    const { data: pendingRes, isLoading: pendingLoading } = useQuery({
        queryKey: ['payments', 'pending'],
        queryFn: () => paymentsService.getPayments(PAYMENT_STATUS.PENDING),
        enabled: admin,
    });
    const pendingPayments = pendingRes?.data ?? [];

    const invalidate = () => {
        queryClient.invalidateQueries({ queryKey: ['payments'] });
        queryClient.invalidateQueries({ queryKey: ['my-payments'] });
        queryClient.invalidateQueries({ queryKey: ['all-dues'] });
    };

    const approveMutation = useMutation({
        mutationFn: (id: number) => paymentsService.approvePayment(id),
        onSuccess: () => { toast.success(t('payments.approvedToast')); invalidate(); },
        onError: (error: any) => toast.error(getErrorMessage(error, t('payments.approveFailed')), { duration: 6000 }),
    });

    const rejectMutation = useMutation({
        mutationFn: ({ id, reason }: { id: number; reason?: string }) => paymentsService.rejectPayment(id, reason),
        onSuccess: () => {
            toast.success(t('payments.rejectedToast'));
            setRejectTarget(null);
            setRejectReason('');
            invalidate();
        },
        onError: (error: any) => toast.error(getErrorMessage(error, t('payments.rejectFailed')), { duration: 6000 }),
    });

    const skeletons = (
        <div className="space-y-3">
            {[1, 2, 3].map(i => <Skeleton key={i} className="h-32 rounded-2xl" />)}
        </div>
    );

    const myList = myLoading ? skeletons : myPayments.length === 0 ? (
        <div className="empty-state">
            <Wallet />
            <h3>{t('payments.emptyTitle')}</h3>
            <p>{t('payments.emptyBody')}</p>
        </div>
    ) : (
        <div className="space-y-3">
            {myPayments.map(p => <PaymentCard key={p.id} payment={p} />)}
        </div>
    );

    return (
        <div className="space-y-4">
            <Button
                className="w-full h-12 rounded-xl font-semibold shadow-[0_4px_14px_rgba(249,115,22,0.35)]"
                onClick={() => setSubmitOpen(true)}
            >
                <Plus className="h-4 w-4" /> {t('payments.submitPayment')}
            </Button>

            {admin ? (
                <Tabs defaultValue="my">
                    <TabsList className="grid grid-cols-2 w-full h-11 rounded-xl">
                        <TabsTrigger value="my" className="rounded-lg">{t('payments.myPayments')}</TabsTrigger>
                        <TabsTrigger value="approvals" className="rounded-lg">
                            {t('payments.approvals')}
                            {pendingPayments.length > 0 && (
                                <span className="ml-1.5 min-w-[18px] h-[18px] px-1 rounded-full bg-destructive text-white text-[10px] font-bold inline-flex items-center justify-center">
                                    {pendingPayments.length}
                                </span>
                            )}
                        </TabsTrigger>
                    </TabsList>
                    <TabsContent value="my" className="mt-4">{myList}</TabsContent>
                    <TabsContent value="approvals" className="mt-4">
                        {pendingLoading ? skeletons : pendingPayments.length === 0 ? (
                            <div className="empty-state">
                                <Check />
                                <h3>{t('payments.queueClearTitle')}</h3>
                                <p>{t('payments.queueClearBody')}</p>
                            </div>
                        ) : (
                            <div className="space-y-3">
                                {pendingPayments.map(p => (
                                    <PaymentCard
                                        key={p.id}
                                        payment={p}
                                        adminView
                                        busy={approveMutation.isPending || rejectMutation.isPending}
                                        onApprove={() => approveMutation.mutate(p.id)}
                                        onReject={() => setRejectTarget(p)}
                                    />
                                ))}
                            </div>
                        )}
                    </TabsContent>
                </Tabs>
            ) : (
                myList
            )}

            <SubmitPaymentSheet open={submitOpen} onOpenChange={setSubmitOpen} />

            {/* Reject dialog */}
            <Dialog open={!!rejectTarget} onOpenChange={(o) => !o && setRejectTarget(null)}>
                <DialogContent className="max-w-md rounded-2xl">
                    <DialogHeader className="text-left">
                        <DialogTitle>{t('payments.rejectPayment')}</DialogTitle>
                    </DialogHeader>
                    <div className="space-y-3">
                        <p className="text-sm text-muted-foreground">
                            {t('payments.rejectingInfo', { amount: formatBDT(rejectTarget?.total_amount ?? 0), name: rejectTarget?.submitted_by_name })}
                        </p>
                        <Textarea
                            value={rejectReason}
                            onChange={e => setRejectReason(e.target.value)}
                            placeholder={t('payments.rejectReasonPlaceholder')}
                            rows={2}
                            className="rounded-xl"
                        />
                        <Button
                            variant="destructive"
                            className="w-full h-11 rounded-xl font-semibold"
                            disabled={rejectMutation.isPending}
                            onClick={() => rejectTarget && rejectMutation.mutate({ id: rejectTarget.id, reason: rejectReason.trim() || undefined })}
                        >
                            {rejectMutation.isPending ? (<><Loader2 className="h-4 w-4 animate-spin" /> {t('payments.rejecting')}</>) : t('payments.rejectPayment')}
                        </Button>
                    </div>
                </DialogContent>
            </Dialog>
        </div>
    );
};

export default Payments;
