using System.ComponentModel.DataAnnotations;
using LeaveManagementSystem.Models;

namespace LeaveManagementSystem.ViewModels;

public class LeaveReviewViewModel
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public LeaveStatus Status { get; set; }
    public int DurationDays { get; set; }

    [StringLength(500)]
    [Display(Name = "Comments")]
    public string? ReviewComments { get; set; }
}
