namespace SharedKernel.Constants;

public static class PolicyConstants
{
    // ✅ Role-based policies
    public const string OnlyAdmins = "OnlyAdmins";
    public const string OnlyCustomers = "OnlyCustomers";
    public const string OnlyAuditors = "OnlyAuditors";

    // ✅ Permission-based policies
    public const string CanCreateAccount = "CanCreateAccount";
    public const string CanViewAuditLogs = "CanViewAuditLogs";
    public const string CanAccessPII = "CanAccessPII";
}
