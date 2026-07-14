import { defineConfig, type PluginOption } from "vite";
import react from "@vitejs/plugin-react";
import { VitePWA } from "vite-plugin-pwa";
import path from "path";

// https://vitejs.dev/config/
const plugins: PluginOption[] = [
  react(),
  VitePWA({
    registerType: "autoUpdate",
    injectRegister: "auto",
    includeAssets: ["favicon.svg", "apple-touch-icon.png"],
    manifest: {
      name: "FoodPower — Office Lunch",
      short_name: "FoodPower",
      description: "Office lunch polls, dues and payments.",
      lang: "en",
      theme_color: "#f97316",
      background_color: "#ffffff",
      display: "standalone",
      orientation: "portrait",
      scope: "/",
      start_url: "/",
      icons: [
        { src: "pwa-192x192.png", sizes: "192x192", type: "image/png" },
        { src: "pwa-512x512.png", sizes: "512x512", type: "image/png" },
        { src: "pwa-maskable-512x512.png", sizes: "512x512", type: "image/png", purpose: "maskable" },
      ],
    },
    workbox: {
      globPatterns: ["**/*.{js,css,html,svg,png,ico,woff2}"],
      // Never cache API calls — always hit the network so data stays fresh.
      navigateFallbackDenylist: [/^\/api/],
      runtimeCaching: [
        {
          urlPattern: ({ url }) => url.pathname.startsWith("/resources/"),
          handler: "StaleWhileRevalidate",
          options: { cacheName: "uploaded-images", expiration: { maxEntries: 100, maxAgeSeconds: 60 * 60 * 24 * 30 } },
        },
      ],
    },
    devOptions: { enabled: false },
  }),
];

export default defineConfig({
  server: {
    host: "::",
    port: 8080,
  },
  plugins,
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
});
