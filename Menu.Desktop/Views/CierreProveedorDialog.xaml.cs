using System.IO;
using System.ComponentModel;
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

    private async void Calcular_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _viewModel.CalcularAsync();
        }
        catch (Exception ex)
        {
            ShowError("No se pudo calcular el cierre.", ex);
        }
    }

    private async void Guardar_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await _viewModel.GuardarAsync();
            MessageBox.Show(
                result.Message,
                "Cierre",
                MessageBoxButton.OK,
                result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            ShowError("No se pudo guardar el borrador.", ex);
        }
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

        try
        {
            var result = await _viewModel.ConfirmarAsync();
            MessageBox.Show(
                result.Message,
                "Cierre",
                MessageBoxButton.OK,
                result.Success ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            ShowError("No se pudo confirmar el cierre.", ex);
        }
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

        try
        {
            var bytes = await _viewModel.GenerateExcelAsync();
            await File.WriteAllBytesAsync(dialog.FileName, bytes);
        }
        catch (Exception ex)
        {
            ShowError("No se pudo generar el archivo Excel.", ex);
        }
    }

    private void Cerrar_Click(object sender, RoutedEventArgs e) => Close();

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (!_viewModel.HasUnsavedChanges)
            return;

        var answer = MessageBox.Show(
            "Hay cambios sin guardar en el cierre. Si sales, se perderan esos cambios.",
            "Cerrar cierre de facturacion",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (answer != MessageBoxResult.Yes)
            e.Cancel = true;
    }

    private void ShowError(string message, Exception exception)
    {
        MessageBox.Show(
            $"{message}{Environment.NewLine}{exception.Message}",
            "Cierre de facturacion",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}
