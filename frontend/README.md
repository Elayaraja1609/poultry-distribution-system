# Poultry Distribution System – Frontend

React frontend for the **Poultry Distribution Management System**. A single application for all user roles: one app, one URL. Menu and features change based on the logged-in user's role.

[![React](https://img.shields.io/badge/React-19-61DAFB?logo=react)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-4.9-3178C6?logo=typescript)](https://www.typescriptlang.org/)

---

## Table of Contents

- [Overview](#overview)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Project Structure](#project-structure)
- [Role-Based Features](#role-based-features)
- [Available Scripts](#available-scripts)
- [Backend Dependency](#backend-dependency)

---

## Overview

This is the **web client** only. It talks to the .NET 8 backend API for all data. You do not run a separate app per role; the same build serves:

- **Admin** – Full access to dashboard, inventory, orders, logistics, deliveries, sales, reports, suppliers, farms, chickens, vehicles, expenses, stock movements, notifications.
- **ShopOwner** (Customer) – Dashboard, orders, deliveries, sales, payments, profile, notifications.
- **Driver** – Dashboard, logistics (distributions), deliveries, profile, notifications.
- **FarmManager** – Dashboard, suppliers, farms, chickens, inventory, stock movements, profile, notifications.

Authentication is JWT; the app stores the token and sends it with each API request.

---

## Tech Stack

| Category | Technology |
|----------|------------|
| **UI** | React 19, TypeScript |
| **Routing** | React Router v7 |
| **HTTP** | Axios (centralized in `src/services/api.ts`) |
| **Charts** | Recharts |
| **Payments** | Stripe (React Stripe.js) |
| **State** | React Context (e.g. `AuthContext`) |
| **Build** | Create React App (react-scripts) |

---

## Prerequisites

- **Node.js** 18+ (LTS recommended)
- **npm** (comes with Node.js)
- **Backend API** running and reachable (see [Backend Dependency](#backend-dependency))

---

## Getting Started

### 1. Install dependencies

From the **frontend** folder:

```bash
cd frontend
npm install
```

### 2. Configure the API URL

Set the backend base URL (see [Configuration](#configuration)). Either use a `.env` file or change the fallback in `src/services/api.ts`.

### 3. Run the app

```bash
npm start
```

The app opens at **http://localhost:3000**. Log in with credentials that exist in the backend (e.g. after registering or via seed data).

---

## Configuration

### API base URL

The app calls the backend using a base URL. You can set it in two ways:

**Option A – Environment variable (recommended)**

Create a `.env` file in the **frontend** folder:

```env
REACT_APP_API_URL=https://localhost:5062/api
```

Replace the URL with your backend base URL (no trailing slash). Restart `npm start` after changing `.env`.

**Option B – Fallback in code**

In `src/services/api.ts`, the fallback is used when `REACT_APP_API_URL` is not set:

```ts
const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://your-api-url/api';
```

Change this fallback if you do not use `.env`.

**Note:** Do not commit real secrets or production URLs to the repo. Add `.env` to `.gitignore` (Create React App ignores `.env.local` by default).

---

## Project Structure

```
frontend/
├── public/                 # Static assets, index.html
├── src/
│   ├── components/         # Reusable UI (Layout, LoadingSpinner, NotificationBell, etc.)
│   ├── config/             # navConfig.ts – role-based menu
│   ├── contexts/           # AuthContext
│   ├── pages/              # Route pages (Dashboard, Orders, Chickens, etc.)
│   ├── services/           # API client (api.ts) + per-entity services
│   ├── App.tsx
│   ├── index.tsx
│   └── index.css
├── package.json
├── tsconfig.json
└── README.md
```

- **components** – Shared components (e.g. `ProtectedRoute`, `RoleBasedRoute`, `StripePaymentForm`).
- **config** – `navConfig.ts` defines menu items and which roles can see them.
- **contexts** – Auth state and helpers.
- **pages** – One folder per main screen; each typically has a `.tsx` and `.css` file.
- **services** – `api.ts` is the Axios instance (base URL, auth header, interceptors). Other files are API helpers per domain (e.g. `orderService.ts`, `farmService.ts`).

---

## Role-Based Features

| Feature / Page | Admin | ShopOwner | Driver | FarmManager |
|----------------|-------|-----------|--------|-------------|
| Dashboard | ✓ | ✓ | ✓ | ✓ |
| Notifications | ✓ | ✓ | ✓ | ✓ |
| Inventory | ✓ | | | ✓ |
| Orders | ✓ | ✓ | | |
| Logistics (Distributions) | ✓ | | ✓ | |
| Deliveries | ✓ | ✓ | ✓ | |
| Sales | ✓ | ✓ | | |
| Reports | ✓ | | | |
| Suppliers | ✓ | | | ✓ |
| Farms | ✓ | | | ✓ |
| Chickens | ✓ | | | ✓ |
| Vehicles | ✓ | | | |
| Financials (Expenses) | ✓ | | | |
| Stock Movements | ✓ | | | ✓ |
| Payments | | ✓ | | |
| Profile | | ✓ | ✓ | ✓ |

Routes are protected; users only see menu items and pages allowed for their role.

---

## Available Scripts

Run these from the **frontend** directory.

| Command | Description |
|---------|-------------|
| `npm start` | Runs the app in development mode at [http://localhost:3000](http://localhost:3000). Hot reload and lint messages in the console. |
| `npm run build` | Production build into the `build/` folder. Minified and optimized for deployment. |
| `npm test` | Runs the test runner in watch mode (e.g. Jest + React Testing Library). |
| `npm run eject` | **One-way:** ejects from Create React App and copies build config into the project. Not required for normal use. |

---

## Backend Dependency

This frontend expects the **Poultry Distribution System** .NET 8 API to be running. The repo root contains the full solution (backend + this frontend).

- **Backend setup and API docs:** see the main [README.md](../README.md) in the repository root.
- **Swagger:** when the API is running, use `/swagger` on the API base URL for endpoint documentation.

Ensure the frontend’s API base URL (see [Configuration](#configuration)) points to the same backend you use for development or production.
