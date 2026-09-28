using LeaveManagementSystem.Data;
using LeaveManagementSystem.Helpers;
using LeaveManagementSystem.Hubs;
using LeaveManagementSystem.Models;
using LeaveManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Controllers;

[Authorize]
public class LeavesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IHubContext<LeaveNotificationHub> _hub;

    public LeavesController(ApplicationDbContext db, IHubContext<LeaveNotificationHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    [Authorize(Roles = nameof(UserRole.Employee))]
    [HttpGet]
    public IActionResult Apply()
    {
        return View(new LeaveRequestFormViewModel());
    }

    [Authorize(Roles = nameof(UserRole.Employee))]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(LeaveRequestFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var employeeId = User.GetUserId();
            var from = model.FromDate.Date;
            var to = model.ToDate.Date;

            var hasOverlap = await _db.LeaveRequests.AnyAsync(l =>
                l.EmployeeId == employeeId &&
                l.Status != LeaveStatus.Rejected &&
                l.FromDate <= to &&
                l.ToDate >= from);

            if (hasOverlap)
            {
                ModelState.AddModelError(string.Empty,
                    "You already have a leave request that overlaps with these dates.");
                return View(model);
            }

            var leave = new LeaveRequest
            {
                EmployeeId = employeeId,
                FromDate = from,
                ToDate = to,
                Reason = model.Reason.Trim(),
                Status = LeaveStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _db.LeaveRequests.Add(leave);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Leave request submitted successfully and is pending approval.";
            return RedirectToAction("Employee", "Dashboard");
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Could not submit the leave request. Please try again.");
            return View(model);
        }
    }

    [Authorize(Roles = nameof(UserRole.Employee))]
    public async Task<IActionResult> MyRequests()
    {
        var employeeId = User.GetUserId();
        var leaves = await _db.LeaveRequests
            .Include(l => l.ReviewedBy)
            .Where(l => l.EmployeeId == employeeId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        return View(leaves);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Index(LeaveStatus? status)
    {
        try
        {
            var query = _db.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.ReviewedBy)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(l => l.Status == status.Value);
            }

            var leaves = await query
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            ViewBag.Status = status;
            return View(leaves);
        }
        catch (Exception)
        {
            TempData["Error"] = "Unable to load leave requests.";
            return View(new List<LeaveRequest>());
        }
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpGet]
    public async Task<IActionResult> Review(int id)
    {
        var leave = await _db.LeaveRequests
            .Include(l => l.Employee)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (leave is null)
        {
            return NotFound();
        }

        var model = new LeaveReviewViewModel
        {
            Id = leave.Id,
            EmployeeName = leave.Employee?.FullName ?? "Unknown",
            Department = leave.Employee?.Department,
            FromDate = leave.FromDate,
            ToDate = leave.ToDate,
            Reason = leave.Reason,
            Status = leave.Status,
            DurationDays = leave.DurationDays,
            ReviewComments = leave.ReviewComments
        };

        return View(model);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, string? reviewComments)
    {
        return await UpdateStatusAsync(id, LeaveStatus.Approved, reviewComments);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? reviewComments)
    {
        return await UpdateStatusAsync(id, LeaveStatus.Rejected, reviewComments);
    }

    private async Task<IActionResult> UpdateStatusAsync(int id, LeaveStatus newStatus, string? reviewComments)
    {
        try
        {
            var leave = await _db.LeaveRequests
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (leave is null)
            {
                return NotFound();
            }

            if (leave.Status != LeaveStatus.Pending)
            {
                TempData["Error"] = "Only pending leave requests can be updated.";
                return RedirectToAction(nameof(Index));
            }

            leave.Status = newStatus;
            leave.ReviewedById = User.GetUserId();
            leave.ReviewedAt = DateTime.UtcNow;
            leave.ReviewComments = string.IsNullOrWhiteSpace(reviewComments)
                ? null
                : reviewComments.Trim();

            await _db.SaveChangesAsync();

            var message = newStatus == LeaveStatus.Approved
                ? $"Your leave request ({leave.FromDate:dd MMM yyyy} – {leave.ToDate:dd MMM yyyy}) was approved."
                : $"Your leave request ({leave.FromDate:dd MMM yyyy} – {leave.ToDate:dd MMM yyyy}) was rejected.";

            await _hub.Clients
                .Group($"employee-{leave.EmployeeId}")
                .SendAsync("LeaveStatusUpdated", new
                {
                    leaveId = leave.Id,
                    status = newStatus.ToString(),
                    message
                });

            TempData["Success"] = $"Leave request has been marked as {newStatus}.";
            return RedirectToAction(nameof(Index), new { status = LeaveStatus.Pending });
        }
        catch (Exception)
        {
            TempData["Error"] = "Could not update the leave request.";
            return RedirectToAction(nameof(Index));
        }
    }
}
