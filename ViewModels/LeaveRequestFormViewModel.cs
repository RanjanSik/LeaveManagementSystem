using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.ViewModels;

public class LeaveRequestFormViewModel : IValidatableObject
{
    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "From Date")]
    public DateTime FromDate { get; set; } = DateTime.Today;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "To Date")]
    public DateTime ToDate { get; set; } = DateTime.Today;

    [Required, StringLength(500, MinimumLength = 5)]
    public string Reason { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FromDate.Date > ToDate.Date)
        {
            yield return new ValidationResult(
                "From Date must be on or before To Date.",
                new[] { nameof(FromDate), nameof(ToDate) });
        }
    }
}
