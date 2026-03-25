# Problem statement
Add a production-grade authentication system to a React + Vite + TypeScript frontend and ASP.NET + PostgreSQL backend that currently has no auth, while preserving current routing and deployment behavior.
## Current architecture context
The app is a SPA served under `/app` with API routes under `/publications`, typically behind Nginx on the same origin in Docker deployment. The backend is ASP.NET Web API with EF Core + PostgreSQL and currently exposes publication endpoints without auth. Frontend currently uses direct `fetch` calls from `client/publications-client/src/api/client.ts` and has no auth state management.
## Recommended authentication approach
Use a hybrid token approach: short-lived JWT access token + long-lived refresh token with rotation.
* Access token: JWT used for API authorization and sent in `Authorization: Bearer <token>` header.
* Refresh token: opaque random token stored server-side and sent to browser as `HttpOnly`, `Secure`, `SameSite` cookie.
* Frontend storage: keep access token in memory (not localStorage/sessionStorage) to reduce XSS persistence risk; rely on refresh endpoint to rehydrate session after reload.
Why this fits this architecture:
* Works well with SPA + API model while keeping backend stateless for access-token validation.
* Refresh cookie is well-suited to same-origin deployment through Nginx (`/app` and API on one host).
* Supports explicit `/refresh` and `/logout` lifecycle and token revocation in PostgreSQL.
* Limits blast radius of leaked access tokens via short expiry and rotation on refresh.
## Implementation plan
1. Create a new branch named warp-dev and perform all work inside this branch.
2. Add backend auth domain model and contracts (user identity, password hashing, token issuing, refresh-token persistence, and auth response DTOs) without changing publication-domain interfaces.
3. Add auth API surface (`/auth/register`, `/auth/login`, `/auth/refresh`, `/auth/logout`, `/auth/me`) and wire JWT validation/authorization middleware so protected endpoints can use `[Authorize]` and anonymous endpoints remain explicit.
4. Add database persistence for users and refresh tokens with EF Core entities/configuration/migration updates, including indexes/constraints for uniqueness, token lookup, expiry, and revocation.
5. Add frontend auth module (service layer, context/provider, hooks, route guard, and auth-aware HTTP wrapper) to manage login/logout/session bootstrap/refresh behavior.
6. Integrate auth into routing and UI entry points (public routes for login/register, protected routes for app pages, and session-aware app initialization).
7. Protect selected backend publication endpoints based on chosen policy (all protected vs read-only public), then align frontend route guards and API calls to that policy.
8. Validate end-to-end flows (register, login, silent refresh, protected navigation, logout, expired/invalid token handling) in both two-port dev mode and reverse-proxy mode.
9. Add/update tests for auth services and auth endpoints plus frontend behavior around auth state transitions and refresh retry handling.
## Frontend step-by-step implementation breakdown
1. Introduce a dedicated auth module under `src/auth` to centralize token/session concerns.
2. Add auth API client functions for register/login/refresh/logout/me, separate from publication fetching.
3. Implement an auth state container (context + reducer/state machine) with states such as `unknown`, `authenticated`, `unauthenticated`, `refreshing`.
4. Add bootstrap logic on app load: call `/auth/refresh` (with credentials) to obtain a fresh access token if refresh cookie is valid.
5. Add authenticated HTTP utility that:
* injects bearer token for protected requests,
* retries once after 401 by calling refresh,
* prevents refresh stampede by queueing concurrent retries.
6. Add route guards for protected pages and optional inverse guard for login/register pages when already authenticated.
7. Add login/register pages and forms with validation/error display mapped from backend ProblemDetails/errors.
8. Add logout action that calls backend logout endpoint, clears in-memory auth state, and redirects to login/public route.
9. Update existing pages/API usage to consume auth-aware client and handle unauthorized states consistently.
## New frontend files to create
* `client/publications-client/src/auth/types.ts`
* `client/publications-client/src/auth/authService.ts`
* `client/publications-client/src/auth/AuthContext.tsx`
* `client/publications-client/src/auth/AuthProvider.tsx`
* `client/publications-client/src/auth/useAuth.ts`
* `client/publications-client/src/auth/tokenManager.ts`
* `client/publications-client/src/auth/ProtectedRoute.tsx`
* `client/publications-client/src/auth/PublicOnlyRoute.tsx`
* `client/publications-client/src/api/authClient.ts`
* `client/publications-client/src/api/httpClient.ts`
* `client/publications-client/src/pages/LoginPage.tsx`
* `client/publications-client/src/pages/RegisterPage.tsx`
* `client/publications-client/src/components/auth/AuthStatus.tsx`
## Existing frontend files expected to be modified
* `client/publications-client/src/main.tsx` (wrap app in auth provider)
* `client/publications-client/src/App.tsx` (add auth routes and route guards)
* `client/publications-client/src/api/client.ts` (switch to auth-aware request utility)
* `client/publications-client/src/pages/PublicationsListPage.tsx` (session-aware UI/actions)
* `client/publications-client/src/pages/PublicationDetailsPage.tsx` (session-aware error handling/navigation)
* `client/publications-client/src/App.css` (auth page/indicator styling if kept in shared stylesheet)
## Backend endpoints that must exist
Auth endpoints (under `/auth`):
* `POST /auth/register` — create user with hashed password; optional auto-login response.
* `POST /auth/login` — validate credentials, return access token, set refresh cookie.
* `POST /auth/refresh` — validate/rotate refresh token, return new access token, set new refresh cookie.
* `POST /auth/logout` — revoke current refresh token (or all user sessions if requested), clear cookie.
* `GET /auth/me` — return current authenticated user profile/claims for UI hydration.
Supporting behavior:
* JWT bearer auth middleware configured with issuer/audience/signing key and token lifetime validation.
* Authorization policy defaults for protected controllers/actions.
* Standardized auth error responses for invalid credentials, locked/disabled user, expired token, revoked token.
## PostgreSQL tables required for auth
1. `users`
* `id` (uuid, PK)
* `email` (citext/text, unique)
* `username` (text, unique or nullable depending on product decision)
* `password_hash` (text)
* `password_salt` (text/bytea if hashing strategy requires explicit salt storage)
* `is_active` (bool)
* `created_at`, `updated_at`, `last_login_at` (timestamptz)
2. `refresh_tokens`
* `id` (uuid, PK)
* `user_id` (uuid, FK -> users.id, indexed)
* `token_hash` (text, unique)
* `expires_at` (timestamptz, indexed)
* `revoked_at` (timestamptz, nullable)
* `replaced_by_token_hash` (text, nullable)
* `created_at` (timestamptz)
* `created_by_ip`, `revoked_by_ip` (text, optional audit)
3. Optional: `user_roles` and `roles` if role-based authorization is needed now; otherwise defer and keep claims minimal.
Key table rules:
* Never store refresh token plaintext; store only hash.
* Enforce one-way rotation chain to detect replay of old tokens.
* Add cleanup strategy for expired/revoked tokens.
## Data flow design
Registration:
* Frontend submits form to `POST /auth/register`.
* Backend validates input + uniqueness, hashes password, stores user.
* Backend returns success (and optionally immediate auth payload depending on chosen UX).
Login:
* Frontend sends credentials to `POST /auth/login`.
* Backend verifies password hash, issues short-lived JWT access token, issues refresh token cookie.
* Frontend stores access token in memory and marks auth state authenticated.
Storing JWT securely:
* Access token lives in memory only (context/token manager).
* Refresh token is `HttpOnly` cookie, not readable by JS.
* Frontend sends `credentials: "include"` only for auth endpoints requiring cookie exchange.
Refreshing tokens:
* On app bootstrap or first 401, frontend calls `POST /auth/refresh`.
* Backend validates refresh token hash, checks revocation/expiry, rotates token, sets new cookie, returns new access token.
* Frontend updates in-memory access token and retries failed request once.
Protecting routes:
* Frontend route guards block protected pages when unauthenticated and redirect to login.
* Backend remains source of truth with `[Authorize]` on protected endpoints.
Logout:
* Frontend calls `POST /auth/logout`.
* Backend revokes active refresh token record(s), clears cookie.
* Frontend clears in-memory token/state and redirects to login/public page.
## Confirmed decisions
1. Publication read endpoints are protected now (login required).
2. Self-registration is enabled.
3. Login is username + password only for now (email login deferred).
4. Roles required now: `admin` and `user`.
5. Session policy now: single active session per user (multi-session deferred).
6. Token lifetime target for now: 24 hours.
7. Registration flow should auto-login immediately after successful signup.
8. Social login is out of scope for this phase (deferred).
9. Password reset and email verification are out of scope for this phase (deferred).
10. Development should support CORS-enabled mode; docker/EC2/production should run with CORS off behind reverse proxy.
## Deferred features to track in README (future development ideas)
* Email-based login.
* Multi-session/device support.
* Social login providers (e.g., Google/GitHub).
* Password reset flow.
* Email verification flow.
