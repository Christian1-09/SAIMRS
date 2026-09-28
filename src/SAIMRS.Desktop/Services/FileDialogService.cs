using Microsoft.Win32;

namespace SAIMRS.Desktop.Services;

/// <summary>Native Windows open/save dialogs (used for exports, backups and the profile photo).</summary>
public sealed class FileDialogService
{
    public const string PdfFilter = "PDF document (*.pdf)|*.pdf";
    public const string ExcelFilter = "Excel workbook (*.xlsx)|*.xlsx";
    public const string BackupFilter = "SAIMRS backup (*.db)|*.db|All files (*.*)|*.*";
    public const string ImageFilter = "Images (*.png;*.jpg;*.jpeg;*.gif;*.webp)|*.png;*.jpg;*.jpeg;*.gif;*.webp";

    /// <summary>Returns the chosen path, or null when the admin cancels.</summary>
    public string? PickSaveFile(string suggestedFileName, string filter, string title = "Save file") =>
        OnUiThread(() =>
        {
            var dialog = new SaveFileDialog
            {
                Title = title,
                FileName = suggestedFileName,
                Filter = filter,
                AddExtension = true,
                OverwritePrompt = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            return dialog.ShowDialog(System.Windows.Application.Current.MainWindow) == true ? dialog.FileName : null;
        });

    public string? PickOpenFile(string filter, string title = "Open file") =>
        OnUiThread(() =>
        {
            var dialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                CheckFileExists = true,
                Multiselect = false
            };
            return dialog.ShowDialog(System.Windows.Application.Current.MainWindow) == true ? dialog.FileName : null;
        });

    private static T OnUiThread<T>(Func<T> action)
    {
        var dispatcher = System.Windows.Application.Current.Dispatcher;
        return dispatcher.CheckAccess() ? action() : dispatcher.Invoke(action);
    }
}
