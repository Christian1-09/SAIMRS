namespace SAIMRS.Domain.Common;

/// <summary>
/// Thrown when a business rule is broken (for example, issuing more stock than is on hand).
/// The Application layer catches it and turns it into a friendly error message.
/// </summary>
public sealed class DomainException(string message) : Exception(message);
