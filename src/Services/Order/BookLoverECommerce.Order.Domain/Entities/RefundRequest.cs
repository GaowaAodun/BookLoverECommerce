using BookLoverECommerce.Order.Domain.Enums;

namespace BookLoverECommerce.Order.Domain.Entities;

public sealed class RefundRequest
{
    private RefundRequest() { }

    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public string CustomerId { get; private set; } = string.Empty;

    public decimal Amount { get; private set; }

    public string Reason { get; private set; } = string.Empty;

    public RefundStatus Status { get; private set; }

    public string? AdminUserId { get; private set; }

    public string? AdminComment { get; private set; }

    public DateTime RequestedAt { get; private set; }

    public DateTime? ReviewedAt { get; private set; }

    public DateTime? RefundedAt { get; private set; }

    public RefundRequest(
        Guid orderId,
        string customerId,
        decimal amount,
        string reason,
        DateTime requestedAt)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid order ID is required.",
                nameof(orderId));
        }

        if (string.IsNullOrWhiteSpace(customerId))
        {
            throw new ArgumentException(
                "Customer ID is required.",
                nameof(customerId));
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Refund amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException(
                "A refund reason is required.",
                nameof(reason));
        }

        if (requestedAt == default)
        {
            throw new ArgumentException(
                "A valid request time is required.",
                nameof(requestedAt));
        }

        Id = Guid.NewGuid();
        OrderId = orderId;
        CustomerId = customerId.Trim();
        Amount = amount;
        Reason = reason.Trim();
        Status = RefundStatus.Requested;
        RequestedAt = requestedAt;
    }

    public void Approve(
        string adminUserId,
        string? comment,
        DateTime reviewedAt)
    {
        EnsureRequested();
        ValidateAdminUserId(adminUserId);
        ValidateReviewTime(reviewedAt);

        Status = RefundStatus.Approved;
        AdminUserId = adminUserId.Trim();
        AdminComment = NormalizeComment(comment);
        ReviewedAt = reviewedAt;
    }

    public void Reject(
        string adminUserId,
        string comment,
        DateTime reviewedAt)
    {
        EnsureRequested();
        ValidateAdminUserId(adminUserId);
        ValidateReviewTime(reviewedAt);

        if (string.IsNullOrWhiteSpace(comment))
        {
            throw new ArgumentException(
                "A rejection comment is required.",
                nameof(comment));
        }

        Status = RefundStatus.Rejected;
        AdminUserId = adminUserId.Trim();
        AdminComment = comment.Trim();
        ReviewedAt = reviewedAt;
    }

    public void MarkRefunded(
        string adminUserId,
        string? comment,
        DateTime refundedAt)
    {
        if (Status != RefundStatus.Approved)
        {
            throw new InvalidOperationException(
                "Only an approved refund request can be completed.");
        }

        ValidateAdminUserId(adminUserId);

        if (refundedAt == default ||
            refundedAt < RequestedAt ||
            (ReviewedAt.HasValue &&
             refundedAt < ReviewedAt.Value))
        {
            throw new ArgumentException(
                "Refund time cannot be earlier than the request or review time.",
                nameof(refundedAt));
        }

        Status = RefundStatus.Refunded;
        AdminUserId = adminUserId.Trim();

        if (!string.IsNullOrWhiteSpace(comment))
        {
            AdminComment = comment.Trim();
        }

        RefundedAt = refundedAt;
    }

    private void EnsureRequested()
    {
        if (Status != RefundStatus.Requested)
        {
            throw new InvalidOperationException(
                "Only a requested refund can be reviewed.");
        }
    }

    private static void ValidateAdminUserId(
        string adminUserId)
    {
        if (string.IsNullOrWhiteSpace(adminUserId))
        {
            throw new ArgumentException(
                "Admin user ID is required.",
                nameof(adminUserId));
        }
    }

    private void ValidateReviewTime(DateTime reviewedAt)
    {
        if (reviewedAt == default ||
            reviewedAt < RequestedAt)
        {
            throw new ArgumentException(
                "Review time cannot be earlier than the request time.",
                nameof(reviewedAt));
        }
    }

    private static string? NormalizeComment(
        string? comment)
    {
        return string.IsNullOrWhiteSpace(comment)
            ? null
            : comment.Trim();
    }
}