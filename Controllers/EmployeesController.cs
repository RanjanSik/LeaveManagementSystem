using LeaveManagementSystem.Data;
using LeaveManagementSystem.Models;
using LeaveManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Controllers;

[Authorize(Roles = nameof(UserRole.Admin))]
public class EmployeesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher<Employee> _passwordHasher;

    public EmployeesController(ApplicationDbContext db, IPasswordHasher<Employee> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<IActionResult> Index(string? search)
    {
        try
        {
            var query = _db.Employees
                .Include(e => e.LeaveRequests)
                .Where(e => e.Role == UserRole.Employee)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(e =>
                    e.FullName.ToLower().Contains(term) ||
                    e.Email.ToLower().Contains(term) ||
                    (e.Department != null && e.Department.ToLower().Contains(term)));
            }

            var employees = await query
                .OrderBy(e => e.FullName)
                .Select(e => new EmployeeListItemViewModel
                {
                    Id = e.Id,
                    FullName = e.FullName,
                    Email = e.Email,
                    Department = e.Department,
                    Role = e.Role,
                    IsActive = e.IsActive,
                    TotalLeaveCount = e.LeaveRequests.Count,
                    ApprovedLeaveCount = e.LeaveRequests.Count(l => l.Status == LeaveStatus.Approved),
                    PendingLeaveCount = e.LeaveRequests.Count(l => l.Status == LeaveStatus.Pending)
                })
                .ToListAsync();

            ViewBag.Search = search;
            return View(employees);
        }
        catch (Exception)
        {
            TempData["Error"] = "Unable to load employees.";
            return View(new List<EmployeeListItemViewModel>());
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new EmployeeFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "Password is required when creating an employee.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var emailExists = await _db.Employees.AnyAsync(e => e.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
                return View(model);
            }

            var employee = new Employee
            {
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim().ToLowerInvariant(),
                Role = model.Role,
                Department = model.Department?.Trim(),
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };
            employee.PasswordHash = _passwordHasher.HashPassword(employee, model.Password!);

            _db.Employees.Add(employee);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Employee created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Could not create the employee. Please try again.");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        var model = new EmployeeFormViewModel
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Email = employee.Email,
            Role = employee.Role,
            Department = employee.Department,
            IsActive = employee.IsActive,
            IsEdit = true
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EmployeeFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        model.IsEdit = true;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var employee = await _db.Employees.FindAsync(id);
            if (employee is null)
            {
                return NotFound();
            }

            var emailTaken = await _db.Employees
                .AnyAsync(e => e.Email == model.Email && e.Id != id);
            if (emailTaken)
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
                return View(model);
            }

            employee.FullName = model.FullName.Trim();
            employee.Email = model.Email.Trim().ToLowerInvariant();
            employee.Role = model.Role;
            employee.Department = model.Department?.Trim();
            employee.IsActive = model.IsActive;

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                employee.PasswordHash = _passwordHasher.HashPassword(employee, model.Password);
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Employee updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Could not update the employee. Please try again.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            var employee = await _db.Employees.FindAsync(id);
            if (employee is null)
            {
                return NotFound();
            }

            if (employee.Role == UserRole.Admin)
            {
                TempData["Error"] = "Admin accounts cannot be deactivated from this screen.";
                return RedirectToAction(nameof(Index));
            }

            employee.IsActive = false;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"{employee.FullName} has been deactivated.";
        }
        catch (Exception)
        {
            TempData["Error"] = "Could not deactivate the employee.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        try
        {
            var employee = await _db.Employees.FindAsync(id);
            if (employee is null)
            {
                return NotFound();
            }

            employee.IsActive = true;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"{employee.FullName} has been activated.";
        }
        catch (Exception)
        {
            TempData["Error"] = "Could not activate the employee.";
        }

        return RedirectToAction(nameof(Index));
    }
}
