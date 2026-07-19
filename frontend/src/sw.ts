/// <reference lib="webworker" />
import { clientsClaim } from "workbox-core";
import { precacheAndRoute } from "workbox-precaching";
import { registerRoute } from "workbox-routing";
import { StaleWhileRevalidate } from "workbox-strategies";
import { ExpirationPlugin } from "workbox-expiration";

declare const self: ServiceWorkerGlobalScope &
  typeof globalThis & {
    __WB_MANIFEST: (string | { url: string; revision: string | null })[];
  };

// Injected at build time by vite-plugin-pwa (injectManifest strategy).
precacheAndRoute(self.__WB_MANIFEST);

// Take control as soon as possible so a freshly installed SW starts handling
// push events without waiting for all tabs to close (registerType: "autoUpdate").
self.skipWaiting();
clientsClaim();

// Runtime cache for user-uploaded images served from /resources/.
// Never matches /api — those calls always hit the network so data stays fresh.
registerRoute(
  ({ url }) => url.pathname.startsWith("/resources/"),
  new StaleWhileRevalidate({
    cacheName: "uploaded-images",
    plugins: [
      new ExpirationPlugin({ maxEntries: 100, maxAgeSeconds: 60 * 60 * 24 * 30 }),
    ],
  })
);

interface PushPayload {
  title?: string;
  body?: string;
  url?: string;
}

// Show a notification when a push message arrives.
self.addEventListener("push", (event: PushEvent) => {
  let payload: PushPayload = {};
  try {
    payload = event.data?.json() ?? {};
  } catch {
    payload = {};
  }

  const title = payload.title || "FoodPower";
  const body = payload.body || "You have a new notification.";
  const url = payload.url || "/";

  event.waitUntil(
    self.registration.showNotification(title, {
      body,
      icon: "/pwa-192x192.png",
      badge: "/pwa-192x192.png",
      data: { url },
    })
  );
});

// Focus an existing window (or open a new one) when a notification is clicked.
self.addEventListener("notificationclick", (event: NotificationEvent) => {
  event.notification.close();
  const targetUrl = (event.notification.data && event.notification.data.url) || "/";

  event.waitUntil(
    self.clients
      .matchAll({ type: "window", includeUncontrolled: true })
      .then(async (clientList) => {
        const windows = clientList as readonly WindowClient[];
        // Reuse an already-open FoodPower tab if there is one.
        for (const client of windows) {
          await client.focus();
          await client.navigate(targetUrl).catch(() => undefined);
          return;
        }
        await self.clients.openWindow(targetUrl);
      })
  );
});
