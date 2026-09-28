namespace SAIMRS.Domain.Enums;

public enum TransactionType
{
    /// <summary>Stock received from a supplier or a harvest.</summary>
    In = 1,

    /// <summary>Stock issued (distributed) to a beneficiary.</summary>
    Out = 2,

    /// <summary>Correction after a physical count.</summary>
    Adjust = 3
}
