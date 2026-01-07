# Publications – Full‑Stack Demo (ASP.NET API + React SPA + Docker + Jenkins CI/CD)
A complete full‑stack demo application featuring:
- **ASP.NET Web API** (search, sort, pagination, details from publications.xml)
- **React (Vite + TypeScript)** SPA frontend
- **Docker & Docker Compose** for containerized deployment
- **Nginx reverse proxy** for single‑port hosting (SPA + API + Swagger)
- **Jenkins CI/CD Pipeline** with **GitHub Webhook** for automatic deployments
Supports:
1. **Single‑port deployment (recommended)**  
  - Everything served on **port 80** (local & EC2)  
  - No CORS needed (same origin)
2. **Two‑port local development**  
  - React dev server → **5173**  
  - API → **5031**  
  - CORS enabled only in Development
---
# 📑 Table of Contents
- [Architecture](#architecture)
- [Architecture Diagram](#architecture-diagram)
- [Repo Structure](#repo-structure)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
 - [Local Development](#local-development)
 - [CI/CD (Jenkins Deployment)](#cicd-jenkins-deployment)
- [Environment Variables](#environment-variables)
- [Local – Single‑Port (Recommended)](#local--single-port-recommended)
- [EC2 – Single‑Port Deployment](#ec2--single-port-deployment)
- [Optional – Two‑Port Dev](#optional--two-port-dev)
- [Routes Summary](#routes-summary)
- [Screenshots](#screenshots)
- [Jenkins CI/CD Pipeline](#jenkins-cicd-pipeline)

# 🏗️ Architecture

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

# 🖼️ Architecture Diagram
```text
                        ┌──────────────────────────┐
                        │        GitHub Repo       │
                        │  (Frontend + Backend)    │
                        └─────────────┬────────────┘
                                      │ Webhook (Push)
                                      ▼
                        ┌──────────────────────────┐
                        │      Jenkins (EC2 #2)     │
                        │  - Node.js 20             │
                        │  - .NET SDK 10            │
                        │  - Docker                 │
                        └─────────────┬────────────┘
                                      │ SSH Deploy
                                      ▼
                        ┌──────────────────────────┐
                        │     EC2 #1 (Deploy)      │
                        │  Docker Compose runs:     │
                        │    - React Client         │
                        │    - ASP.NET API          │
                        │    - Nginx Reverse Proxy  │
                        └─────────────┬────────────┘
                                      │
                                      ▼
                        ┌──────────────────────────┐
                        │      Public Internet      │
                        │  http://EC2_PUBLIC_IP     │
                        │  (SPA + API + Swagger)    │
                        └──────────────────────────┘
```

# 📁 Repo Structure

```text
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
│     ├─ vite.config.ts
│     ├─ nginx-client.conf
│     ├─ Dockerfile
│     ├─ .env.development
│     └─ .env.production
├─ reverse-proxy/
│  └─ nginx.conf
├─ docker-compose.yml
└─ README.md
```

# 🧰 Tech Stack

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


  
# 🔧 Prerequisites

## 🖥️ Local Development
- **Node.js (LTS)** & **npm**
- **.NET SDK 10.0**
- **Docker & Docker Compose**
- **PowerShell / Bash**

## 🚀 CI/CD (Jenkins Deployment)
- Jenkins (latest LTS) on EC2  
- Node.js 20+ on Jenkins  
- .NET SDK 10.0 on Jenkins  
- Docker & Docker Compose on Jenkins  
- SSH key-based access from Jenkins → EC2  
- GitHub Webhook configured


# 🔐 Environment Variables

## Client (Vite)

### `.env.development` (two‑port dev)

`VITE_API_BASE=http://localhost:5031`

### `.env.production` (single‑port reverse proxy)

`VITE_API_BASE=http://<host>`

If SPA is served under `/app`, update:
- `vite.config.ts` → `base: '/app/'`
- React Router → `basename="/app"`
- Nginx → `try_files $uri /index.html;`

# 🏠 Local – Single‑Port (Recommended)

Run the full stack via Docker Compose (production‑like, one port):

```
docker compose down –volumes –remove-orphans
docker compose build –no-cache
docker compose up -d
```

**Open:**

- SPA → `http://localhost/`
- API → `http://localhost/publications`
- Swagger UI → `http://localhost/swagger/`
- Swagger JSON → `http://localhost/swagger/v1/swagger.json`

If SPA is under `/app`, always use the trailing slash:

`http://localhost/app/`

# ☁️ EC2 – Single‑Port Deployment (Docker + Docker Compose)

### 1. Security Group
Allow inbound:
- 80/tcp (HTTP)
- 22/tcp (SSH from your IP)
### 2. SSH & Pull Latest Code

```
ssh -i your-key.pem ec2-user@<EC2_PUBLIC_IP> cd ~/microchip-interview-private git pull origin main
```

### 3. Set Client Production Environment

echo `VITE_API_BASE=http://<EC2_PUBLIC_IP>` > client/publications-client/.env.production

### 4. Build & Run

```
docker compose down –volumes –remove-orphans docker compose build –no-cache docker compose up -d
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
docker compose build –no-cache api && docker compose up -d api
```

Rebuild only client

```
docker compose build –no-cache client && docker compose up -d client
```

Logs

```
docker compose logs -f reverse-proxy
docker compose logs -f api
docker compose logs -f client
```

# 🔄 Optional – Two‑Port Dev (CORS ON)

For fast local development with Vite HMR.

### 1. Run API in Development Mode

```
$env:ASPNETCORE_ENVIRONMENT = “Development” dotnet run –project Microchip.Interview.Api/Microchip.Interview.Api.csproj
```

API → `http://localhost:5031`
Swagger → `http://localhost:5031/swagger`
---

### 2. Run Client Dev Server

```
cd client/publications-client npm install npm run dev
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


# 🛣️ Routes Summary

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




# 📸 Screenshots

**Publication exposed as JSON file**

- Publication JSON data (Localhost)

<img width="500" height="750" alt="publication-json-localhost" src="https://github.com/user-attachments/assets/b4730708-e2d9-4aeb-a39d-5a3b6aee64a2" />



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



---

# ⚙️ CI/CD Pipeline (Jenkins)

The repository includes a Jenkinsfile that automates:
- Pulling latest code from GitHub  
- Building the React client  
- Building the ASP.NET API  
- Building Docker images  
- Deploying to EC2 via SSH  
- Restarting the Docker Compose stack  

The full pipeline script is available in the root `Jenkinsfile`.
