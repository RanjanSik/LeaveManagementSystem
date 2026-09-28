using System.Security.Claims;
using LeaveManagementSystem.Models;

namespace LeaveManagementSystem.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : 0;
    }

    public static bool IsInRole(this ClaimsPrincipal user, UserRole role)
        => user.IsInRole(role.ToString());

    public static bool IsAdmin(this ClaimsPrincipal user)
        => user.IsInRole(UserRole.Admin);

    public static bool IsEmployee(this ClaimsPrincipal user)
        => user.IsInRole(UserRole.Employee);
}
