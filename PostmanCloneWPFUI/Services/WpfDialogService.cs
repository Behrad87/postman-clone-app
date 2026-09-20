using System.Windows;
using Microsoft.Win32;
using PostmanCloneWPFUI.Dialogs;

namespace PostmanCloneWPFUI.Services;

public class WpfDialogService : IDialogService
{
    public void ShowMessage(string message, string title = "Information")
    {
        var owner = Application.Current?.MainWindow;
        if (owner is not null)
            MessageBox.Show(owner, message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        else
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public bool Confirm(string message, string title = "Confirm")
    {
        var owner = Application.Current?.MainWindow;
        var res = owner is not null
            ? MessageBox.Show(owner, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question)
            : MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return res == MessageBoxResult.Yes;
    }

    public string? Prompt(string title, string message, string initial = "")
    {
        return Dialogs.Prompt.Show(title, message, initial);
    }

    public string? ShowOpenFileDialog(string title, string filter = "All files (*.*)|*.*")
    {
        var dlg = new OpenFileDialog
        {
            Title = title,
            Filter = filter
        };
        var owner = Application.Current?.MainWindow;
        var res = owner is not null ? dlg.ShowDialog(owner) : dlg.ShowDialog();
        return res == true ? dlg.FileName : null;
    }

    public string? ShowSaveFileDialog(string title, string defaultFileName, string filter = "All files (*.*)|*.*")
    {
        var dlg = new SaveFileDialog
        {
            Title = title,
            FileName = defaultFileName,
            Filter = filter
        };
        var owner = Application.Current?.MainWindow;
        var res = owner is not null ? dlg.ShowDialog(owner) : dlg.ShowDialog();
        return res == true ? dlg.FileName : null;
    }

    public void SetClipboardText(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            try
            {
                Clipboard.SetText(text);
            }
            catch
            {
                // In case of clipboard locked by another process
            }
        }
    }
}
