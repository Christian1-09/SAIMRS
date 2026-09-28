namespace SAIMRS.Desktop.Services;

public sealed record ConfirmRequest(string Title, string Message, string ConfirmText, bool IsDanger, string Icon);

/// <summary>Shows a yes/no dialog and waits for the answer: <c>if (await Confirm.AskAsync(...))</c>.</summary>
public sealed class ConfirmService
{
    private TaskCompletionSource<bool>? _pending;

    public ConfirmRequest? Current { get; private set; }

    public event Action? Changed;

    public Task<bool> AskAsync(string title, string message, string confirmText = "Confirm",
        bool isDanger = false, string icon = "fa-circle-question")
    {
        _pending?.TrySetResult(false);
        _pending = new TaskCompletionSource<bool>();
        Current = new ConfirmRequest(title, message, confirmText, isDanger, icon);
        Changed?.Invoke();
        return _pending.Task;
    }

    public void Answer(bool confirmed)
    {
        Current = null;
        Changed?.Invoke();
        _pending?.TrySetResult(confirmed);
        _pending = null;
    }
}
