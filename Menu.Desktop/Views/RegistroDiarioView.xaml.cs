using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Menu.Desktop.ViewModels;
using Menu.Enums;

namespace Menu.Desktop.Views;

public partial class RegistroDiarioView : UserControl
{
    public RegistroDiarioView()
    {
        InitializeComponent();
    }

    private void RegistroDiarioView_Loaded(object sender, RoutedEventArgs e)
    {
        BusquedaTextBox.Focus();
        Keyboard.Focus(BusquedaTextBox);
    }

    private void BusquedaTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        if (DataContext is not RegistroDiarioViewModel viewModel)
            return;

        var command = viewModel.RegistrarMenuAlEscanear
            ? viewModel.RegistrarMenuRapidoCommand
            : viewModel.BuscarCommand;

        if (command.CanExecute(null))
        {
            command.Execute(null);
            e.Handled = true;
        }
    }

    private void ResultadosBusqueda_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0)
            return;

        if (DataContext is RegistroDiarioViewModel viewModel &&
            viewModel.SeleccionarCommand.CanExecute(null))
        {
            viewModel.SeleccionarCommand.Execute(null);
        }
    }

    private void AdditionalTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source != AdditionalTabs ||
            DataContext is not RegistroDiarioViewModel viewModel)
        {
            return;
        }

        var inactiveType = AdditionalTabs.SelectedIndex == 0
            ? TipoAdicional.Producto
            : TipoAdicional.MenuExtra;

        viewModel.DescartarBorradoresVacios(inactiveType);
    }

    private async void AnularConsumo_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: RegistroDiarioConsumoRowViewModel consumo })
            return;

        if (DataContext is not RegistroDiarioViewModel viewModel)
            return;

        var dialog = new AnularConsumoDialog(consumo)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true)
            await viewModel.AnularConsumoAsync(consumo, dialog.MotivoTexto);
    }
}
