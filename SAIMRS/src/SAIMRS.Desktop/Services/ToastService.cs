namespace SAIMRS.Desktop.Services;

public enum ToastKind
{
    Info,
    Success,
    Warning,
    Error
}

public sealed record ToastMessage(Guid Id, string Text, ToastKind Kind);

/// <summary>Small pop-up messages in the bottom-right corner.</summary>
public sealed class ToastService
{
    public event Action<ToastMessage>? Shown;

    public void Show(string text, ToastKind kind = ToastKind.Info) =>
        Shown?.Invoke(new ToastMessage(Guid.NewGuid(), text, kind));

    public void Success(string text) => Show(text, ToastKind.Success);
    public void Error(string text) => Show(text, ToastKind.Error);
    public void Warning(string text) => Show(text, ToastKind.Warning);
    public void Info(string text) => Show(text, ToastKind.Info);
}
