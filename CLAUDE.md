# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Architecture Overview

This is a fullstack publication browser with three deployable components:

- **Backend API** (`Golden.Publication.Api/`) — ASP.NET Core 10 Web API, targets port `5031` locally and `8080` in Docker
- **Data layer** (`src/Golden.Publication.Data/`) — separate class library; EF Core + Npgsql repository backed by PostgreSQL
- **Frontend** (`client/publications-client/`) — React 19 + TypeScript + Vite SPA, served under the `/app/` path prefix
- **Reverse proxy** (`reverse-proxy/nginx.conf`) — Nginx routes `/app/` to the SPA, `/publications` and `/swagger/` directly to the API
- **Database** — PostgreSQL 17 (`postgres:17-alpine`), managed as a Docker service with a named volume (`postgres_data`)

The solution file is `Golden.Publication.sln` at the root. The API project references `Golden.Publication.Data` directly; the tests project also references it.

### Data flow

Publication data lives in PostgreSQL (`publications` + `publication_versions` tables). On every cold start, `DatabaseSeeder.SeedAsync` runs: it creates the schema with `CREATE TABLE IF NOT EXISTS`, records the migration in `__EFMigrationsHistory`, then seeds from `publications.xml` if the table is empty. The XML file is only needed on first run; subsequent starts skip seeding.

### Key layering

```
PublicationsController  →  PublicationService  →  IPublicationRepository
                                                        ↑
                                               EfPublicationRepository
                                               (PublicationDbContext / Npgsql)
```

`IPublicationRepository` and all model classes are unchanged. All searching, sorting, and paging still happen in `PublicationService.SearchAsync` in memory (the repository loads all records). `EfPublicationRepository.SingleAsync` eagerly includes `Versions`; `WhereAsync` does not (list view doesn't need them).

### Database schema

| Table | Columns |
|---|---|
| `publications` | `id` (uuid PK), `publication_type`, `title`, `description`, `isbn` |
| `publication_versions` | `id` (uuid PK), `publication_guid` (FK → publications.id), `version`, `language`, `cover_title` |

Migration files live in `src/Golden.Publication.Data/Migrations/`. The seeder uses raw SQL rather than `MigrateAsync()` because EF Core 9's reflection-based migration discovery did not pick up the manually authored class at runtime — the raw SQL approach is equivalent and records the same entry in `__EFMigrationsHistory` so future `dotnet ef migrations add` commands work correctly.

### CORS

CORS is **disabled** in production (`appsettings.json`: `"EnableCors": false`) because the reverse proxy collocates the API and client on the same origin. It is enabled only in `appsettings.Development.json` (allows `http://localhost:5173`) so the Vite dev server can call the local API directly.

### Frontend routing

The React app uses `BrowserRouter` with `basename="/app"`. In production (`VITE_API_BASE=""`), API calls use a relative base so they go through the reverse proxy. In development (`VITE_API_BASE=http://localhost:5031`), calls hit the API directly.

## Commands

### Backend

```bash
# Run API locally (from repo root)
dotnet run --project Golden.Publication.Api

# Build solution
dotnet build Golden.Publication.sln

# Run all tests
dotnet test Golden.Publication.sln

# Run a single test
dotnet test tests/golden.publication.Tests/golden.publication.Tests.csproj --filter "FullyQualifiedName~Single_FindsExistingRecord_ById"
```

### Frontend

```bash
cd client/publications-client

npm install
npm run dev        # Vite dev server at http://localhost:5173
npm run build      # Type-check + production build
npm run lint       # ESLint
npm run preview    # Preview production build locally
```

### Docker (full stack)

```bash
# Build and run all four services (postgres, API, client, reverse proxy)
docker compose up --build

# Access points:
# SPA:     http://localhost/app/
# API:     http://localhost/publications
# Swagger: http://localhost/swagger/
```

After `docker compose up`, the API waits for postgres to pass its healthcheck, then automatically creates the schema and seeds data from the bundled XML. No manual DB setup required.

> **Note:** If you restart individual containers after a long gap, the reverse proxy may need a restart too (`docker compose restart reverse-proxy`) if it was started before the other containers joined the network.

## Project Layout

| Path | Purpose |
|------|---------|
| `Golden.Publication.Api/` | ASP.NET Core API project |
| `Golden.Publication.Api/Api/` | DTOs, query model, mappers |
| `Golden.Publication.Api/Domain/` | `PublicationService` — business logic |
| `Golden.Publication.Api/Controllers/` | `PublicationsController` |
| `Golden.Publication.Api/Infrastructure/DatabaseSeeder.cs` | Startup seeder: creates schema + seeds from XML |
| `src/Golden.Publication.Data/` | Repository interfaces, EF Core implementation |
| `src/Golden.Publication.Data/Configuration/` | Fluent entity configs (column names, FK, table names) |
| `src/Golden.Publication.Data/Migrations/` | `InitialCreate` migration + model snapshot |
| `src/Golden.Publication.Data/PublicationDbContext.cs` | EF Core DbContext |
| `src/Golden.Publication.Data/EfPublicationRepository.cs` | Active repository implementation |
| `src/Golden.Publication.Data/XmlPublicationRepository.cs` | Legacy XML implementation (kept, unused at runtime) |
| `src/Golden.Publication.Data/Data/publications.xml` | Seed data source (XML, copied into Docker image) |
| `src/Golden.Publication.Data/DesignTimeDbContextFactory.cs` | Enables `dotnet ef` CLI tooling |
| `tests/golden.publication.Tests/` | xUnit integration tests against the XML file (still valid) |
| `client/publications-client/src/api/` | `client.ts` (fetch wrappers) + `types.ts` |
| `client/publications-client/src/pages/` | `PublicationsListPage`, `PublicationDetailsPage` |
| `client/publications-client/src/components/` | `SearchBar`, `SortControls`, `Pagination` |
| `reverse-proxy/nginx.conf` | Nginx routing config for Docker deployment |

## API Endpoints

```
GET /publications?title=&isbn=&description=&pageNumber=1&pageSize=10&sortBy=title,isbn&sortDir=asc
GET /publications/{guid}
GET /publications/{guid}/versions
GET /swagger/
```

Sorting supports comma-separated fields: `title`, `publication_type`, `isbn`, `description`.

## Coding Standards

- Always use `var` for local variables in C# where type is obvious
- Prefer `async/await` over `.Result` or `.Wait()`
- Error handling: use `ProblemDetails` for API errors, never swallow exceptions
- Tests: AAA pattern (Arrange/Act/Assert), descriptive test names
- React: functional components only, no class components
- CSS: use existing Tailwind/CSS module patterns already in project
- Git commits: conventional commits format (feat:, fix:, docs:, refactor:)

## Planned Improvements (in priority order)

1. Add authentication (JWT) to API
2. Add CI/CD pipeline (GitHub Actions)
3. Frontend: add loading skeletons, improve error states

## Known Issues
- EF Core 9 reflection-based migration discovery does not pick up manually authored migration classes at runtime — schema creation uses raw SQL in `DatabaseSeeder` as a workaround (idempotent, tracked in `__EFMigrationsHistory`)
- No input sanitization on search parameters
- No rate limiting on API

## Do Not Change Without Asking
- The IPublicationRepository interface (other code depends on it)
- Nginx routing config (fragile, tested)
- CORS configuration (deliberate decision)
- The basename="/app" in BrowserRouter

## After Every Implementation Phase

1. Run existing tests — fix any failures before finishing
2. Do a self-review: check for hardcoded values, missing env vars, unhandled exceptions
3. Ensure all environment-sensitive config uses env variables only (no code changes between dev/prod)
4. Build and run Docker container, expose port so I can test on localhost
5. Tell me exactly what URL to open and what I should see
6. If Docker isn't appropriate for this change, explain why and show me the alternative run command

## Environment Strategy
- Dev and prod must work by changing env variables only, never code
- All secrets and config via environment variables (never hardcoded)
- Docker is the preferred way to verify final output unless I say otherwise

## Git Workflow (Always Follow This)

### Branch Strategy
- `main` — production only, never commit here directly
- `claude-dev` — active development branch, all work happens here
- `feature/*` — short-lived feature branches (e.g. feature/postgresql, feature/auth-jwt)


### Development Flow Per Phase
1. All development work goes into `claude-dev` branch. After developement and testing completed when user confirm then for merging purpose go to 2nd point
2. For each new feature, create branch from `claude-dev`:
   `git checkout -b feature/<name>` (e.g. feature/auth-jwt)
3. When feature is complete, commit to feature branch
4. STOP — do not merge. Notify user: "feature/auth-jwt is ready for review"
5. User creates GitHub issue, provides issue number
6. Add issue reference to final commit message: "feat: add JWT auth (#12)"
7. User creates PR and merges feature → main in GitHub
8. User deletes feature branch in GitHub after merge
9. After merge confirmed by user: pull latest claude-dev locally
10. claude-dev and main stay in sync only after user-approved PR to main

### Rules Claude Must Never Break
- Never commit to main directly
- Never create a PR — user always does this in GitHub
- Never merge branches — user always does this in GitHub
- Never delete branches — user always does this in GitHub
- Always wait for user to confirm issue number before referencing it
- Always confirm current branch before any commit: `git branch --show-current`

