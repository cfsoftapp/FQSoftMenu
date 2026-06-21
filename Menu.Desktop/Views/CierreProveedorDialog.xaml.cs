using System.IO;
using System.Windows;
using Menu.Desktop.ViewModels;
using Microsoft.Win32;

namespace Menu.Desktop.Views;

public partial class CierreProveedorDialog : Window
{
    private readonly CierreProveedorDialogViewModel _viewModel;

    public CierreProveedorDialog(CierreProveedorDialogViewModel viewModel)
    {
        _viewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
    }

    private async void Calcular_Click(object sender, RoutedEventArgs e) =>
        await _viewModel.CalcularAsync();

    private async void Guardar_Click(object sender, RoutedEventArgs e)
    {
        var result = await _viewModel.GuardarAsync();
        if (!result.Success)
            MessageBox.Show(result.Message, "Cierre", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private async void Confirmar_Click(object sender, RoutedEventArgs e)
    {
        var answer = MessageBox.Show(
            "La liquidacion quedara confirmada y ya no podra modificarse. ¿Deseas continuar?",
            "Confirmar cierre",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (answer != MessageBoxResult.Yes)
            return;

        var result = await _viewModel.ConfirmarAsync();
        MessageBox.Show(
            result.Message,
            "Cierre",
            MessageBoxButton.OK,
            result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
    }

    private async void Excel_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Guardar cierre de facturacion",
            Filter = "Libro de Excel (*.xlsx)|*.xlsx",
            FileName = $"Cierre_{_viewModel.FechaDesde:yyyyMMdd}_{_viewModel.FechaHasta:yyyyMMdd}.xlsx"
        };

        if (dialog.ShowDialog(this) != true)
            return;

        var bytes = await _viewModel.GenerateExcelAsync();
        await File.WriteAllBytesAsync(dialog.FileName, bytes);
    }

    private void Cerrar_Click(object sender, RoutedEventArgs e) => Close();
}
