# FoodPower

Office lunch management: the admin publishes the catering menu and a nightly lunch poll, everyone votes before the 10:00 cutoff, dues accumulate per lunch, and payments are settled by uploading a payment screenshot for admin approval. Replaces the WhatsApp-poll + manual-notes process. See `SPEC.md` for the full functional spec.

## Structure

- `backend/` — ASP.NET Core (.NET 10), EF Core + SQL Server, ASP.NET Core Identity, JWT auth, OTP email via Gmail SMTP. Vertical-slice architecture following the AMS project patterns. Dockerized for Render.
- `frontend/` — Vite + React 18 + TypeScript + Tailwind + shadcn/ui + TanStack Query. Mobile-app-style UI (bottom tab bar), English/Bangla localization (browser-detected, EN/বাং toggle).

## Backend — run locally

Requires .NET 10 SDK and SQL Server.

```bash
cd backend/FoodPower
cp env.example .env        # fill in DB_CONNECTION_STRING, EMAIL_* (Gmail SMTP), ADMIN_EMAIL/PASSWORD
dotnet run
```

Swagger is served at the root URL. On first start the app creates the schema (`EnsureCreated`), seeds roles (`Admin`, `User`), the admin user from `ADMIN_EMAIL`/`ADMIN_PASSWORD`, default settings (price 120 BDT, cutoff 10:00, Asia/Dhaka) and a sample caterer + menu.

Recommended once the SDK is available: generate a proper initial migration so future schema changes are migratable.

```bash
dotnet ef migrations add InitialCreate
```

### Gmail SMTP (OTP email)

Use your personal Gmail account: `EMAIL_SMTP_HOST=smtp.gmail.com`, port 587, `EMAIL_USE_SSL=true`, `EMAIL_SENDER_ADDRESS=<your gmail address>`, `EMAIL_SENDER_PASSWORD=<app password>`. Gmail requires an app password (Google Account → Security → 2-Step Verification → App passwords) — your normal account password will not work.

### Docker Compose (local — API + SQL Server)

The quickest way to run the backend locally is `docker compose`, which starts SQL Server and the API together. The API waits for a database healthcheck before booting (its startup initializer swallows DB-connection errors, so it must not start before SQL Server is ready).

```bash
cd backend
cp .env.example .env        # then edit .env — set MSSQL_SA_PASSWORD and JWT_SECRET at minimum
docker compose up --build
```

- API + Swagger: `http://localhost:8080`
- SQL Server: `localhost:1433` (user `sa`)

On first start the app auto-creates the schema (`EnsureCreated`) and seeds roles, the admin user, default settings and a sample caterer. The first run is slow: the .NET SDK image compiles the project and SQL Server takes ~30–40 s to become healthy before the API starts. Point the frontend's `VITE_API_BASE_URL` at `http://localhost:8080`.

Common commands:

```bash
docker compose up -d --build   # run in background
docker compose logs -f api     # follow API logs
docker compose down            # stop (keeps DB + screenshots)
docker compose down -v         # stop and wipe the DB and uploaded screenshots
```

Notes:

- `MSSQL_SA_PASSWORD` must be strong (8+ chars incl. upper, lower, digit and symbol) or the SQL Server container refuses to start and the healthcheck never passes.
- Email/OTP stays disabled until `EMAIL_SENDER_ADDRESS`/`EMAIL_SENDER_PASSWORD` are set (Gmail app password); everything else runs without it.
- Data persists in the `mssql-data` and `screenshots` named volumes across rebuilds.

### Docker (single image) / Render

To build and run just the API image against an external SQL Server:

```bash
cd backend
docker build -t foodpower-api .
docker run -p 8080:8080 --env-file FoodPower/.env foodpower-api
```

On Render: create a Web Service from this Dockerfile; the container binds `http://+:$PORT` automatically. Set all env vars from `env.example` in the Render dashboard. Notes:

- Render does not host SQL Server — point `DB_CONNECTION_STRING` at an external SQL Server (e.g. Azure SQL free tier, or somee.com).
- Payment screenshots are stored on local disk (`resources/screenshots`) and served at `/resources/screenshots/...`. Render's disk is ephemeral unless you attach a persistent disk — attach one (mount path: the app's `resources` folder) or screenshots vanish on redeploy.
- Change `JWT_SECRET` from the default before going live.

## Frontend — run locally

```bash
cd frontend
npm install
cp .env.example .env       # VITE_API_BASE_URL=http://localhost:5000 (backend origin, no /api suffix)
npm run dev
```

`npm run build` outputs `dist/` — deployable to any static host (Vercel/Netlify/Render static site).

## Key behaviour

- One vote per user per poll; changeable until the cutoff; blocked after. Late lunches are added by the admin ("manual vote"), which increases the user's due like a normal vote.
- Due = (lunches taken × poll price) − approved payments. Overpayment is kept as advance and consumed by future lunches.
- One payment can cover multiple people: pick beneficiaries and days per person; the server computes the amount. Admin approves/rejects with the screenshot as evidence.
- Poll publish notifies all active users in-app and produces a public share link (`/poll/<token>`) for WhatsApp.
- Cutoff time (default 10:00), price per lunch and timezone are admin-configurable in Settings; per-poll cutoff can be overridden at publish time.
- Bangla text is safe end-to-end (nvarchar columns, UTF-8 email); UI language auto-detects the browser and can be toggled EN/বাং.

## Status / TODO

- Backend was written against the AMS patterns and manually cross-checked, but could not be compiled in the build environment (no NuGet access there). Run `dotnet build` once locally and fix any residual nits.
- Frontend compiles clean (`tsc` + `vite build`, zero errors).
- Tests are not yet included; add integration tests for the dues calculation and cutoff enforcement before relying