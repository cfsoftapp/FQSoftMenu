using System.Windows;
using Menu.Desktop.ViewModels;

namespace Menu.Desktop.Views;

public partial class EmpleadoFormDialog : Window
{
    private readonly EmpleadosViewModel _viewModel;

    public EmpleadoFormDialog(EmpleadosViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    private async void Guardar_Click(object sender, RoutedEventArgs e)
    {
        await _viewModel.SaveAsync();

        if (!_viewModel.ShowForm)
            DialogResult = true;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.CancelCommand.CanExecute(null))
            _viewModel.CancelCommand.Execute(null);

        DialogResult = false;
    }
}
