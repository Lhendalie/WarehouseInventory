namespace WarehouseInventory.Domain.Entities;

public class ExpiryWarning : BaseEntity
{
    public Guid ProductStockId { get; set; }
    public DateTime WarningSentAt { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int DaysToExpiry { get; set; }
    public Guid RecipientUserId { get; set; }
    public EmailStatus EmailStatus { get; set; }

    // Navigation properties
    public ProductStock ProductStock { get; set; } = null!;
    public User RecipientUser { get; set; } = null!;
}

public enum EmailStatus
{
    Sent,
    Failed,
    Pending
}
