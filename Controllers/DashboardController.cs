using LeaveManagementSystem.Data;
using LeaveManagementSystem.Helpers;
using LeaveManagementSystem.Models;
using LeaveManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;

    public DashboardController(ApplicationDbContext db)
    {
        _db = db;
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Admin(
        string? department,
        LeaveStatus? status,
        DateTime? fromDate,
        DateTime? toDate)
    {
        try
        {
            var query = _db.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.ReviewedBy)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(department))
            {
                query = query.Where(l => l.Employee != null && l.Employee.Department == department);
            }

            if (status.HasValue)
            {
                query = query.Where(l => l.Status == status.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(l => l.FromDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(l => l.ToDate <= toDate.Value.Date);
            }

            var employees = await _db.Employees
                .Where(e => e.Role == UserRole.Employee)
                .ToListAsync();

            var allRequests = await _db.LeaveRequests.AsNoTracking().ToListAsync();
            var filtered = await query
                .OrderByDescending(l => l.CreatedAt)
                .Take(50)
                .ToListAsync();

            var departments = await _db.Employees
                .Where(e => e.Department != null && e.Department != "")
                .Select(e => e.Department!)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            var model = new AdminDashboardViewModel
            {
                TotalEmployees = employees.Count,
                ActiveEmployees = employees.Count(e => e.IsActive),
                PendingRequests = allRequests.Count(r => r.Status == LeaveStatus.Pending),
                ApprovedRequests = allRequests.Count(r => r.Status == LeaveStatus.Approved),
                RejectedRequests = allRequests.Count(r => r.Status == LeaveStatus.Rejected),
                TotalRequests = allRequests.Count,
                DepartmentFilter = department,
                StatusFilter = status,
                FromDateFilter = fromDate,
                ToDateFilter = toDate,
                Departments = departments,
                RecentRequests = filtered
            };

            return View(model);
        }
        catch (Exception)
        {
            TempData["Error"] = "Unable to load the admin dashboard right now.";
            return View(new AdminDashboardViewModel());
        }
    }

    [Authorize(Roles = nameof(UserRole.Employee))]
    public async Task<IActionResult> Employee()
    {
        try
        {
            var userId = User.GetUserId();
            var employee = await _db.Employees.FindAsync(userId);
            if (employee is null)
            {
                return RedirectToAction("Login", "Account");
            }

            var leaves = await _db.LeaveRequests
                .Include(l => l.ReviewedBy)
                .Where(l => l.EmployeeId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            var model = new EmployeeDashboardViewModel
            {
                FullName = employee.FullName,
                PendingCount = leaves.Count(l => l.Status == LeaveStatus.Pending),
                ApprovedCount = leaves.Count(l => l.Status == LeaveStatus.Approved),
                RejectedCount = leaves.Count(l => l.Status == LeaveStatus.Rejected),
                TotalCount = leaves.Count,
                LeaveHistory = leaves
            };

            return View(model);
        }
        catch (Exception)
        {
            TempData["Error"] = "Unable to load your dashboard right now.";
            return View(new EmployeeDashboardViewModel());
        }
    }

    public IActionResult Index()
    {
        return User.IsAdmin()
            ? RedirectToAction(nameof(Admin))
            : RedirectToAction(nameof(Employee));
    }
}
