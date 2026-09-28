using SAIMRS.Domain.Common;

namespace SAIMRS.Domain.Entities;

public class Supplier : BaseEntity, IArchivable
{
    private Supplier() { } // for EF Core

    public Supplier(string name, string? contactPerson, string? phone, string? itemsSupplied, DateTime now)
    {
        Update(name, contactPerson, phone, itemsSupplied, now);
        CreatedAt = now;
        UpdatedAt = null;
    }

    public string Name { get; private set; } = string.Empty;
    public string? ContactPerson { get; private set; }
    public string? Phone { get; private set; }
    public string? ItemsSupplied { get; private set; }

    /// <summary>Updated automatically when a Stock IN names this supplier.</summary>
    public DateOnly? LastDeliveryDate { get; private set; }

    public bool IsArchived { get; private set; }
    public DateTime? ArchivedAt { get; private set; }

    public void Update(string name, string? contactPerson, string? phone, string? itemsSupplied, DateTime now)
    {
        Name = Guard.Required(name, "Supplier name", 120);
        ContactPerson = Guard.Optional(contactPerson, "Contact person", 120);
        Phone = Guard.Optional(phone, "Phone", 30);
        ItemsSupplied = Guard.Optional(itemsSupplied, "Items supplied", 300);
        Touch(now);
    }

    public void RecordDelivery(DateOnly date)
    {
        if (LastDeliveryDate is null || date > LastDeliveryDate)
            LastDeliveryDate = date;
    }

    public void Archive(DateTime now)
    {
        IsArchived = true;
        ArchivedAt = now;
    }

    public void Restore()
    {
        IsArchived = false;
        ArchivedAt = null;
    }
}
