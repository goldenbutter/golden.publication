# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Architecture Overview

This is a fullstack publication browser with three deployable components:

- **Backend API** (`Golden.Publication.Api/`) — ASP.NET Core 10 Web API, targets port `5031` locally and `8080` in Docker
- **Data layer** (`src/Golden.Publication.Data/`) — separate class library; XML-backed repository loaded at startup as a singleton
- **Frontend** (`client/publications-client/`) — React 19 + TypeScript + Vite SPA, served under the `/app/` path prefix
- **Reverse proxy** (`reverse-proxy/nginx.conf`) — Nginx routes `/app/` to the SPA, `/publications` and `/swagger/` directly to the API

The solution file is `Golden.Publication.sln` at the root. The API project references `Golden.Publication.Data` directly; the tests project also references it.

### Data flow

All publication data is stored in `src/Golden.Publication.Data/Data/publications.xml`. The `XmlPublicationRepository` loads this file once at startup (singleton). Locally it resolves the path relative to `ContentRootPath`; in Docker it reads `PUBLICATION_XML_FILE=/app/Data/publications.xml`.

### Key layering

```
PublicationsController  →  PublicationService  →  IPublicationRepository
                                                        ↑
                                               XmlPublicationRepository
```

All searching, sorting, and paging happen in `PublicationService.SearchAsync` — the repository always returns all records and filtering is done in memory.

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
# Build and run all three services (API, client, reverse proxy)
docker-compose up --build

# Access points:
# SPA:     http://localhost/app/
# API:     http://localhost/publications
# Swagger: http://localhost/swagger/
```

## Project Layout

| Path | Purpose |
|------|---------|
| `Golden.Publication.Api/` | ASP.NET Core API project |
| `Golden.Publication.Api/Api/` | DTOs, query model, mappers |
| `Golden.Publication.Api/Domain/` | `PublicationService` — business logic |
| `Golden.Publication.Api/Controllers/` | `PublicationsController` |
| `src/Golden.Publication.Data/` | Repository interfaces + XML implementation |
| `src/Golden.Publication.Data/Data/publications.xml` | The data source |
| `tests/golden.publication.Tests/` | xUnit integration tests against the real XML file |
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
