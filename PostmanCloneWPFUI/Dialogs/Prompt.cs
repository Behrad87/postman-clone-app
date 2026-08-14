using System.Windows;

namespace PostmanCloneWPFUI.Dialogs;

public static class Prompt
{
    public static string? Show(string title, string message, string initial = "")
    {
        var dlg = new PromptDialog
        {
            Owner = Application.Current?.MainWindow,
            Title = title,
            Message = message,
            Value = initial
        };
        return dlg.ShowDialog() == true ? dlg.Value : null;
    }
}
