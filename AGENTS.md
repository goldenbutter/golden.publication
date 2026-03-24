# AGENTS.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Core development commands

### Backend (.NET)
- Restore/build solution:
  - `dotnet restore Golden.Publication.sln`
  - `dotnet build Golden.Publication.sln`
- Run API locally (Development profile in `launchSettings.json`, default `http://localhost:5031`):
  - `dotnet run --project Golden.Publication.Api/Golden.Publication.Api.csproj`
- Run all backend tests:
  - `dotnet test tests/golden.publication.Tests/golden.publication.Tests.csproj`
- Run one test:
  - `dotnet test tests/golden.publication.Tests/golden.publication.Tests.csproj --filter "FullyQualifiedName~Single_FindsExistingRecord_ById"`

### Frontend (Vite + React)
- `cd client/publications-client`
- Install deps: `npm install`
- Dev server: `npm run dev` (Vite, default `http://localhost:5173`)
- Lint: `npm run lint`
- Production build: `npm run build`
- Preview production build: `npm run preview`

### Full stack via Docker Compose (recommended integration run)
- `docker compose down --volumes --remove-orphans`
- `docker compose build --no-cache`
- `docker compose up -d`
- Endpoints after startup:
  - SPA: `http://localhost/app/`
  - API: `http://localhost/publications`
  - Swagger UI: `http://localhost/swagger/`

## Big-picture architecture

This repository is a 3-layer app deployed as 4 containers:
- PostgreSQL (`postgres`)
- ASP.NET API (`Golden.Publication.Api`)
- React SPA (`client/publications-client`)
- Nginx reverse proxy (`reverse-proxy/nginx.conf`)

In containerized mode, all traffic enters through Nginx on port 80. Nginx routes:
- `/app/` → client
- `/publications` and `/swagger/` → API

### Backend request flow
`PublicationsController` (`Golden.Publication.Api/Controllers`) calls `PublicationService` (`Golden.Publication.Api/Domain`), which depends on `IPublicationRepository` from `src/Golden.Publication.Data`.

The active runtime repository is `EfPublicationRepository` over `PublicationDbContext` (PostgreSQL via EF Core/Npgsql). Mapping from domain models to response JSON happens in `Golden.Publication.Api/Api/PublicationMappers.cs`.

### Query behavior that affects performance and behavior
`PublicationService.SearchAsync` currently loads records first (`WhereAsync(p => true)`) and then applies filtering/sorting/paging in memory. Keep this in mind when changing query logic or troubleshooting list endpoint performance.

### Schema + seed lifecycle
On API startup, `DatabaseSeeder.SeedAsync` runs unconditionally:
- Ensures schema using raw SQL (not `MigrateAsync`)
- Inserts migration marker `20260322000000_InitialCreate` into `__EFMigrationsHistory`
- Seeds from `src/Golden.Publication.Data/Data/publications.xml` only when `publications` is empty

The XML path can be overridden with `PUBLICATION_XML_FILE`.

### Frontend routing/API coupling
- SPA router uses `BrowserRouter basename="/app"` (`client/publications-client/src/App.tsx`)
- Vite base path is `/app/` (`client/publications-client/vite.config.ts`)
- API base URL comes from `VITE_API_BASE` (`client/publications-client/src/api/client.ts`)
  - Dev: `.env.development` points to `http://localhost:5031`
  - Production build: `.env.production` uses empty base so requests go through Nginx origin

## Existing repo-specific constraints to preserve

From existing project guidance (`CLAUDE.md`) and current implementation:
- Keep `IPublicationRepository` stable unless intentionally coordinating wider changes.
- Preserve reverse-proxy route behavior for `/app/`, `/publications`, and `/swagger/`; frontend routing depends on this.
- Preserve `BrowserRouter basename="/app"` unless Nginx and Vite base config are changed together.
- CORS is intentionally enabled in Development and disabled in production settings.
