# FoodPower — Session Handoff

Office lunch management PWA. Replaces a WhatsApp-poll + manual-notes process: admin publishes a daily lunch poll, coworkers vote before a 10:00 cutoff, dues accrue per lunch, payments settled via screenshot + admin approval.

## Stack & layout
- Repo root: `E:\1. MD. AL-AMIN BHUIYAN\FoodPower` (git repo, branch `main`).
- **Backend** `backend/FoodPower/` — ASP.NET Core **.NET 10**, EF Core + **SQL Server**, ASP.NET Core Identity (`AppUser : IdentityUser<int>`), JWT (1-month access token, no refresh token). Vertical slices: `Features/<Area>/<Area>Controllers` + `<Area>Handlers`; MediatR + ErrorOr + Mapster + FluentValidation; `ResponseModel<T>` envelope `{status:{code,message},data}`; routes in `Presentation/Routes/*Routes.cs`; **snake_case** DTOs; `<ImplicitUsings>disable</ImplicitUsings>` (explicit usings required). Global `UtcDateTimeConverter` serializes all datetimes as UTC `...Z`. Solution: `backend/FoodPower.sln`.
- **Frontend** `frontend/` — Vite + React 18 + TS + Tailwind + shadcn/ui + TanStack Query + axios. Mobile-app UX (bottom tabs, max-w-md shell). i18n en/bn (`src/i18n/locales/*.json`, keys must stay aligned). **PWA** via vite-plugin-pwa (injectManifest, custom `src/sw.ts`). Hosted on **Vercel** (`https://food-power.vercel.app`), `vercel.json` SPA rewrite. Deep-links via `VITE_API_BASE_URL`.

## Deployment
- **Backend → MonsterASP** (`foodpower.runasp.net`, site79197) via WebDeploy profile:
  ```powershell
  cd backend/FoodPower
  dotnet publish -c Release /p:PublishProfile=foodpower-runasp /p:Password="Pn9-7!sEw3M="
  ```
  Schema is created/updated on app **startup** by `DbInitializer` (`Migrate()` if migrations exist, else `EnsureCreated()`), then idempotent seeding (roles, admin from `ADMIN_EMAIL`/`ADMIN_PASSWORD`, settings, caterer + Bangla menu). Must hit the site once after deploy to trigger startup.
- **Frontend → Vercel:** `cd frontend && npm install && npm run build` then push (Vercel auto-deploys on git push). HTTPS is automatic (needed for PWA + push).
- **DB:** SQL Server on MonsterASP. No self-service HTTPS on MonsterASP free tier (SSL needs a support ticket — pending).
- Host env vars (MonsterASP panel): `DB_CONNECTION_STRING`, `JWT_SECRET`, `ADMIN_EMAIL`/`ADMIN_PASSWORD`, `FRONTEND_BASE_URL=https://food-power.vercel.app`, `EMAIL_*` (Gmail SMTP), and **`VAPID_PUBLIC_KEY` / `VAPID_PRIVATE_KEY` / `VAPID_SUBJECT`** for push.

## Migrations (now on EF migrations, not EnsureCreated)
- Workflow: change model → `dotnet ef migrations add <Name>` (in `backend/FoodPower`, set `DB_CONNECTION_STRING` env locally first) → commit → `dotnet publish`; startup `Migrate()` applies it.
- The DB was dropped and rebuilt from `InitialCreate`. Drop-all-tables SQL and manual admin-edit SQL live in chat history if needed.

## Features implemented
Auth (register + email OTP verify, login, forgot/reset/change password). Lunch polls (Home = accordion of ~10 recent lunch polls, newest expanded, admin FAB to publish; publishing auto-closes the previous open lunch poll). Voting + **unvote** before cutoff (10:00 Asia/Dhaka default, admin-changeable). Admin manual votes (allowed even after close). **General polls** ("Other") that never affect dues. Dues/advance (lifetime; balance = approved payments − lunch votes×price; Everyone list + per-user detail sheet; Weekly summary **Mon–Fri**; unverified users hidden from admin lists). Payments (base64 screenshot → admin approve/reject; pay-for-others allocations, default **4 days**; WhatsApp share; admin gets notified on submit). Settings (price, cutoff, timezone, bKash/bank — shown on Payments + WhatsApp share). Profile (photo upload → `resources/avatars`, change password, notifications toggle). In-app notifications (bell, latest 10). All times Asia/Dhaka; BDT currency; en/bn toggle. **Web Push**: subscribe from Profile; server pushes on poll-publish (non-admins), payment-submit (admins), and admin buttons for poll reminder / payment reminder / "Lunch arrived" (notifies that poll's voters).

## OUTSTANDING before it fully works (do in order)
1. **`dotnet ef migrations add AddPushSubscriptions`** — CRITICAL. Snapshot still has 0 `push_subscriptions` refs; without it the table isn't created and EF errors "pending model changes."
2. Commit + push all uncommitted work (everything after commit `76b1ae2 pwa`: Web Push backend+frontend, reminder/announce endpoints+buttons, PaymentSubmitted/LunchArrived notification types, unverified-user dues filter, Dues lunch-date-only fix).
3. Deploy backend (`dotnet publish`) → migration auto-applies.
4. Set `VAPID_*` env vars on MonsterASP host (push silently disabled without them). Current dev VAPID keypair is in chat history; regenerate for production if desired.
5. Deploy frontend (`npm install && npm run build` → Vercel).

## Known caveats
- **Build not possible in the assistant sandbox** (no .NET SDK / NuGet, stale Linux mount). All backend/frontend changes were hand-written + manually reviewed; **build both locally** (`dotnet build`, `npm run build`) and fix any residual nits.
- **Email/OTP to `@netpower.no`** work addresses get quarantined by Microsoft 365 (phishing/spam heuristics on automated code emails from Gmail). App passwords are disabled on the tenant, so Outlook SMTP isn't an option. Personal emails (Gmail) receive fine. Reliable fix would be a Tenant Allow-list entry (admin) or a transactional ESP + owned domain. Verify-OTP page shows a quarantine-release guide. For work accounts, admin can verify a user directly in DB (`EmailConfirmed=1`) or via SQL.
- **`resources/` (screenshots + avatars)** is disk-based — needs a persistent disk on the host or files vanish on redeploy.
- **Polling:** Home refetches active/general polls every 30s, notifications every 60s — only while the tab is focused. Push is one-time subscribe, not polling.
- Rider may overwrite assistant edits — File → Reload All from Disk before building.
