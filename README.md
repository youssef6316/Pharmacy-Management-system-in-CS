# Pharmacy Management System (PMS_CS)

A desktop Pharmacy Management System built with **C# WinForms**, **.NET Framework**, and **ADO.NET** connecting to a **Microsoft SQL Server** backend. The application supports role-based access for Patients, Cashiers, Pharmacists, and Administrators, providing a complete workflow from inventory management and prescription handling to order placement and payment processing.

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Database Architecture](#database-architecture)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Database Setup](#database-setup)
  - [Configuration](#configuration)
  - [Running the Application](#running-the-application)
- [Default Users & Credentials](#default-users--credentials)
- [Application Workflow](#application-workflow)
- [CRUD Operations](#crud-operations)
- [Key Design Decisions](#key-design-decisions)
- [Notes & Known Limitations](#notes--known-limitations)

---

## Features

### For Patients
- **Account Registration & Login** — Create a patient profile with demographics and allergy information.
- **Browse Medicines** — View available inventory with stock levels and pricing.
- **Place Orders** — Add medicines to a cart and submit orders; stock is deducted automatically.
- **Top-Up Wallet** — Add balance to an internal patient account.
- **Make Payments** — Pay for pending orders using Credit Card, Cash, or Fawry.
- **Order History** — View past and current orders with status tracking.

### For Employees (Pharmacists / Cashiers / Admins)
- **Employee Login & Registration** — Secure signup gated by a validation code (`PHARMACY123`).
- **Inventory Management** — Add new medicines (Admin only) and view stock levels.
- **Process Orders** — Cashiers/Admins can complete or cancel pending orders.
- **Prescription Management** — Backend support for issuing, filling, and cancelling prescriptions with patient allergy-safety checks.
- **Role-Based Dashboard** — UI buttons are conditionally enabled based on job type.

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| **Language** | C# |
| **Framework** | .NET Framework (WinForms) |
| **Data Access** | ADO.NET (`System.Data.SqlClient` / `Microsoft.Data.SqlClient`) |
| **Database** | Microsoft SQL Server |
| **Architecture** | N-Tier (Models → Repositories → Services → Views) |

---

## Database Architecture

The database follows a fully normalized relational schema derived from an Enhanced Entity-Relationship Diagram (EERD).

### Core Entities
- **USER** — Central identity table (login credentials, contact info, active status).
- **PATIENT** — Subtype of USER storing age, address, and wallet balance.
- **EMPLOYEE** — Subtype of USER storing salary and job type (`Admin`, `Cashier`, `Pharmacist`).
- **MEDICINE** — Inventory catalog with price, stock, expiry, and refundability.
- **ORDER** — Transaction header linked to a patient and a cashier.
- **ORDER_ITEM** — Line items linking orders to medicines with quantity and unit price.
- **PRESCRIPTION** — Medical authorization issued by a pharmacist for a patient.
- **PRESCRIPTION_ITEM** — Medicines and quantities within a prescription.
- **PAYMENT** — Financial settlement record linked to an order.
- **ROLE / USER_ROLE** — Role-based permission system.

### Multi-Valued Attributes (Normalized)
- `PATIENT_ALLERGY` — Patient allergy records.
- `MED_SIDE_EFFECT` — Adverse effects per medicine.
- `MED_HEALING_EFFECT` — Therapeutic effects per medicine.

> **Schema File:** `Creation.sql`  
> **Seed Data:** `Insertion.sql`

---

## Project Structure

```
.gitignore
Creation.sql
Insertion.sql
Pharmacy_Management_System.pdf   # Project's Documentation
PMS_CS.sln                       # Solution file
README.md
PMS_CS.Tests/                    # Xunit Test project for testing the Backend's functionality
PMS_CS/
├── PMS_CS.csproj
├── Program.cs                   # Project's entry point
├── Database/
│   └── DBConnection.cs          # SQL Server connection string
├── src/
│   ├── Models/                  # POCO entity classes
│   │   ├── User.cs
│   │   ├── Patient.cs
│   │   ├── Employee.cs
│   │   ├── Medicine.cs
│   │   ├── Order.cs
│   │   ├── OrderItem.cs
│   │   ├── Prescription.cs
│   │   ├── PrescriptionItem.cs
│   │   ├── Payment.cs
│   │   └── Role.cs
│   ├── Repositories/            # ADO.NET data access layer
│   │   ├── UserRepository.cs
│   │   ├── PatientRepository.cs
│   │   ├── EmployeeRepository.cs
│   │   ├── MedicineRepository.cs
│   │   ├── OrderRepository.cs
│   │   ├── PrescriptionRepository.cs
│   │   └── PaymentRepository.cs
│   └── Services/                # Business logic layer
│       ├── UserService.cs
│       ├── PatientService.cs
│       ├── InventoryService.cs
│       ├── OrderService.cs
│       ├── PrescriptionService.cs
│       └── PaymentService.cs
└── Views/                       # WinForms UI layer
    ├── MainForm.cs              # Host shell / navigation router
    ├── EntryView.cs             # Landing screen
    ├── PatientLoginView.cs
    ├── PatientSignupView.cs
    ├── PatientProfileView.cs
    ├── OrderView.cs
    ├── PaymentView.cs
    ├── PharmacistLoginView.cs
    ├── PharmacistSignupView.cs
    ├── PharmacistProfileView.cs
    ├── InventoryView.cs
    ├── ReceivedOrdersView.cs
    └── UIHelper.cs              # Session manager + Prompt dialog
```

---

## Getting Started

### Prerequisites

- Windows OS
- Visual Studio (2019 or later recommended)
- .NET Framework (target version as per project settings)
- Microsoft SQL Server (Express or higher) with SQL Server Management Studio (SSMS)

### Database Setup

1. Open **SQL Server Management Studio (SSMS)**.
2. Execute `Creation.sql` to create the database and all tables with constraints:
   ```sql
   USE PharmacyDB;
   GO
   -- Run the full CREATE TABLE script
   ```
3. Execute `Insertion.sql` to populate the database with sample data (3+ rows per table).

> **Note:** The default connection string in `DBConnection.cs` uses the database name `PharmacyDB`. Ensure this matches your SSMS database name, or update the `Database` constant accordingly.

### Configuration

Open `Database/DBConnection.cs` and verify the connection parameters:

```csharp
private const string Server   = "YOUR_SERVER_NAME";   // e.g., localhost\SQLEXPRESS
private const string Database = "PharmacyDB";         // or "PharmacyDB"
```

The connection string uses **Windows Authentication** (`Integrated Security=True`).

### Running the Application

1. Open the solution (`.sln`) in Visual Studio.
2. Restore NuGet packages if prompted (requires `Microsoft.Data.SqlClient`).
3. Build the solution (`Ctrl+Shift+B`).
4. Run the project (`F5`).

The application launches at the **EntryView**, presenting options for Patient or Employee login.

---

## Default Users & Credentials

The seed data (`Insertion.sql`) provides the following accounts for immediate testing:

### Patients
| Username | Password | Email | Balance |
|----------|----------|-------|---------|
| Youssef | hash123 | john.patient@example.com | $0.00 |
| Mohamed | hash123 | jane@example.com | $50.50 |
| Shamia | hash123 | bob@example.com | $0.00 |

### Employees
| Username | Password | Email | Job Type | Salary |
|----------|----------|-------|----------|--------|
| Neveen | hash123 | alice@pharmacy.com | Admin | $80,000 |
| Musa | hash123 | john.cashier@pharmacy.com | Cashier | $40,000 |
| Omar | hash123 | phil@pharmacy.com | Pharmacist | $95,000 |

> **Employee Signup Code:** `PHARMACY123` (required when registering new staff).

---

## Application Workflow

### Patient Journey
```
EntryView → Patient Login → Patient Profile
                                ├── Place Order → OrderView → (Submit)
                                ├── Proceed to Payment → PaymentView → (Pay)
                                ├── Top Up Balance → (Dialog)
                                └── Logout → EntryView
```

### Employee Journey
```
EntryView → Employee Login → PharmacistProfileView
                                 ├── Manage Inventory → InventoryView (Admin/Pharmacist only)
                                 ├── Received Orders → ReceivedOrdersView (Admin/Cashier only)
                                 └── Logout → EntryView
```

Navigation is centralized via `MainForm.LoadPage()`, swapping UserControls in a docked panel for a seamless single-page desktop experience.

---

## CRUD Operations

The application implements all required database interactions:

| Operation | Implementation |
|-----------|----------------|
| **SELECT** | Authentication, patient/employee profile loading, inventory browsing, order history, pending order queues, allergy/side-effect lookups, role resolution. |
| **INSERT** | Patient/employee registration, order placement (transactional), payment recording, prescription issuance (transactional), medicine addition, allergy registration. |
| **UPDATE** | Order status transitions (Complete/Cancel), patient balance adjustments (debit/credit/refund), stock deductions/restorations, medicine metadata edits, salary updates, user profile changes. |
| **DELETE** | Medicine hard-delete (blocked by FK if referenced), order cancellation (status update + stock restore + refund), prescription cancellation (status update), user deactivation (soft-delete via `IsActive=0`), role revocation. |

### Transaction Safety
- **Order Placement** and **Prescription Issuance** use explicit `SqlTransaction` with `Commit()` / `Rollback()` to ensure header and line-item rows are written atomically.
- **Stock Updates** include a database guard (`StockQuantity + @Delta >= 0`) to prevent negative inventory.

---

## Key Design Decisions

- **Parameterized Queries:** All SQL commands use `SqlParameter` to prevent SQL injection.
- **Role-Based Access Control (RBAC):** Authorization is enforced at both the UI layer (button enablement) and the service layer (method guards).
- **Session Management:** A static `Session` class holds the authenticated user, patient, and employee objects across views.
- **On-Demand Loading:** Multi-valued attributes (allergies, effects) and navigation properties are loaded explicitly by repositories rather than via ORM lazy loading.
- **Separation of Concerns:** Models hold data and trivial computed properties; Repositories contain SQL; Services enforce business rules; Views handle layout and events.

---

## Notes & Known Limitations

- **Password Storage:** Passwords are stored in plain text (`hash123` in seed data). For production, integrate a hashing library (e.g., BCrypt).
- **Connection String:** Currently hardcoded in `DBConnection.cs`. For deployment, move it to `App.config` or environment variables.
- **Admin Bypass:** `InventoryView` and `PharmacistSignupView` simulate an Admin context (`dummyAdmin`) to allow standalone UI functionality without a pre-existing admin session.
- **Prescription UI:** The backend for prescriptions is fully implemented, but there is no dedicated WinForms view for issuing prescriptions in the current upload set.
---

## License

This project was developed for academic purposes as part of a Database Management Systems course.
