### Publications – API & React SPA
A small full‑stack demo with:

- **ASP.NET Web API** (reads publications.xml, search/sort/pagination, details)
- **React (Vite + TypeScript)** SPA frontend
- **Docker / Docker Compose**
- **Nginx reverse proxy** for **single‑port deployment** (SPA + API + Swagger via one origin)
- **Jenkins CI/CD Integration** with **GitHub Webhook Integration**

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
- #local--singleport-recommended (reverse proxy)
- #ec2--singleport-deployment (with docker)
- #optional--local-twopot-dev-cors-on
- #routes-summary
- #screenshots
- #jenkins-ci-cd-github-webhook

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

### Local – Single‑port via reverse proxy (Recommended) 

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

### EC2 – Single‑port Deployment with Docker / Docker compose

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

**Publication exposed as JSON file**

- Publication JSON data (Localhost)

<img width="650" height="910" alt="publication-json-localhost" src="https://github.com/user-attachments/assets/b4730708-e2d9-4aeb-a39d-5a3b6aee64a2" />


- Publication JSON data (EC2)

<img width="650" height="910" alt="publication-json-ec2" src="https://github.com/user-attachments/assets/5552199b-ac1c-4ef4-b8ae-27fba4901d70" />


 
**SINGLE-port testing**

- SPA homepage (Frontend) --> Localhost (no port)

<img width="550" height="600" alt="spa-home-localhost" src="https://github.com/user-attachments/assets/80afcd5a-ca66-42df-8dd1-98b190a09f8b" />

- SPA homepage (Frontend) --> EC2 instance (no port)

<img width="550" height="600" alt="spa-home-ec2" src="https://github.com/user-attachments/assets/001d016d-e800-44f3-a0b8-e6039f978e94" />

- Swagger UI (Backend) -->  Localhost (no port)

<img width="1800" height="650" alt="swagger-ui-localhost" src="https://github.com/user-attachments/assets/63eb2555-9a22-4502-a767-1effe780eff6" />

- Swagger UI (Backend) -->  EC2 instance (no port)

<img width="1800" height="680" alt="swagger-ui-ec2" src="https://github.com/user-attachments/assets/47760997-21f5-451c-b318-c134fc6c8071" />


**DOUBLE-port testing**

- SPA homepage (Frontend) --> Localhost (port : 5173)

<img width="550" height="600" alt="spa-home-localhost-5173" src="https://github.com/user-attachments/assets/55ea9bd4-1918-429f-95d2-19e8d1254155" />

- Swagger UI (Backend) --> Localhost (port : 5031)

<img width="1800" height="650" alt="swagger-ui-localhost-5031" src="https://github.com/user-attachments/assets/cca07372-6320-4918-9a91-16d42e38f5f8" />


### Jenkins

##  CI/CD Pipeline with Jenkins

This project includes a fully automated CI/CD pipeline using Jenkins, integrated with GitHub via webhooks.

###  Jenkins Setup
- Jenkins master hosted on EC2
- Node.js 20, .NET SDK 10, and Docker installed
- SSH-based deployment to a separate EC2 instance

###  Pipeline Stages
1. **Checkout SCM** – Pulls latest code from GitHub
2. **Build React Frontend** – Runs `npm ci` and `npm run build`
3. **Build .NET Backend** – Restores and publishes the .NET API
4. **Build Docker Images** – Builds containers for frontend, backend, and reverse proxy
5. **Deploy to EC2** – SSH into deployment server and runs Docker Compose
6. **Restart Services** – Restarts running containers via `docker-compose restart`

###  GitHub Webhook Integration
- Webhook triggers Jenkins build on every push
- No manual intervention required
- Instant deployment to live EC2 environment

###  Live Deployment
- React frontend served via Nginx reverse proxy
- .NET backend exposed via API container
- All services orchestrated using Docker Compose

---
