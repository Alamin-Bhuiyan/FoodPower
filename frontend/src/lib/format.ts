import { PAYMENT_STATUS, POLL_STATUS } from './constants';
import { dateLocale } from '@/i18n';

/** Format an amount in BDT, e.g. "৳120.00". */
export const formatBDT = (amount: number | string | null | undefined): string => {
    const n = Number(amount ?? 0);
    const formatted = new Intl.NumberFormat('en-BD', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2,
    }).format(Math.abs(n));
    return `${n < 0 ? '-' : ''}৳${formatted}`;
};

export const formatDate = (value?: string | null): string => {
    if (!value) return '—';
    const d = new Date(value);
    if (isNaN(d.getTime())) return '—';
    return d.toLocaleDateString(dateLocale(), { weekday: 'short', day: 'numeric', month: 'short', year: 'numeric' });
};

export const formatDateShort = (value?: string | null): string => {
    if (!value) return '—';
    const d = new Date(value);
    if (isNaN(d.getTime())) return '—';
    return d.toLocaleDateString(dateLocale(), { day: 'numeric', month: 'short' });
};

export const formatDateTime = (value?: string | null): string => {
    if (!value) return '—';
    const d = new Date(value);
    if (isNaN(d.getTime())) return '—';
    return `${d.toLocaleDateString(dateLocale(), { day: 'numeric', month: 'short' })}, ${d.toLocaleTimeString(dateLocale(), { hour: '2-digit', minute: '2-digit', hour12: false })}`;
};

export const formatTime = (value?: string | null): string => {
    if (!value) return '—';
    const d = new Date(value);
    if (isNaN(d.getTime())) return '—';
    return d.toLocaleTimeString(dateLocale(), { hour: '2-digit', minute: '2-digit', hour12: false });
};

/** Normalize a payment/poll status that may arrive as a string or an enum int. */
export const paymentStatusLabel = (status: string | number | undefined | null): string => {
    if (typeof status === 'string') return status;
    switch (status) {
        case 0: return PAYMENT_STATUS.PENDING;
        case 1: return PAYMENT_STATUS.APPROVED;
        case 2: return PAYMENT_STATUS.REJECTED;
        default: return String(status ?? '');
    }
};

export const pollStatusLabel = (status: string | number | undefined | null): string => {
    if (typeof status === 'string') return status;
    switch (status) {
        case 0: return POLL_STATUS.OPEN;
        case 1: return POLL_STATUS.CLOSED;
        default: return String(status ?? '');
    }
};

/** yyyy-MM-dd for date inputs / API query params (local time). */
export const toDateInputValue = (d: Date): string => {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
};

/** Start of the current lunch week (Sunday) as yyyy-MM-dd. */
export const currentWeekStart = (): string => {
    const now = new Date();
    const d = new Date(now);
    d.setDate(now.getDate() - now.getDay()); // back to Sunday
    return toDateInputValue(d);
};

export const addDays = (dateStr: string, days: number): string => {
    const d = new Date(dateStr + 'T00:00:00');
    d.setDate(d.getDate() + days);
    return toDateInputValue(d);
};
