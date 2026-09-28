using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeaveManagementSystem.Models;

public class LeaveRequest
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee? Employee { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "From Date")]
    public DateTime FromDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "To Date")]
    public DateTime ToDate { get; set; }

    [Required, StringLength(500)]
    public string Reason { get; set; } = string.Empty;

    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Audit fields
    public int? ReviewedById { get; set; }

    [ForeignKey(nameof(ReviewedById))]
    public Employee? ReviewedBy { get; set; }

    public DateTime? ReviewedAt { get; set; }

    [StringLength(500)]
    [Display(Name = "Admin Comments")]
    public string? ReviewComments { get; set; }

    [NotMapped]
    public int DurationDays => (ToDate.Date - FromDate.Date).Days + 1;
}
