using LeaveManagementSystem.Models;

namespace LeaveManagementSystem.ViewModels;

public class EmployeeDashboardViewModel
{
    public string FullName { get; set; } = string.Empty;
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public int TotalCount { get; set; }
    public List<LeaveRequest> LeaveHistory { get; set; } = new();
}
