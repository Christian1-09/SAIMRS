using SAIMRS.Domain.Enums;

namespace SAIMRS.Application.Common;

/// <summary>Builds a reference number when the admin leaves the field blank.</summary>
public static class ReferenceNumber
{
    public static string Generate(TransactionType type, DateTime at)
    {
        var prefix = type switch
        {
            TransactionType.In => "RCV",
            TransactionType.Out => "ISS",
            _ => "ADJ"
        };
        return $"{prefix}-{at:yyyyMMdd-HHmmss}";
    }
}
