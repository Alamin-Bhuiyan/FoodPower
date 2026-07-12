import { Sheet, SheetContent, SheetHeader, SheetTitle } from '@/components/ui/sheet';
import { Button } from '@/components/ui/button';
import { BellOff, CheckCheck } from 'lucide-react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import * as notificationsService from '@/services/notifications.service';
import { formatDateTime } from '@/lib/format';
import { cn } from '@/lib/utils';
import type { AppNotification } from '@/types';

interface NotificationsSheetProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    notifications: AppNotification[];
}

const NotificationsSheet = ({ open, onOpenChange, notifications }: NotificationsSheetProps) => {
    const { t } = useTranslation();
    const queryClient = useQueryClient();

    const markAllMutation = useMutation({
        mutationFn: notificationsService.markAllRead,
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['notifications'] }),
    });

    const markOneMutation = useMutation({
        mutationFn: (id: number) => notificationsService.markRead(id),
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['notifications'] }),
    });

    const unread = notifications.filter(n => !n.is_read).length;

    return (
        <Sheet open={open} onOpenChange={onOpenChange}>
            <SheetContent side="right" className="w-full sm:max-w-md p-0 flex flex-col">
                <SheetHeader className="px-4 pt-4 pb-2 border-b text-left">
                    <div className="flex items-center justify-between">
                        <SheetTitle>{t('notificationsSheet.title')}</SheetTitle>
                        {unread > 0 && (
                            <Button
                                variant="ghost"
                                size="sm"
                                className="text-xs text-primary mr-6"
                                onClick={() => markAllMutation.mutate()}
                                disabled={markAllMutation.isPending}
                            >
                                <CheckCheck className="h-3.5 w-3.5 mr-1" /> {t('notificationsSheet.markAllRead')}
                            </Button>
                        )}
                    </div>
                </SheetHeader>
                <div className="flex-1 overflow-y-auto">
                    {notifications.length === 0 ? (
                        <div className="empty-state">
                            <BellOff />
                            <h3>{t('notificationsSheet.emptyTitle')}</h3>
                            <p>{t('notificationsSheet.emptyBody')}</p>
                        </div>
                    ) : (
                        <ul className="divide-y">
                            {notifications.map(n => (
                                <li
                                    key={n.id}
                                    className={cn(
                                        'px-4 py-3 cursor-pointer active:bg-secondary/60',
                                        !n.is_read && 'bg-primary/5'
                                    )}
                                    onClick={() => !n.is_read && markOneMutation.mutate(n.id)}
                                >
                                    <div className="flex items-start gap-2">
                                        {!n.is_read && <span className="mt-1.5 w-2 h-2 rounded-full bg-primary shrink-0" />}
                                        <div className="min-w-0">
                                            <p className="text-sm font-semibold text-foreground">{n.title}</p>
                                            {n.body && <p className="text-xs text-muted-foreground mt-0.5">{n.body}</p>}
                                            <p className="text-[11px] text-muted-foreground mt-1">{formatDateTime(n.created_at)}</p>
                                        </div>
                                    </div>
                                </li>
                            ))}
                        </ul>
                    )}
                </div>
            </SheetContent>
        </Sheet>
    );
};

export default NotificationsSheet;
