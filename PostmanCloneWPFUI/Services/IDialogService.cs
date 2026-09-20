namespace PostmanCloneWPFUI.Services;

public interface IDialogService
{
    void ShowMessage(string message, string title = "Information");
    bool Confirm(string message, string title = "Confirm");
    string? Prompt(string title, string message, string initial = "");
    string? ShowOpenFileDialog(string title, string filter = "All files (*.*)|*.*");
    string? ShowSaveFileDialog(string title, string defaultFileName, string filter = "All files (*.*)|*.*");
    void SetClipboardText(string text);
}
