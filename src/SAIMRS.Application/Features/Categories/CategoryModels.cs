namespace SAIMRS.Application.Features.Categories;

public sealed record CategoryDto(int Id, string Name, string? Description, int ItemCount, DateTime CreatedAt);

public sealed class SaveCategoryRequest
{
    /// <summary>Null when creating a new category.</summary>
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
