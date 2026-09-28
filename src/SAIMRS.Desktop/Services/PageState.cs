namespace SAIMRS.Desktop.Services;

/// <summary>Title shown in the top bar. Each page sets it when it opens.</summary>
public sealed class PageState
{
    public string Title { get; private set; } = "Dashboard";

    public event Action? Changed;

    public void SetTitle(string title)
    {
        if (Title == title) return;
        Title = title;
        Changed?.Invoke();
    }
}
