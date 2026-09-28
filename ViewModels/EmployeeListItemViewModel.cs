using LeaveManagementSystem.Models;

namespace LeaveManagementSystem.ViewModels;

public class EmployeeListItemViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Department { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public int TotalLeaveCount { get; set; }
    public int ApprovedLeaveCount { get; set; }
    public int PendingLeaveCount { get; set; }
}
