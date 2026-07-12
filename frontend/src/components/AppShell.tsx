import { useState } from 'react';
import { Outlet, NavLink, useLocation } from 'react-router-dom';
import { Home, UtensilsCrossed, Wallet, Receipt, User, Bell } from 'lucide-react';
import { useQuery } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import * as notificationsService from '@/services/notifications.service';
import NotificationsSheet from './NotificationsSheet';
import LanguageToggle from '@/components/common/LanguageToggle';
import { cn } from '@/lib/utils';

const tabs = [
    { to: '/', labelKey: 'tabs.home', icon: Home },
    { to: '/menu', labelKey: 'tabs.menu', icon: UtensilsCrossed },
    { to: '/payments', labelKey: 'tabs.payments', icon: Wallet },
    { to: '/dues', labelKey: 'tabs.dues', icon: Receipt },
    { to: '/profile', labelKey: 'tabs.profile', icon: User },
];

const pageTitleKeys: Record<string, string> = {
    '/': 'titles.home',
    '/menu': 'titles.menu',
    '/payments': 'titles.payments',
    '/dues': 'titles.dues',
    '/profile': 'titles.profile',
    '/settings': 'titles.settings',
};

const AppShell = () => {
    const { t } = useTranslation();
    const location = useLocation();
    const [notifOpen, setNotifOpen] = useState(false);
    const title = t(pageTitleKeys[location.pathname] ?? 'titles.home');

    const { data: notifRes } = useQuery({
        queryKey: ['notifications'],
        queryFn: notificationsService.getNotifications,
        refetchInterval: 60000,
    });
    const notifications = notifRes?.data ?? [];
    const unreadCount = notifications.filter(n => !n.is_read).length;

    return (
        <div className="app-shell">
            {/* Sticky top header */}
            <header className="app-header">
                <div className="flex items-center justify-between h-14 px-4">
                    <div className="flex items-center gap-2">
                        {location.pathname === '/' && (
                            <span className="w-8 h-8 rounded-xl bg-primary text-primary-foreground flex items-center justify-center text-base shadow-sm">
                                🍱
                            </span>
                        )}
                        <h1 className="text-lg font-bold tracking-tight">{title}</h1>
                    </div>
                    <div className="flex items-center gap-1.5">
                        <LanguageToggle />
                        <button
                            onClick={() => setNotifOpen(true)}
                            className="relative w-10 h-10 rounded-full flex items-center justify-center hover:bg-secondary active:scale-95"
                            aria-label={t('common.notifications')}
                        >
                            <Bell className="h-5 w-5 text-foreground" />
                            {unreadCount > 0 && (
                                <span className="absolute top-1 right-1 min-w-[18px] h-[18px] px-1 rounded-full bg-destructive text-white text-[10px] font-bold flex items-center justify-center">
                                    {unreadCount > 9 ? '9+' : unreadCount}
                                </span>
                            )}
                        </button>
                    </div>
                </div>
            </header>

            {/* Page content */}
            <main className="px-4 pt-4 pb-28 animate-fade-in">
                <Outlet />
            </main>

            {/* Fixed bottom tab bar */}
            <nav className="app-tabbar">
                <div className="grid grid-cols-5">
                    {tabs.map(tab => (
                        <NavLink
                            key={tab.to}
                            to={tab.to}
                            end={tab.to === '/'}
                            className={({ isActive }) =>
                                cn(
                                    'flex flex-col items-center justify-center gap-0.5 py-2.5 min-h-[56px]',
                                    isActive ? 'text-primary' : 'text-muted-foreground'
                                )
                            }
                        >
                            {({ isActive }) => (
                                <>
                                    <tab.icon className={cn('h-5 w-5', isActive && 'fill-primary/15')} strokeWidth={isActive ? 2.4 : 1.9} />
                                    <span className={cn('text-[10px] leading-none', isActive ? 'font-semibold' : 'font-medium')}>
                                        {t(tab.labelKey)}
                                    </span>
                                </>
                            )}
                        </NavLink>
                    ))}
                </div>
            </nav>

            <NotificationsSheet open={notifOpen} onOpenChange={setNotifOpen} notifications={notifications} />
        </div>
    );
};

export default AppShell;
