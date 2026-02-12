# Poultry Distribution Management System

A full-stack application for managing poultry distribution: farms, shops, orders, inventory, deliveries, payments, and analytics. Built with **.NET 8** (Clean Architecture) and **React 19** (TypeScript).

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-19-61DAFB?logo=react)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-4.9-3178C6?logo=typescript)](https://www.typescriptlang.org/)
[![MySQL](https://img.shields.io/badge/MySQL-8+-4479A1?logo=mysql)](https://www.mysql.com/)

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Project Structure](#project-structure)
- [API Reference](#api-reference)
- [Database](#database)
- [Scripts Reference](#scripts-reference)
- [Documentation](#documentation)
- [License & Contributing](#license--contributing)

---

## Overview

This system supports:

- **Admins**: Manage farms, chickens, vehicles, drivers, shops, inventory, orders (approve/fulfill), expenses, reports, and analytics.
- **Customers**: Place orders, view deliveries, make payments, and receive notifications.

It includes inventory tracking, order workflows, PDF invoices/receipts, expense tracking, profit & loss reports, route optimization, demand forecasting, and optional multi-tenant (SaaS) support.

---

## Features

### Core (MVP)

| Feature | Description |
|--------|-------------|
| **Inventory tracking** | Real-time stock per farm; movement types: In, Out, Loss, Adjustment; CSV export. |
| **Order management** | Create orders, approve/reject workflow, partial fulfillment, customer order tracking. |
| **Notifications** | In-app and email: delivery scheduled, payment reminder, order approved/rejected/fulfilled, low stock alerts. |

### Business & Reporting

| Feature | Description |
|--------|-------------|
| **Expense tracking** | Categories (Fuel, Vehicle Maintenance, Salary, Feed, Utilities, Other); filter by category and date range. |
| **Profit & loss** | Revenue vs expenses, profit margin. |
| **Invoices & receipts** | PDF generation for sales invoices, payment receipts, and expense receipts (QuestPDF). |
| **Audit logs** | Full audit trail of system changes (admin-only). |
| **Analytics** | Sales trends, customer analytics, inventory utilization, performance metrics (Recharts). |

### Advanced

| Feature | Description |
|--------|-------------|
| **Route optimization** | Delivery route optimization (nearest-neighbor algorithm). |
| **Demand forecasting** | Moving average, seasonal trends, stock level recommendations. |
| **Multi-tenant SaaS** | Subdomain-based tenants; plans: Basic, Professional, Enterprise. |

### Integrations

- **JWT** – Authentication with role-based access (Admin / Customer).
- **Stripe** – Payment processing.
- **SMTP** – Email notifications.
- **File storage** – Single folder for uploads; images are not stored in the database.

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| **Backend** | .NET 8, ASP.NET Core Web API, Entity Framework Core, MySQL |
| **Frontend** | React 19, TypeScript, React Router, Axios, Recharts |
| **Auth** | JWT Bearer |
| **Validation** | FluentValidation |
| **Mapping** | AutoMapper |
| **PDF** | QuestPDF |
| **Payments** | Stripe |

---

## Architecture

### Backend (Clean Architecture)

The solution is split into four projects:

| Project | Responsibility |
|---------|----------------|
| **Domain** | Entities, enums, domain interfaces. |
| **Application** | Services, DTOs, validators, mappings (business logic). |
| **Infrastructure** | EF Core DbContext, repositories, unit of work, external services (email, PDF, file storage, etc.). |
| **API** | Controllers, middleware, startup; thin controllers that delegate to application services. |

```
PoultryDistributionSystem.sln
├── PoultryDistributionSystem.Domain/       # Entities, Enums, Interfaces
├── PoultryDistributionSystem.Application/ # Services, DTOs, Validators, Mappings
├── PoultryDistributionSystem.Infrastructure/ # EF Core, Repositories, Unit of Work, External services
└── PoultryDistributionSystem.API/          # Controllers, Middleware, Program.cs
```

**Conventions:**

- Repository + Unit of Work for data access.
- DTOs are separate from entities.
- Database: `snake_case` tables/columns, UUID primary keys, soft delete (`is_deleted`).
- Audit fields: `created_at`, `updated_at`, `created_by`.
- Global exception middleware; file-based TXT logging.

### Frontend

- **Structure**: Functional components, React hooks, centralized API layer (Axios).
- **Roles**: Admin panel (full CRUD and reports); Customer panel (orders, deliveries, payments, profile, notifications).
- **Routing**: React Router with protected and role-based routes; auth stored in context.

---

## Prerequisites

Before you begin, ensure you have:

| Requirement | Version / Notes |
|-------------|-----------------|
| .NET SDK | 8.0 |
| Node.js | 18 or later (LTS recommended) |
| npm | Comes with Node.js |
| MySQL | 8.0 or later |
| (Optional) | Stripe account, SMTP credentials for payments and email |

---

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/Elayaraja1609/poultry-distribution-system.git
cd poultry-distribution-system
```

### 2. Backend setup

All backend commands below assume you are in the **project root** unless stated otherwise.

**Step 2.1 – Restore packages**

```bash
dotnet restore
```

**Step 2.2 – Configure database**

Edit `PoultryDistributionSystem.API/appsettings.json` and set your MySQL connection string under `ConnectionStrings:DefaultConnection`. Example:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=poultryDB;User=root;Password=YOUR_PASSWORD;"
}
```

**Step 2.3 – Apply migrations**

From the **project root**:

```bash
dotnet ef database update --project PoultryDistributionSystem.Infrastructure --startup-project PoultryDistributionSystem.API
```

Or from inside the API project:

```bash
cd PoultryDistributionSystem.API
dotnet ef database update
cd ..
```

**Step 2.4 – Run the API**

```bash
cd PoultryDistributionSystem.API
dotnet run
```

- API base URL: check the console (e.g. `https://localhost:7xxx` or `http://localhost:5xxx`).
- **Swagger UI**: open `https://localhost:7xxx/swagger` (replace port if different).

Leave the API running for the next step.

### 3. Frontend setup

Open a **new terminal** and run:

```bash
cd frontend
npm install
npm start
```

- Frontend runs at **http://localhost:3000**.
- Ensure the frontend is configured to call your API base URL (see [Configuration – Frontend](#frontend-api-base-url) below).

### 4. Verify

- Open http://localhost:3000 in a browser.
- Use Swagger to test API endpoints.
- Register a user and log in (Admin or Customer depending on your seed data).

---

## Configuration

### Backend – `PoultryDistributionSystem.API/appsettings.json`

| Section | Key / Purpose |
|--------|----------------|
| **ConnectionStrings** | `DefaultConnection` – MySQL connection string. |
| **Jwt** | `SecretKey` (min 32 characters), `Issuer`, `Audience`, `AccessTokenExpiryMinutes`, `RefreshTokenExpiryDays`. |
| **FileStorage** | `BasePath` (e.g. `wwwroot`), `MaxFileSizeBytes`. |
| **Stripe** | `SecretKey`, `PublishableKey` – for payment processing. |
| **Email** | `SmtpServer`, `SmtpPort`, `SmtpUsername`, `SmtpPassword`, `FromEmail`, `FromName`. |

**Security:** Do not commit real secrets. Use `appsettings.Development.json` (and ensure it is gitignored) or environment variables in production.

### Frontend – API base URL

The frontend calls the backend using a base URL defined in `frontend/src/services/api.ts`. Update it to match your running API, for example:

```ts
const API_BASE_URL = 'https://localhost:7xxx';  // Use the port shown when you run the API
```

You can replace this with an environment variable (e.g. `process.env.REACT_APP_API_URL`) for different environments.

---

## Project Structure

```
poultry-distribution-system/
├── PoultryDistributionSystem.API/           # Web API, controllers, middleware
├── PoultryDistributionSystem.Application/  # Services, DTOs, validators
├── PoultryDistributionSystem.Domain/       # Entities, enums
├── PoultryDistributionSystem.Infrastructure/ # DbContext, repositories, external services
├── frontend/                                # React app
│   ├── public/
│   └── src/
│       ├── components/
│       ├── config/
│       ├── contexts/
│       ├── pages/
│       └── services/
├── IMPLEMENTATION_SUMMARY.md                # Detailed features, schema, API list
├── MOBILE_APP_API.md                        # API spec for mobile driver app
└── README.md
```

---

## API Reference

The API is REST-style. When the backend is running, use **Swagger** at `/swagger` for the full list of endpoints and to try them out.

### Base URL

- Local: `https://localhost:7xxx` (or the port shown in the console).

### Main areas

| Area | Description |
|------|-------------|
| **Auth** | Register, login, refresh token. |
| **Inventory** | Farm stock, movements, stock summary. |
| **Orders** | CRUD, approve/reject, fulfillment; customers use `GET /api/orders/my`. |
| **Notifications** | List, unread count, mark as read. |
| **Expenses** | CRUD, filter by category/date, receipt PDF. |
| **Reports** | Dashboard, sales, profit-loss, revenue, expenses, trends, analytics. |
| **PDFs** | Invoice (sales), receipt (payments, expenses). |
| **Audit logs** | Admin-only. |
| **Route optimization** | Optimize route, distance. |
| **Forecasting** | Demand, seasonal trends, stock recommendations. |
| **Tenants** | Super-admin tenant CRUD (multi-tenant). |

For a complete endpoint list and DTOs, see **IMPLEMENTATION_SUMMARY.md** and the Swagger UI.

---

## Database

- **Engine**: MySQL 8+.
- **ORM**: Entity Framework Core.
- **Migrations**: Stored in `PoultryDistributionSystem.Infrastructure/Migrations`.

**Apply migrations** (from repo root):

```bash
dotnet ef database update --project PoultryDistributionSystem.Infrastructure --startup-project PoultryDistributionSystem.API
```

Ensure the connection string in `appsettings.json` points to your MySQL instance before running.

---

## Scripts Reference

| Where | Command | Description |
|-------|---------|-------------|
| **Backend** (API folder) | `dotnet run` | Start the API. |
| **Backend** | `dotnet ef database update ...` | Apply pending migrations. |
| **Frontend** | `npm start` | Start development server. |
| **Frontend** | `npm run build` | Production build. |
| **Frontend** | `npm test` | Run tests. |

---

## Documentation

| Document | Contents |
|----------|----------|
| **README.md** (this file) | Overview, setup, configuration, structure. |
| **IMPLEMENTATION_SUMMARY.md** | Feature list, architecture details, database schema, API summary, production notes. |
| **MOBILE_APP_API.md** | API specification for building a mobile driver app (e.g. React Native). |
| **Swagger** | Interactive API docs at `/swagger` when the API is running. |

---

## License & Contributing

**License:** This project is private/proprietary. All rights reserved.

**Contributing:**

1. Fork the repository.
2. Create a feature branch (`git checkout -b feature/your-feature`).
3. Commit your changes (`git commit -m "Add your message"`).
4. Push to the branch (`git push origin feature/your-feature`).
5. Open a Pull Request on GitHub.

---

*Poultry Distribution Management System – .NET 8 & React*
