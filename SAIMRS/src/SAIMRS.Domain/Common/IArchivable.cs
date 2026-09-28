namespace SAIMRS.Domain.Common;

/// <summary>
/// Records that are referenced by transactions are archived instead of deleted,
/// so the transaction history and reports stay complete.
/// </summary>
public interface IArchivable
{
    bool IsArchived { get; }

    DateTime? ArchivedAt { get; }

    void Archive(DateTime now);

    void Restore();
}
