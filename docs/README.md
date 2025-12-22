# 🏪 Singular Systems Self-Service Kiosk Documentation

## 📌 Project Overview

This project is a **full-stack Singular Systems Self-Service kiosk application** designed to streamline customer interactions in a retail or service environment.
The system allows users to browse products, place orders, and process payments, while administrators manage products, users, and orders efficiently.

**Key Components:**
- **Frontend(tech-kiosk-fr):** React application for user interface and kiosk interactions
- **Backend(KioskAPI):** ASP.NET Core Web API for handling data, authentication, and business logic
- **Database:** SQL Server for persistent storage
- **Authentication:** JWT-based secure authentication for users and admins

---

## 🧩 Tech Stack

| Layer         | Technology / Framework             |
|---------------|----------------------------------|
| Frontend      | React, React Router, CSS         |
| Backend       | ASP.NET Core Web API, Entity Framework Core, Swagger |
| Database      | SQL Server                        |
| Authentication| JWT (JSON Web Tokens)             |

---

## ⚙️ Features

### Frontend (Kiosk UI)
- User-friendly touch interface
- Browse products with images, prices, quantity, and descriptions
- Add products to cart and view order summary
- Add funds to wallet
- Checkout and delivery method
- payment according to avalible balance in wallet (mock)
- Order history

### Backend (Kiosk API)
- RESTful endpoints for CRUD operations
- User authentication and role-based authorization
- Product, category, account, cart, supplier, transactions and order management
- Logging and error handling
- Secure API with JWT

### Administration
- Admin dashboard
- Admin add funds to Users
- Manage and add products
- Manage users
- View orders and transactions
- Admin can shop for users, add to cart & checkout

### User
- Browse products
- Add to cart
- Add funds to wallet
- checkout

### Backend Setup
- Open KioskAPI
- update the connection string in appsettings.json
- In your terminal run the database migrations: **donet-ef migrations init** or **dotnet-ef database update**
- in the same terminal run: **dotnet watch run**

### Frontend Setup
- Navigate and open **tech-kioskfr**
- Run npm install
- Run npm start


---
> ### Notes
> Ensure the backend is running before starting the frontend.
> The backend uses HTTPS
---

## 🏗️ Project Architecture

```text
Self-Service Kiosk System
├── tech-kiosk-fr/
|   ├── src/         # React-based Kiosk UI
│        ├── components/   # Reusable React components
│        ├── pages/        # Pages for browsing, cart, checkout
│        ├── admin/        # admin pages, manageproducts, transaction, etc
|        └── assets/       # Images, icons, CSS
|
├── backend/           # ASP.NET Core APIs
│   ├── Controllers/  # API controllers
│   ├── Models/       # Data models
│   ├── Services/     # Business logic
|   ├── Mappers/      # Convert between Models and DTOs.
|   ├── Repository/   # Handle database operations for Models.
|   ├── Interfaces/   # Define contracts for services and repositories (dependency injection).
│   └── Data/         # Database context and migrations
└── docs/              # Documentation (Docsify)

