# 💼 Asset Management System (Blazor WebAssembly + ASP.NET Core + Azure SQL)

## 🌐 Overview
The *Asset Management System* is a full-stack web application built using *Blazor WebAssembly* for the frontend and *ASP.NET Core Web API* for the backend, integrated with *Azure SQL Database* and *ASP.NET Identity* for secure authentication.

This project is fully deployed on *Microsoft Azure*.

---

## 🚀 Deployment Links

| Service | URL |
|----------|-----|
| 🌍 *Frontend (Blazor WebAssembly)* | [https://am-frontend-aanchal.azurewebsites.net](https://am-frontend-aanchal.azurewebsites.net) |
| 🖥 *Backend API (ASP.NET Core)* | [https://am-api-aanchal.azurewebsites.net](https://am-api-aanchal.azurewebsites.net) |
| 🗄 *Azure SQL Database* | Connected via EF Core to the API |

---

## ⚙ Tech Stack

### 🧩 Frontend
- *Blazor WebAssembly (.NET 8)*
- *Tailwind CSS + Custom CSS*
- *Authentication via ASP.NET Identity cookies*
- *Deployed on Azure App Service*

### 🔧 Backend
- *ASP.NET Core Web API (.NET 8)*
- *Entity Framework Core (SQL Server)*
- *Dapper for optimized queries*
- *ASP.NET Identity for authentication*
- *CORS + HTTPS enforced*
- *Deployed on Azure App Service*

### 💾 Database
- *Azure SQL Database (Standard S0 Tier)*
- Auto-seeded admin credentials on first run.

---

## 🔑 Default Admin Credentials

| Field | Value |
|--------|--------|
| *Username* | admin |
| *Password* | admin@123 |

> These can be configured in appsettings.json or Azure App Configuration:
> json
> "AdminCredentials": {
>   "Username": "admin",
>   "Password": "YourStrongPassword123!"
> }
> 

---

## 🧱 Project Structure
AssetManagementSolution/
│
├── AssetManagementAPI/ # Backend (.NET 8 Web API)
│ ├── Controllers/ # API Endpoints
│ ├── Data/ # EF Core Context + Seeder
│ ├── Models/ # Entity Models
│ ├── Program.cs # Startup Configuration
│ ├── appsettings.production.json # Azure connection settings
│
├── AssetManagementWASM/ # Frontend (Blazor WebAssembly)
│ ├── Pages/ # Razor Pages (Login, Dashboard, etc.)
│ ├── Shared/ # Layouts and Components
│ ├── wwwroot/ # Static files (CSS/JS)
│ ├── Program.cs # Blazor app entry point
│
└── README.md # Documentation


---

## 🧭 Local Setup Instructions

### 1️⃣ Clone the Repository
```bash
git clone https://github.com/<your-username>/asset-management-azure.git
cd asset-management-azure

2️⃣ Setup Backend
cd AssetManagementAPI
dotnet restore
dotnet ef database update
dotnet run


Backend will start at https://localhost:5299.

3️⃣ Setup Frontend
cd ../AssetManagementWASM
dotnet run


Frontend will run at https://localhost:5074.

☁ Azure Deployment Summary
Component	Azure Service	Name
Backend API	Azure App Service	am-api-aanchal
Frontend App	Azure App Service (Static)	am-frontend-aanchal
Database	Azure SQL Database	AssetManagementDB
Resource Group	Azure Resource Group	am-rg
📡 API Endpoints
Method	Endpoint	Description
POST	/api/auth/login	Login with credentials
POST	/api/auth/logout	Logout the user
GET	/api/auth/status	Check authentication status
GET	/api/assets	Get all assets
POST	/api/assets	Add new asset
...	...	...
🧠 Notes

This app uses cookie-based authentication (no JWT).

CORS, HTTPS redirection, and secure cookies are enforced.

If testing locally, ensure both frontend and backend are running on HTTPS.
