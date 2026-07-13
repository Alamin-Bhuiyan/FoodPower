import { STORAGE_KEYS, ROLES } from './constants';
import type { AuthUser } from '@/types';

export const getToken = (): string | null =>
    localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN);

export const getStoredUser = (): AuthUser | null => {
    try {
        const raw = localStorage.getItem(STORAGE_KEYS.USER);
        if (!raw) return null;
        return JSON.parse(raw) as AuthUser;
    } catch {
        return null;
    }
};

export const isAuthenticated = (): boolean => !!getToken();

export const isAdmin = (): boolean => {
    const user = getStoredUser();
    if (!user) return false;
    const roles = user.roles ?? [];
    return roles.includes(ROLES.ADMIN);
};

export const setSession = (token: string, user: AuthUser) => {
    localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, token);
    localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(user));
    localStorage.setItem(STORAGE_KEYS.USER_ID, String(user.id));
    if (user.full_name) localStorage.setItem(STORAGE_KEYS.USER_NAME, user.full_name);
    const roles = user.roles ?? [];
    localStorage.setItem(STORAGE_KEYS.USER_ROLE, roles.includes(ROLES.ADMIN) ? ROLES.ADMIN : ROLES.USER);
};

/** Persist an updated user back to localStorage (e.g. after a profile photo change). */
export const setStoredUser = (user: AuthUser) => {
    localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(user));
    localStorage.setItem(STORAGE_KEYS.USER_ID, String(user.id));
    if (user.full_name) localStorage.setItem(STORAGE_KEYS.USER_NAME, user.full_name);
};

export const clearSession = () => {
    localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN);
    localStorage.removeItem(STORAGE_KEYS.USER);
    localStorage.removeItem(STORAGE_KEYS.USER_ID);
    localStorage.removeItem(STORAGE_KEYS.USER_NAME);
    localStorage.removeItem(STORAGE_KEYS.USER_ROLE);
};

export const getInitials = (name?: string | null): string => {
    if (!name) return '?';
    return name
        .split(' ')
        .filter(Boolean)
        .map(n => n[0])
        .join('')
        .toUpperCase()
        .slice(0, 2);
};
