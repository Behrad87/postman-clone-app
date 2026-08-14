using System.Windows;

namespace PostmanCloneWPFUI.Dialogs;

public partial class PromptDialog : Window
{
    public string Message
    {
        get => MessageText.Text;
        set => MessageText.Text = value;
    }

    public string Value
    {
        get => ValueBox.Text;
        set => ValueBox.Text = value;
    }

    public PromptDialog()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            ValueBox.Focus();
            ValueBox.SelectAll();
        };
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
