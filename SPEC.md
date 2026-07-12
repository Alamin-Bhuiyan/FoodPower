# FoodPower — Office Lunch Management

Digitizes the office lunch process: catering menu publishing, nightly lunch polls (WhatsApp-poll replacement), 10:00 voting cutoff, due tracking, and screenshot-based payment approval.

- Currency: BDT. Price per lunch is flat, set by admin (default 120.00), snapshotted on each poll.
- Backend: ASP.NET Core (.NET 9) + EF Core + SQL Server + ASP.NET Core Identity + JWT. Dockerized for Render.
- Frontend: Vite + React + TypeScript + Tailwind + shadcn/ui + TanStack Query + axios. Mobile-app-like UX (bottom tab bar, max-width shell).
- Follows coding patterns of `Self Project/AMS` (backend) and `Self Project Frontend/apartment-management-system-frontend` (frontend).

## Roles
- `Admin`: everything a User can do, plus menu/poll/payment/settings management and voting on behalf of others.
- `User`: register, verify OTP (email via Gmail SMTP), vote, submit payments (for self and others), view dues.

## Domain model (EF Core, Identity int keys)
- `AppUser : IdentityUser<int>` — FullName, IsActive, CreatedAt.
- `OtpToken` — Id, UserId, Code(6), Purpose(Register|ResetPassword), ExpiresAt(10 min), ConsumedAt?.
- `Caterer` — Id, Name, Phone?, PricePerLunch(decimal 18,2), IsActive.
- `MenuItem` — Id, CatererId, DayOfWeek(enum Sunday..Saturday), Name, Description?, IsActive. (Admin publishes catering menu per day with multiple set menus.)
- `Poll` — Id, LunchDate(date), CatererId, PricePerLunch(snapshot), CutoffAt(datetime, default LunchDate 10:00 local, admin-changeable), Status(Open|Closed), ShareToken(guid, unique), Question, CreatedAt/By.
- `PollOption` — Id, PollId, MenuItemId?(null for custom), Name(denormalized label), SortOrder.
- `Vote` — Id, PollId, PollOptionId, UserId, IsManual(bool, added by admin after cutoff), CreatedAt/By. Unique(PollId, UserId).
- `Payment` — Id, SubmittedById, TotalAmount, ScreenshotPath, Note?, Status(Pending|Approved|Rejected), ReviewedById?, ReviewedAt?, CreatedAt.
- `PaymentAllocation` — Id, PaymentId, BeneficiaryUserId, Days(int), Amount(Days × price at submission). One payment can cover multiple people ("pay for me + coworker").
- `Notification` — Id, UserId, Title, Body, Type(PollPublished|PaymentApproved|PaymentRejected|ManualVoteAdded), RefId?, IsRead, CreatedAt.
- `Setting` — Key, Value. Keys: `DefaultCutoffTime` ("10:00"), `PricePerLunch` ("120"), `TimeZone` ("Asia/Dhaka").

**Balance rule:** userBalance = Σ approved PaymentAllocation.Amount − Σ (votes × poll.PricePerLunch). Negative → due; positive → advance. Advance is consumed naturally by future votes.

## API (all responses `{ "status": {"code","message"}, "data": ... }`)
Auth (`/api/auth`): POST `/register` (sends OTP email), `/verify-otp`, `/resend-otp`, `/login` → JWT {token, user}, `/forget-password`, `/reset-password`, `/change-password`.
Users (`/api/users`): GET `/` (authorized; for pay-for-others picker), GET `/me`.
Caterers (`/api/caterers`): GET `/`; POST/PUT/DELETE admin.
Menu (`/api/menu-items`): GET `/?catererId=&day=`; POST (bulk: day + multiple set menus), PUT `/{id}`, DELETE `/{id}` — admin.
Polls (`/api/polls`):
- POST `/` admin: {lunchDate, catererId?, options:[{menuItemId?|customName}], cutoffAt?} → creates Open poll, notifies all active users, returns shareToken. WhatsApp share URL = `{frontend}/poll/{shareToken}`.
- GET `/active` — current open poll (+options, counts, myVote).
- GET `/shared/{shareToken}` — anonymous: poll + options + vote counts + voter names.
- GET `/` admin/paged history; GET `/{id}/results` — per-option voters.
- POST `/{id}/votes` {pollOptionId} — self vote; rejected after CutoffAt (or if closed). Vote change allowed before cutoff (upsert).
- POST `/{id}/manual-votes` admin: {userId, pollOptionId} — after-cutoff vote for any user (incl. admin himself); notifies the user.
- POST `/{id}/close` admin.
Payments (`/api/payments`):
- POST `/` multipart or base64: {screenshot, note?, allocations:[{beneficiaryUserId, days}]} — amount computed server-side = days × current PricePerLunch.
- GET `/my`, GET `/?status=` admin, POST `/{id}/approve` admin, POST `/{id}/reject` admin {reason?} → notification to submitter.
Dues (`/api/dues`): GET `/my` {balance, lunchCount, history}; GET `/` admin: all users {lunches, paid, balance}; GET `/weekly-summary?weekStart=` admin: per-user lunch count and amount for that week.
Notifications (`/api/notifications`): GET `/`, POST `/{id}/read`, POST `/read-all`.
Settings (`/api/settings`): GET `/` ; PUT `/` admin (cutoff time, price, timezone).

Seed: role `Admin`/`User`; first registered user configurable via `ADMIN_EMAIL` env → gets Admin role.

## Frontend routes (mobile-first)
`/login`, `/register`, `/verify-otp`, `/forget-password`, `/reset-password` — auth shell.
Bottom tabs (authenticated): `/` Home (today's poll, vote, countdown to cutoff, share-to-WhatsApp button), `/menu` (weekly menu by day; admin: edit), `/payments` (submit + history; admin: approval queue tab), `/dues` (my balance/history; admin: everyone + weekly summary), `/profile`.
Admin-only screens reached from tabs: publish poll, manual vote, settings.
Public: `/poll/:shareToken` (results + login CTA).

## Business rules recap
1. Voting blocked at/after `CutoffAt` (default 10:00 on lunch date, admin-changeable per poll and by default setting).
2. Late lunch → user asks admin (offline/WhatsApp); admin records manual vote; due increases like a normal vote.
3. Every vote (normal or manual) increases due by poll price. Payment approval decreases due. Overpayment = advance.
4. Payments can allocate to multiple beneficiaries in one submission.
5. Poll publish creates in-app notifications for all active users + a shareable public link for WhatsApp.
