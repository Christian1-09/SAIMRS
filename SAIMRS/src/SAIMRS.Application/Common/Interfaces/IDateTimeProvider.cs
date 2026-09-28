namespace SAIMRS.Application.Common.Interfaces;

/// <summary>Abstracts the clock so business rules can be tested with a fixed date.</summary>
public interface IDateTimeProvider
{
    DateTime Now { get; }

    DateOnly Today => DateOnly.FromDateTime(Now);
}
