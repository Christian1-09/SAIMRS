namespace SAIMRS.Domain.Common;

/// <summary>Base class for every table-backed entity.</summary>
public abstract class BaseEntity
{
    public int Id { get; protected set; }

    public DateTime CreatedAt { get; protected set; }

    public DateTime? UpdatedAt { get; protected set; }

    protected void Touch(DateTime now) => UpdatedAt = now;
}
