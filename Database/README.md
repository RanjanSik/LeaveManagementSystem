# Database Setup

## Automatic (recommended)

On application startup, EF Core migrations are applied and default users are seeded when the database is empty.

Running `dotnet run` creates `LeaveManagement.db` in the project root.

## Manual SQL

`schema.sql` contains the SQLite DDL generated from EF migrations:

```bash
sqlite3 LeaveManagement.db < Database/schema.sql
```

Default users are still created by the app seeder (`Data/DbSeeder.cs`) using hashed passwords. Prefer starting the app once so seeding runs.

## Switching to SQL Server

1. Replace the SQLite package with `Microsoft.EntityFrameworkCore.SqlServer`.
2. Update `Program.cs` to call `UseSqlServer(...)`.
3. Set `ConnectionStrings:DefaultConnection` in `appsettings.json`.
4. Recreate migrations for SQL Server.
