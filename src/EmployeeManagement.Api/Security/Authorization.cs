using Microsoft.AspNetCore.Authorization;

namespace EmployeeManagement.Api.Security;

public static class Roles
{
    public const string Viewer = "Viewer";
    public const string HrManager = "HrManager";
    public const string HrAdmin = "HrAdmin";

    public static readonly IReadOnlyList<string> All = [Viewer, HrManager, HrAdmin];
}

// Controllers ask for a capability (policy), not a role, so the role mapping can change in one place.
public static class Policies
{
    public const string CanReadData = nameof(CanReadData);
    public const string CanManageEmployees = nameof(CanManageEmployees);
    public const string CanManageDepartments = nameof(CanManageDepartments);

    public static void Configure(AuthorizationOptions options)
    {
        options.AddPolicy(CanReadData, policy => policy.RequireRole(Roles.Viewer, Roles.HrManager, Roles.HrAdmin));
        options.AddPolicy(CanManageEmployees, policy => policy.RequireRole(Roles.HrManager, Roles.HrAdmin));
        options.AddPolicy(CanManageDepartments, policy => policy.RequireRole(Roles.HrAdmin));

        // Anything without an explicit [Authorize]/[AllowAnonymous] still needs a signed-in user.
        options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    }
}
