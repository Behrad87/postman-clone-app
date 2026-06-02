using System.Windows;
using PostmanCloneWPFUI.ViewModels;

namespace PostmanCloneWPFUI.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }
}
