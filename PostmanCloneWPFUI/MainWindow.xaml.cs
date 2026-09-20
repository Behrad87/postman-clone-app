using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using PostmanCloneWPFUI.ViewModels;

namespace PostmanCloneWPFUI;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Closing += OnClosing;
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.FlushSaveAsync().GetAwaiter().GetResult();
    }

    private void CopyAs_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.ContextMenu is null) return;
        button.ContextMenu.PlacementTarget = button;
        button.ContextMenu.Placement = PlacementMode.Bottom;
        button.ContextMenu.IsOpen = true;
    }

    private void SavedRequestList_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListBox { SelectedItem: SavedRequestViewModel req } &&
            WindowViewModel() is { } vm)
        {
            vm.OpenSavedRequestCommand.Execute(req);
        }
    }

    private void HistoryList_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListBox { SelectedItem: HistoryItemViewModel item } &&
            WindowViewModel() is { } vm)
        {
            vm.OpenHistoryItemCommand.Execute(item);
        }
    }

    private MainViewModel? WindowViewModel() => DataContext as MainViewModel;
}
