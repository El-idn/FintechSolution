using SharedKernel.Constants;


namespace SharedKernel.Constants;

public static class PermissionConstants
{
    public const string CreateAccount = "CreateAccount";
    public const string ViewAuditLogs = "ViewAuditLogs";
    public const string AccessPII = "AccessPII";

    // Optional additions for future services
    public const string MakeTransfer = "MakeTransfer";
    public const string ViewOwnTransactions = "ViewOwnTransactions";
    public const string ViewCustomerAccounts = "ViewCustomerAccounts";
}
