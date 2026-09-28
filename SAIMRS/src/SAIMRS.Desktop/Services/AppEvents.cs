namespace SAIMRS.Desktop.Services;

/// <summary>
/// Lets components know that data changed (e.g. a transaction was logged),
/// so the sidebar badges and the bell count refresh.
/// </summary>
public sealed class AppEvents
{
    public event Action? DataChanged;

    public void NotifyDataChanged() => DataChanged?.Invoke();
}
