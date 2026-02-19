# Publications – Full‑Stack Demo (ASP.NET API + React SPA + Docker + Jenkins CI/CD)
A complete full‑stack demo application featuring:
- **ASP.NET Web API** (search, sort, pagination, details from publications.xml)
- **React (Vite + TypeScript)** SPA frontend
- **Docker & Docker Compose** for containerized deployment
- **Nginx reverse proxy** for single‑port hosting (SPA + API + Swagger)
- **Jenkins CI/CD Pipeline** with **GitHub Webhook** for automatic deployments
Supports:
1. **Single‑port deployment (recommended)**  
    - Everything served on port `80` (local & EC2)  
    - No CORS needed (same origin)
2. **Two‑port local development**  
    - React dev server → `5173`
    - API → `5031` 
    - CORS enabled only in Development
---
## Table of Contents

- [Architecture](#architecture)
- [Architecture Diagram](#architecture-diagram)
- [Repo Structure](#repo-structure)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Local Development](#local-development)
- [CI/CD (Jenkins Deployment)](#cicd-jenkins-deployment)
- [Environment Variables](#environment-variables)
- [Local – Single‑Port (Recommended)](#local-single-port-recommended)
- [EC2 ‑ Single‑Port Deployment](#ec2-single-port-deployment)
- [Optional ‑ Two‑Port Dev (CORS ON)](#optional-two-port-dev)
- [Routes Summary](#routes-summary)
- [Screenshots](#screenshots)
- [Jenkins CI/CD Pipeline](#jenkins-cicd-pipeline)

## Architecture

### **API (ASP.NET)**
- Endpoints:
 - `GET /publications` — list, search, sort, paginate  
 - `GET /publications/{id}` — details + versions  
- Swagger UI → `/swagger/`  
- Reads `publications.xml` (copied into image)

### **Client (React + Vite + TypeScript)**
- SPA for listing/searching publications  
- API base configured via Vite env (`VITE_API_BASE`)

### **Reverse Proxy (Nginx)**
- One public port (**80**)  
- Routes:
 - `/` → SPA  
 - `/publications` → API  
 - `/swagger/` → Swagger UI

## Architecture Diagram
```text
                        ┌──────────────────────────┐
                        │        GitHub Repo       │
                        │  (Frontend + Backend)    │
                        └─────────────┬────────────┘
                                      │ Webhook (Push)
                                      ▼
                        ┌───────────────────────────┐
                        │      Jenkins (EC2 #2)     │
                        │  - Node.js 20             │
                        │  - .NET SDK 10            │
                        │  - Docker                 │
                        └─────────────┬─────────────┘
                                      │ SSH Deploy
                                      ▼
                        ┌───────────────────────────┐
                        │     EC2 #1 (Deploy)       │
                        │  Docker Compose runs:     │
                        │    - React Client         │
                        │    - ASP.NET API          │
                        │    - Nginx Reverse Proxy  │
                        └─────────────┬─────────────┘
                                      │
                                      ▼
                        ┌───────────────────────────┐
                        │      Public Internet      │
                        │  http://EC2_PUBLIC_IP     │
                        │  (SPA + API + Swagger)    │
                        └───────────────────────────┘
```

<img width="1650" height="750" alt="architecture-diagram" src="assets\architecture-diagram.png" />


## Repo Structure

```text
Golden.Publication/
├─ Golden.Publication.Api/                  # ASP.NET Web API
│  ├─ Program.cs
│  ├─ appsettings.json
│  ├─ appsettings.Development.json
│  ├─ Dockerfile                            # multi-stage build, publishes to /app/publish
│  └─ Properties/launchSettings.json
├─ src/
│  └─ Golden.Publication.Data/
│     └─ Data/publications.xml
├─ client/
│  └─ publications-client/                   # React (Vite + TS)
│     ├─ src/
│     ├─ vite.config.ts
│     ├─ nginx-client.conf
│     ├─ Dockerfile                          # build → Nginx serving
│     ├─ .env.development
│     └─ .env.production
├─ reverse-proxy/
│  └─ nginx.conf
├─ docker-compose.yml                         # api, client, reverse-proxy (port 80 exposed)
├─ Jenkinsfile                                # CI/CD pipeline (build/test, deploy via SSH to EC2)
└─ README.md
```

## Tech Stack

### **Backend**
- ASP.NET 10 Web API  
- LINQ, XML parsing  
- Swagger / OpenAPI  

### **Frontend**
- React  
- Vite  
- TypeScript  

### **DevOps**
- Docker  
- Docker Compose  
- Nginx  
- Jenkins  
- GitHub Webhooks  
- AWS EC2


  
## Prerequisites

### Local Development
- **Node.js (LTS)** & **npm**
- **.NET SDK 10.0**
- **Docker & Docker Compose**
- **PowerShell / Bash**

### CI/CD (Jenkins Deployment)
- **Jenkins** (latest LTS) on EC2  
- **Node.js 20+** on Jenkins  
- **.NET SDK 10.0** on Jenkins  
- **Docker** & **Docker Compose** on **Jenkins**  
- SSH key-based access from **Jenkins** → **EC2**  
- **GitHub Webhook** configured with **Jenkins**


## <span id="environment-variables">Environment Variables</span>

## Client (Vite)

### `.env.development` (two‑port dev)

`VITE_API_BASE=http://localhost:5031`

### `.env.production` (single‑port reverse proxy)

`VITE_API_BASE=""`

If SPA is served under `/app`, update:
- `vite.config.ts` → base: `/app/`
- React Router → basename=`/app`
- Nginx → `try_files $uri /index.html;`

## <span id="local-single-port-recommended">Local – Single‑Port (Recommended)</span>

Run the full stack via **Docker Compose** (production‑like, one port):

```
docker compose down --volumes --remove-orphans
docker compose build --no-cache
docker compose up -d
```

**Open:**

- SPA → `http://localhost/`
- API → `http://localhost/publications`
- Swagger UI → `http://localhost/swagger/`
- Swagger JSON → `http://localhost/swagger/v1/swagger.json`

If SPA is under `/app`, always use the trailing slash:

`http://localhost/app/`

## <span id="ec2-single-port-deployment">EC2 ‑ Single‑Port Deployment</span>

### 1. Security Group
Allow inbound:
- `80`/tcp (HTTP)
- `22`/tcp (SSH from your IP)
### 2. SSH & Pull Latest Code

```
ssh -i your-key.pem ec2-user@<EC2_PUBLIC_IP> cd ~/REPO_LINK git pull origin main
```

### 3. Set Client Production Environment

echo `VITE_API_BASE=http://""` > client/publications-client/.env.production

### 4. Build & Run

```
docker compose down --volumes --remove-orphans
docker compose build --no-cache
docker compose up -d
```

### 5. Test
- SPA → `http://<EC2_PUBLIC_IP>/`
- API → `http://<EC2_PUBLIC_IP>/publications`
- Swagger → `http://<EC2_PUBLIC_IP>/swagger/`

### Ops Shortcuts

Update code

```
git pull origin main
```

Rebuild only API

```
docker compose build --no-cache api && docker compose up -d api
```

Rebuild only client

```
docker compose build --no-cache client && docker compose up -d client
```
Rebuild only Reverse-Proxy 

```
docker compose build --no-cache reverse-proxy && docker compose up -d reverse-proxy
```

Logs

```
docker compose logs -f reverse-proxy
docker compose logs -f api
docker compose logs -f client
```

## <span id="optional-two-port-dev">Optional ‑ Two‑Port Dev (CORS ON)</span>

For fast local development with Vite HMR.

### 1. Run API in Development Mode

`$env:ASPNETCORE_ENVIRONMENT = "Development"`

```
dotnet run –project Golden.Publication.Api/Golden.Publication.Api.csproj
```

API → `http://localhost:5031`

Swagger → `http://localhost:5031/swagger`

---

### 2. Run Client Dev Server

```
cd client/publications-client
npm install
npm run dev
```


SPA → `http://localhost:5173`
---

### 3. Client Dev Environment

`VITE_API_BASE=http://localhost:5031`

**Expected:**  
- SPA calls API without CORS issues (Development mode enables CORS).  
- Hot reload works instantly via Vite.

**Note:**  
Stop the API with `Ctrl + C`.  
Use a second terminal for `npm run dev`.


## Routes Summary

### API Routes
- `GET /publications`  
 - Query params:  
   - `search=`  
   - `sort=title|year`  
   - `page=`  
   - `pageSize=`  
- `GET /publications/{id}`  
 - Returns publication + versions

### Client Routes (React)
- `/` — Publications list  
- `/publication/:id` — Details page

### Reverse Proxy Routes (Nginx)
- `/` → React SPA  
- `/publications` → ASP.NET API  
- `/swagger/` → Swagger UI  

---




## Screenshots

**Publication exposed as JSON file**

- **Publication** JSON data (Localhost)

<img width="500" height="750" alt="publication-json-localhost" src="assets\publication-json-localhost.png" />



- **Publication** JSON data (EC2)

<img width="650" height="910" alt="publication-json-ec2" src="assets\publication-json-ec2.png" />


## SINGLE-port testing

- **SPA homepage** (Frontend) --> **Localhost** (no port)

<img width="550" height="600" alt="spa-home-localhost" src="assets\spa-home-localhost.png" />


- **SPA homepage** (Frontend) --> **EC2 instance** (no port)

<img width="550" height="600" alt="spa-home-ec2" src="assets\spa-home-ec2.png" />


- **Swagger UI** (Backend) -->  **Localhost** (no port)

<img width="1800" height="650" alt="swagger-ui-localhost" src="assets\swagger-ui-localhost.png" />


- **Swagger UI** (Backend) -->  **EC2 instance** (no port)

<img width="1800" height="680" alt="swagger-ui-ec2" src="assets\swagger-ui-ec2.png" />



## DOUBLE-port testing

- **SPA homepage** (Frontend) --> **Localhost** (port : `5173`)


<img width="550" height="600" alt="spa-home-localhost-5173" src="assets\spa-home-localhost-5173.png" />

- **Swagger UI** (Backend) --> **Localhost** (port : `5031`)

<img width="1800" height="650" alt="swagger-ui-localhost-5031" src="assets\swagger-ui-localhost-5031.png" />



---

## <span id="jenkins-cicd-pipeline">Jenkins CI/CD Pipeline</span>

The repository includes a **Jenkinsfile** that automates:
- Pulling latest code from **GitHub**  
- Building the React client  
- Building the **ASP.NET API**  
- Building **Docker** images  
- Deploying to **EC2** via **SSH**  
- Restarting the **Docker Compose** stack  

The full pipeline script is available in the root `Jenkinsfile`.


## CI/CD Architecture (Detailed)

This project uses a fully automated CI/CD pipeline built with **GitHub Webhooks**, **Jenkins**, **Docker**, and **AWS EC2**.  
The pipeline ensures that every push to `main` triggers a clean build, test, and deployment to the production **EC2 instance**.



### CI/CD Flow Overview

1. **Developer pushes code to GitHub**
   - Triggers **GitHub Webhook**

2. **GitHub Webhook notifies Jenkins**
   - **Jenkins** receives the event at `/github-webhook/`

3. **Jenkins Pipeline executes**
   - Pulls latest code
   - Builds React client
   - Builds **ASP.NET API**
   - Builds **Docker** images
   - Runs unit tests (if configured)
   - Packages artifacts
   - **SSH** deploys to **EC2**

4. **EC2 Deployment Server**
   - Pulls latest code
   - Rebuilds **Docker** images
   - Restarts **Docker Compose** stack
   - **Nginx reverse proxy** exposes everything on port `80`

5. **Production environment updates instantly**
   - SPA, API, and Swagger UI become available immediately

---

### CI/CD Architecture Diagram

```text
                  ┌──────────────────────────┐
                  │        Developer         │
                  │   Pushes to GitHub Repo  │
                  └──────────────┬───────────┘
                                 │ git push
                                 ▼
                  ┌──────────────────────────┐
                  │        GitHub            │
                  │  Sends Webhook Event     │
                  └──────────────┬───────────┘
                                 │  Webhook
                                 ▼
                  ┌────────────────────────────┐
                  │      Jenkins Server        │
                  │  (Build & CI Orchestration)│
                  │                            │
                  │  - Pull latest code        │
                  │  - Build client (npm)      │
                  │  - Build API (.NET)        │
                  │  - Build Docker images     │
                  │  - Run tests               │
                  │  - SSH deploy to EC2       │
                  └──────────────┬─────────────┘
                                 │ SSH Deploy
                                 ▼
                  ┌───────────────────────────┐
                  │      EC2 Deploy Server    │
                  │  (Docker Compose Runtime) │
                  │                           │
                  │  - Pull latest code       │
                  │  - Rebuild images         │
                  │  - Restart containers     │
                  │  - Nginx reverse proxy    │
                  └──────────────┬────────────┘
                                 │
                                 ▼
                  ┌──────────────────────────┐
                  │       Public Internet    │
                  │    http://EC2_PUBLIC_IP  │
                  │    SPA + API + Swagger   │
                  └──────────────────────────┘
