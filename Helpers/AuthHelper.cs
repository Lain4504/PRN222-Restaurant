using System.Security.Claims;

public static class AuthHelper
{
    
    public static bool IsAuthenticated(ClaimsPrincipal user)
    {
        return user?.Identity != null && user.Identity.IsAuthenticated;
    }

    
    public static bool IsInRole(ClaimsPrincipal user, string role)
    {
        return IsAuthenticated(user) && user.IsInRole(role);
    }

    
    public static bool IsInRoles(ClaimsPrincipal user, params string[] roles)
    {
        if (!IsAuthenticated(user)) return false;

        foreach (var role in roles)
        {
            if (user.IsInRole(role))
                return true;
        }
        return false;
    }

    
    public static bool IsAdmin(ClaimsPrincipal user)
    {
        return IsInRole(user, "Admin");
    }

    
    public static bool IsStaff(ClaimsPrincipal user)
    {
        return IsInRole(user, "Staff");
    }

    
    public static bool IsStaffOrAdmin(ClaimsPrincipal user)
    {
        return IsInRoles(user, "Admin", "Staff");
    }

    
    public static string? GetUserName(ClaimsPrincipal user)
    {
        return IsAuthenticated(user) ? user.Identity?.Name : null;
    }

   
    public static string? GetFirstRole(ClaimsPrincipal user)
    {
        return IsAuthenticated(user) ? user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value : null;
    }
}
