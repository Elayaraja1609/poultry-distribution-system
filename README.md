# Poultry Distribution Management System

A full-stack **poultry distribution management system** for farms, shops, orders, inventory, deliveries, and analytics. Built with **.NET 8** (Clean Architecture) and **React 19** with TypeScript.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![React](https://img.shields.io/badge/React-19-61DAFB?logo=react)
![TypeScript](https://img.shields.io/badge/TypeScript-4.9-3178C6?logo=typescript)
![MySQL](https://img.shields.io/badge/MySQL-8+-4479A1?logo=mysql)

---

## Features

### Core (MVP)
- **Inventory tracking** – Real-time stock per farm, movements (In/Out/Loss/Adjustment), CSV export
- **Order management** – Create, approve/reject, partial fulfillment, customer order tracking
- **Notifications** – In-app + email (delivery scheduled, payment reminder, order status, low stock)

### Business & Reporting
- **Expense tracking** – Categories (Fuel, Maintenance, Salary, Feed, Utilities, etc.), date filters
- **Profit & loss reports** – Revenue vs expenses, profit margin
- **Invoices & receipts** – PDF generation for sales, payments, expenses (QuestPDF)
- **Audit logs** – Full system change trail (admin-only)
- **Analytics** – Sales trends, customer analytics, inventory utilization, performance metrics (Recharts)

### Advanced
- **Route optimization** – Delivery route optimization (nearest-neighbor)
- **Demand forecasting** – Moving average, seasonal trends, stock recommendations
- **Multi-tenant SaaS** – Subdomain-based tenants, subscription plans (Basic/Professional/Enterprise)

### Integrations
- **JWT** authentication with role-based access (Admin/Customer)
- **Stripe** payments
- **SMTP** email notifications
- **File storage** – Single folder, no DB storage for images

---

## Tech Stack

| Layer        | Technology |
|-------------|------------|
| **Backend** | .NET 8, ASP.NET Core Web API, EF Core, MySQL |
| **Frontend**| React 19, TypeScript, React Router, Axios, Recharts |
| **Auth**    | JWT Bearer |
| **Validation** | FluentValidation |
| **Mapping** | AutoMapper |
| **PDF**     | QuestPDF |
| **Payments**| Stripe |

---

## Architecture

**Backend** follows **Clean Architecture**:

```
PoultryDistributionSystem.sln
├── PoultryDistributionSystem.Domain      # Entities, Enums, Interfaces
├── PoultryDistributionSystem.Application # Services, DTOs, Validators, Mappings
├── PoultryDistributionSystem.Infrastructure # EF Core, Repositories, Unit of Work, External services
└── PoultryDistributionSystem.API         # Controllers, Middleware, Program.cs
```

- **Repository + Unit of Work** for data access  
- **Thin controllers**; business logic in Application services  
- **DTOs** separate from entities  
- **snake_case** tables/columns, **UUID** primary keys, **soft delete** (`is_deleted`)  
- **Audit fields**: `created_at`, `updated_at`, `created_by`  
- **Global exception middleware**; file-based TXT logging  

**Frontend**:
- Functional components, hooks, centralized API service (Axios)
- Admin and Customer panels with role-based routes
- Protected routes and auth context

---

## Prerequisites

- **.NET 8 SDK**
- **Node.js** 18+ and npm
- **MySQL** 8+
- (Optional) Stripe account, SMTP credentials for full functionality

---

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/your-username/poultry-distribution-system.git
cd poultry-distribution-system
```

### 2. Backend setup

```bash
# Restore packages
dotnet restore

# Update connection string in appsettings
# PoultryDistributionSystem.API/appsettings.json

# Run migrations (from API project directory)
cd PoultryDistributionSystem.API
dotnet ef database update

# Run API
dotnet run
```

API runs at **https://localhost:7xxx** (or port in `launchSettings.json`). Swagger: **https://localhost:7xxx/swagger**.

### 3. Frontend setup

```bash
cd frontend
npm install
npm start
```

Frontend runs at **http://localhost:3000**.

---

## Configuration

### Backend – `PoultryDistributionSystem.API/appsettings.json`

| Section | Purpose |
|--------|---------|
| `ConnectionStrings:DefaultConnection` | MySQL connection string |
| `Jwt` | SecretKey (min 32 chars), Issuer, Audience, token expiry |
| `FileStorage` | BasePath for uploads, MaxFileSizeBytes |
| `Stripe` | SecretKey, PublishableKey |
| `Email` | SmtpServer, SmtpPort, SmtpUsername, SmtpPassword, FromEmail |

**Do not commit real secrets.** Use `appsettings.Development.json` (gitignored) or environment variables in production.

### Frontend

Point the API base URL in `frontend/src/services/api.ts` (or use env) to your backend URL (e.g. `https://localhost:7xxx`).

---

## Project structure (high level)

```
├── PoultryDistributionSystem.API/     # Web API, Controllers, Middleware
├── PoultryDistributionSystem.Application/  # Services, DTOs, Validators
├── PoultryDistributionSystem.Domain/      # Entities, Enums
├── PoultryDistributionSystem.Infrastructure/ # DbContext, Repositories, External services
├── frontend/                        # React app (src/pages, services, components)
├── IMPLEMENTATION_SUMMARY.md        # Detailed feature and API summary
├── MOBILE_APP_API.md                # API spec for mobile driver app
└── README.md
```

---

## API overview

- **Auth** – Register, Login, refresh token  
- **Inventory** – Farm stock, movements, stock summary  
- **Orders** – CRUD, approve/reject, fulfillment, `GET /api/orders/my`  
- **Notifications** – List, unread count, mark read  
- **Expenses** – CRUD, by category/date, receipt PDF  
- **Reports** – Dashboard, sales, profit-loss, revenue, expenses, trends, analytics  
- **PDFs** – Invoice (sales), receipt (payments, expenses)  
- **Audit logs** – Admin only  
- **Route optimization** – Optimize route, distance  
- **Forecasting** – Demand, seasonal trends, stock recommendations  
- **Tenants** – Super admin tenant CRUD  

Full endpoint list and DTOs: see **IMPLEMENTATION_SUMMARY.md** and **Swagger** when the API is running.

---

## Database

- **MySQL** with Entity Framework Core
- Migrations in `PoultryDistributionSystem.Infrastructure/Migrations`
- Run `dotnet ef database update` from the API project (with correct connection string)

---

## Scripts

| Location | Command | Description |
|----------|---------|-------------|
| Backend | `dotnet run` | Run API |
| Backend | `dotnet ef database update` | Apply migrations |
| Frontend | `npm start` | Dev server |
| Frontend | `npm run build` | Production build |
| Frontend | `npm test` | Run tests |

---

## Documentation

- **IMPLEMENTATION_SUMMARY.md** – Feature list, architecture, DB schema, API summary, production notes  
- **MOBILE_APP_API.md** – API specification for a mobile driver app (React Native)  
- **Swagger** – Interactive API docs at `/swagger` when the API is running  

---

## License

This project is private/proprietary. All rights reserved.

---

## Contributing

1. Fork the repo  
2. Create a feature branch  
3. Commit and push  
4. Open a Pull Request  

---

*Poultry Distribution Management System – .NET 8 & React*
