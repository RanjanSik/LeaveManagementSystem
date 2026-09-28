using System.ComponentModel.DataAnnotations;
using LeaveManagementSystem.Models;

namespace LeaveManagementSystem.ViewModels;

public class EmployeeFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    public string? Password { get; set; }

    [Required]
    public UserRole Role { get; set; } = UserRole.Employee;

    [StringLength(100)]
    public string? Department { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public bool IsEdit { get; set; }
}
