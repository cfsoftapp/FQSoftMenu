using System.Windows;
using System.Windows.Controls;
using Menu.Desktop.ViewModels;

namespace Menu.Desktop.Views;

public partial class CuentasPorCobrarView : UserControl
{
    public CuentasPorCobrarView()
    {
        InitializeComponent();
    }

    private async void RegistrarPago_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not CuentasPorCobrarViewModel viewModel ||
            !viewModel.CanRegisterPayment)
        {
            return;
        }

        var dialog = new PagoCreditoDialog(viewModel.SelectedEmployeeName, viewModel.TotalSeleccionado)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() != true)
            return;

        await viewModel.RegistrarPagoAsync(
            dialog.FormaPago,
            dialog.FechaPago,
            dialog.Observacion);
    }
}
