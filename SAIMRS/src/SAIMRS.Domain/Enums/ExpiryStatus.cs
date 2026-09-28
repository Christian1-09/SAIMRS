namespace SAIMRS.Domain.Enums;

public enum ExpiryStatus
{
    /// <summary>More days left than the near-expiry window.</summary>
    Safe = 1,

    /// <summary>Within the near-expiry window (default 30 days).</summary>
    NearExpiry = 2,

    /// <summary>7 days or fewer left.</summary>
    Critical = 3,

    /// <summary>Expiry date has passed.</summary>
    Expired = 4
}
