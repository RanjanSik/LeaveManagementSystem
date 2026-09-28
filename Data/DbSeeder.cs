using LeaveManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Employee>>();

        await db.Database.MigrateAsync();

        if (await db.Employees.AnyAsync())
        {
            return;
        }

        var admin = new Employee
        {
            FullName = "System Admin",
            Email = "admin@example.com",
            Role = UserRole.Admin,
            Department = "Administration",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        admin.PasswordHash = hasher.HashPassword(admin, "admin123");

        var employee = new Employee
        {
            FullName = "Ranjan Sikdar",
            Email = "ranjan.sikdar@example.com",
            Role = UserRole.Employee,
            Department = "Engineering",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        employee.PasswordHash = hasher.HashPassword(employee, "emp123");

        db.Employees.AddRange(admin, employee);
        await db.SaveChangesAsync();
    }
}
