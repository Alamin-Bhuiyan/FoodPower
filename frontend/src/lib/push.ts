import * as pushService from '@/services/push.service';

/**
 * Web Push helpers. Everything here no-ops safely when the environment does not
 * support push (e.g. iOS Safari before the app is installed to the Home Screen,
 * or non-HTTPS origins), so callers never have to guard for `undefined` APIs.
 */

export const isPushSupported = (): boolean =>
    typeof navigator !== 'undefined' &&
    'serviceWorker' in navigator &&
    typeof window !== 'undefined' &&
    'PushManager' in window &&
    'Notification' in window;

export const getNotificationPermission = (): NotificationPermission | 'unsupported' => {
    if (!isPushSupported()) return 'unsupported';
    return Notification.permission;
};

/** Standard VAPID key conversion: URL-safe base64 → Uint8Array. */
export const urlBase64ToUint8Array = (base64String: string): Uint8Array<ArrayBuffer> => {
    const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
    const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(new ArrayBuffer(rawData.length));
    for (let i = 0; i < rawData.length; i += 1) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
};

/**
 * Resolve the active service worker registration, or null when none exists.
 * `navigator.serviceWorker.ready` never settles when no SW is registered
 * (it would hang forever, e.g. in dev with the PWA plugin disabled), so we
 * race it against a short timeout and fall back to getRegistration().
 */
const getRegistrationSafe = async (timeoutMs = 4000): Promise<ServiceWorkerRegistration | null> => {
    try {
        const ready = navigator.serviceWorker.ready;
        const timeout = new Promise<null>(resolve => window.setTimeout(() => resolve(null), timeoutMs));
        const registration = await Promise.race([ready, timeout]);
        if (registration) return registration;
        return (await navigator.serviceWorker.getRegistration()) ?? null;
    } catch {
        return null;
    }
};

/** Whether an active push subscription already exists for this browser. */
export const hasActiveSubscription = async (): Promise<boolean> => {
    if (!isPushSupported()) return false;
    try {
        const registration = await getRegistrationSafe();
        if (!registration) return false;
        const subscription = await registration.pushManager.getSubscription();
        return subscription !== null;
    } catch {
        return false;
    }
};

/**
 * Request permission, subscribe via PushManager and register the subscription
 * with the backend.
 * @returns 'granted' on success, 'denied' if the user blocked notifications,
 *          or 'unsupported' when push is unavailable.
 */
export const enablePush = async (): Promise<'granted' | 'denied' | 'unsupported'> => {
    if (!isPushSupported()) return 'unsupported';

    const permission = await Notification.requestPermission();
    if (permission !== 'granted') return 'denied';

    // No active service worker (e.g. registration failed) — treat as unsupported
    // instead of hanging forever on `serviceWorker.ready`.
    const registration = await getRegistrationSafe();
    if (!registration) return 'unsupported';

    const keyRes = await pushService.getVapidPublicKey();
    const publicKey = keyRes.data?.public_key;
    if (!publicKey) return 'unsupported';

    const applicationServerKey = urlBase64ToUint8Array(publicKey);
    const subscription = await registration.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey,
    });

    const json = subscription.toJSON();
    const keys = json.keys ?? {};
    await pushService.subscribe({
        endpoint: subscription.endpoint,
        p256dh: keys.p256dh ?? '',
        auth: keys.auth ?? '',
    });

    return 'granted';
};

/** Unsubscribe from the backend and the browser PushManager. Safe to call when not subscribed. */
export const disablePush = async (): Promise<void> => {
    if (!isPushSupported()) return;

    const registration = await getRegistrationSafe();
    if (!registration) return;
    const subscription = await registration.pushManager.getSubscription();
    if (!subscription) return;

    try {
        await pushService.unsubscribe({ endpoint: subscription.endpoint });
    } finally {
        await subscription.unsubscribe();
    }
};
