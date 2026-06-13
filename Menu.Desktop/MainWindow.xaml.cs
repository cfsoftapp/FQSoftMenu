using System.Windows;
using Menu.Desktop.ViewModels;

namespace Menu.Desktop;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
