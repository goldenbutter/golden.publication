### Publications – API & React SPA
A small full‑stack demo with:

- **ASP.NET Web API** (reads publications.xml, search/sort/pagination, details)
- **React (Vite + TypeScript)** SPA frontend
- **Docker / Docker Compose**
- **Nginx reverse proxy** for **single‑port deployment** (SPA + API + Swagger via one origin)

This repo supports:

1.  **Primary: Single‑port (reverse proxy)**
    - Everything served on **port 80** (local & EC2)
    - No CORS needed (same origin)
2.  **Optional: Two‑port dev (local only)**
     - React dev server on **5173**
     - API on **5031**
     - CORS enabled **only** in Development via **appsettings.Development.json**




### Table of Contents

- #architecture
- #repo-structure
- #prerequisites
- #environment-variables
- #local--singleport-recommended
- #ec2--singleport-deployment
- #optional--local-twopot-dev-cors-on
- #routes-summary


### Architecture

- **API (ASP.NET):**
  - Endpoints:
    - GET /publications — list, search, sort, paginate
    - GET /publications/{id} — details + versions
  - Swagger UI at /swagger/ (JSON at /swagger/v1/swagger.json)
  - Reads publications.xml (copied into image at /app/Data/publications.xml via Dockerfile)
- **Client (React + Vite + TypeScript):**
  - SPA for listing/searching publications and viewing details
  - API base configured via Vite env (VITE_API_BASE)
- **Reverse proxy (Nginx):**
  - One public port (**80**)
  - Path routing:
     - SPA → / (or /app/ depending on your choice)
    - API → /publications
    - Swagger → /swagger/


### Repo Structure

```
microchip.interview/
├─ Microchip.Interview.Api/                  # ASP.NET Web API
│  ├─ Program.cs
│  ├─ appsettings.json
│  ├─ appsettings.Development.json
│  ├─ Dockerfile
│  └─ Properties/launchSettings.json
├─ src/
│  └─ Microchip.Interview.Data/
│     └─ Data/publications.xml
├─ client/
│  └─ publications-client/                   # React (Vite + TS)
│     ├─ src/
│     │  ├─ api/client.ts
│     │  ├─ api/types.ts
│     │  ├─ pages/...
│     │  └─ components/...
│     ├─ vite.config.ts                      # only if serving under /app
│     ├─ nginx-client.conf
│     ├─ Dockerfile
│     ├─ .env.development
│     └─ .env.production
├─ reverse-proxy/
│  └─ nginx.conf                             # Nginx routing (single port)
├─ docker-compose.yml
└─ README.md
```

### Prerequisites

- **Node.js** (LTS) & **npm** (for local client dev)
- **.NET SDK** compatible with dot net**10.0**
- **Docker** & **Docker Compose**
- **PowerShell** / **Bash** for commands


### Environment Variables

### Client (Vite)

- client/publications-client/.env.development (two‑port dev)

`VITE_API_BASE=http://localhost:5031`

- client/publications-client/.env.production (single‑port reverse proxy)

`VITE_API_BASE=http://<host>     # e.g., http://localhost or http://<ec2-ip>`

If you serve SPA under /app, ensure:

- vite.config.ts → base: '/app/'
- React Router → basename="/app"
- Client Nginx fallback → try_files $uri /index.html;

### API (ASP.NET) – Conditional CORS
Microchip.Interview.Api/appsettings.json (**Production** / default – CORS **OFF**):


```
{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "EnableCors": false,
  "AllowedOrigins": []
}
```

Microchip.Interview.Api/appsettings.Development.json (**Development** – CORS **ON** for two‑port dev):

```
{
  "Logging": { "LogLevel": { "Default": "Debug", "Microsoft.AspNetCore": "Information" } },
  "AllowedHosts": "*",
  "EnableCors": true,
  "AllowedOrigins": [ "http://localhost:5173" ]
}
```

### Local – Single‑port (Recommended)

Run the full stack via Docker Compose (production‑like, one port).


```
docker compose down --volumes --remove-orphans
docker compose build --no-cache
docker compose up -d
```

**Open:**

- SPA (choose your path):
  - http://localhost/ or
  - http://localhost/app/ (if you deployed under /app)
- API: http://localhost/publications
- Swagger UI: http://localhost/swagger/
- Swagger JSON: http://localhost/swagger/v1/swagger.json

If SPA is under /app, always use the **trailing slash**: /app/.

### EC2 – Single‑port Deployment

1. **Security Group:** allow inbound **80/tcp** (HTTP) and **22/tcp** (SSH from your IP).
2. **SSH & pull:**

```
ssh -i your-key.pem ec2-user@<EC2_PUBLIC_IP>
cd ~/microchip-interview-private
git pull origin main
```

3. **Client production env:**

```
echo "VITE_API_BASE=http://<EC2_PUBLIC_IP>" > client/publications-client/.env.production
```
4. **Build & run:**

```
docker compose down --volumes --remove-orphans
docker compose build --no-cache
docker compose up -d
```
5. **Test:**
   - SPA: http://<EC2_PUBLIC_IP>/ (or /app/)
   - API: http://<EC2_PUBLIC_IP>/publications
   - Swagger: http://<EC2_PUBLIC_IP>/swagger

**Ops shortcuts:**

```
# Update code
git pull origin main

# Rebuild only API
docker compose build --no-cache api && docker compose up -d api

# Rebuild only client (after .env.production / vite changes)
docker compose build --no-cache client && docker compose up -d client

# Logs
docker compose logs -f reverse-proxy
docker compose logs -f api
docker compose logs -f client

```

### Optional – Local Two‑port Dev (CORS ON)

For fast iteration with HMR on Vite.

1. **Run API in Development (CORS ON via appsettings.Development.json):**

```
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project Microchip.Interview.Api/Microchip.Interview.Api.csproj
# http://localhost:5031 (Swagger at /swagger)
```

2. **Run client dev:**

```
cd client/publications-client
npm install
npm run dev
# http://localhost:5173
```

3. **Client dev env:**

```
VITE_API_BASE=http://localhost:5031
```

**Expected:** SPA calls API with no CORS errors (Development env enables CORS).
**Note:** Stop the API with Ctrl+C—it runs in the foreground. Use a second terminal for npm run dev.

### Routes Summary

- **SPA:**
   - http://<host>/ or http://<host>/app/ (if sub‑path deployment)
- **API:**
  - GET http://<host>/publications
  - GET http://<host>/publications/{id}
- **Swagger:**
  - UI → http://<host>/swagger/
  - JSON → http://<host>/swagger/v1/swagger.json

Controller route is **lower‑case** [Route("publications")], matching Nginx’s case‑sensitive location /publications.


### Screenshots

**SINGLE-port testing**

- SPA homepage (Frontend) --> Localhost (testing)

assets/screenshots/spa-home-localhost.png

- SPA homepage (Frontend) --> EC2 instance (PROD)

- Swagger UI (Backend) -->  Localhost (testing)

assets/screenshots/swagger-ui-localhost.png

- Swagger UI (Backend) -->  EC2 instance (PROD)

- Publication JSON data

assets/screenshots/publication-json-localhost.png



**DOUBLE-port testing**