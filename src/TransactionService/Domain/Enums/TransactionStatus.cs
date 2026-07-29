namespace TransactionService.Domain.Enums
{
    public enum TransactionStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Cancelled,
        Reversed,
        Disputed,
        UnderReview
    }
} 