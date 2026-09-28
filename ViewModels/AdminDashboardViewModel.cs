using LeaveManagementSystem.Models;

namespace LeaveManagementSystem.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int PendingRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int TotalRequests { get; set; }

    public string? DepartmentFilter { get; set; }
    public LeaveStatus? StatusFilter { get; set; }
    public DateTime? FromDateFilter { get; set; }
    public DateTime? ToDateFilter { get; set; }

    public List<string> Departments { get; set; } = new();
    public List<LeaveRequest> RecentRequests { get; set; } = new();
}
