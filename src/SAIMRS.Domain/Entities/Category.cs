using SAIMRS.Domain.Common;

namespace SAIMRS.Domain.Entities;

public class Category : BaseEntity
{
    private readonly List<InventoryItem> _items = [];

    private Category() { } // for EF Core

    public Category(string name, string? description, DateTime now)
    {
        Update(name, description, now);
        CreatedAt = now;
        UpdatedAt = null;
    }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public IReadOnlyCollection<InventoryItem> Items => _items;

    public void Update(string name, string? description, DateTime now)
    {
        Name = Guard.Required(name, "Category name", 80);
        Description = Guard.Optional(description, "Description", 300);
        Touch(now);
    }
}
