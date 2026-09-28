# Employee Leave Management System

ASP.NET Core MVC web application for managing employee leave requests with role-based access for **Admin** and **Employee**.

## Features

- Cookie-based authentication with Admin and Employee roles
- Employee management (add, edit, activate/deactivate, search)
- Leave request workflow (apply → pending → approve/reject)
- Overlapping leave date validation and FromDate ≤ ToDate checks
- Admin dashboard with summary counts and filters (department, status, date range)
- Employee dashboard with personal leave summary and history
- Audit fields for who reviewed a request and when
- Real-time SignalR toast notifications when leave status changes

## Tech Stack

| Area | Choice |
|------|--------|
| Framework | ASP.NET Core MVC (.NET 9) |
| Database | SQLite |
| Data access | Entity Framework Core |
| UI | Razor Views + Bootstrap 5 |
| Auth | Cookie authentication |
| Real-time | SignalR |

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- A modern browser

No separate SQL Server install is required. SQLite creates `LeaveManagement.db` automatically on first run.

## Setup Instructions

1. Clone or copy this project folder.
2. Open a terminal in the project root (`LeaveManagementSystem`).
3. Restore and run:

```bash
dotnet restore
dotnet run
```

4. Open the URL shown in the terminal (typically `https://localhost:7xxx` or `http://localhost:5xxx`).
5. Sign in with one of the default accounts below.

### Optional: apply EF migrations manually

Migrations run automatically at startup. To apply them yourself:

```bash
dotnet ef database update
```

SQL script for the schema is also available at:

`Database/schema.sql`

## Default Login Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@example.com` | `admin123` |
| Employee | `ranjan.sikdar@example.com` | `emp123` |

These users are seeded on first startup if the database is empty.

## Application Roles

### Admin

- Manage employees (add, edit, deactivate/activate)
- View all leave requests
- Approve or reject pending leaves (with optional comments)
- View dashboard statistics and filtered reports
- Audit trail shows reviewer name and timestamp

### Employee

- Apply for leave (From Date, To Date, Reason)
- View own leave history and status
- Dashboard summary of pending / approved / rejected leaves
- Receive a popup notification when an admin approves or rejects a request

## Project Structure

```
Controllers/     Account, Dashboard, Employees, Leaves, Home
Models/          Employee, LeaveRequest, enums
ViewModels/      Form and dashboard models with DataAnnotations
Data/            DbContext, seeder
Hubs/            SignalR leave notification hub
Views/           Razor UI
Migrations/      EF Core migrations
Database/        SQL schema script
```

## Notes

- Passwords are stored using ASP.NET Core `PasswordHasher` (not plain text).
- Overlap checks ignore rejected requests so employees can re-apply for similar dates after a rejection.
- Deactivated employees cannot sign in.
- For production, change default passwords and consider switching the connection string to SQL Server.

## License

Created as an assignment / learning project.
