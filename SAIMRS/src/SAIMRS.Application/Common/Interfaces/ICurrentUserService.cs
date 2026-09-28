namespace SAIMRS.Application.Common.Interfaces;

/// <summary>Who is performing the action. Implemented by the Desktop layer (logged-in admin).</summary>
public interface ICurrentUserService
{
    string DisplayName { get; }
}
