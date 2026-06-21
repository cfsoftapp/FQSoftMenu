using System.IO;
using System.Windows;
using System.Windows.Controls;
using Menu.Desktop.ViewModels;
using Microsoft.Win32;

namespace Menu.Desktop.Views;

public partial class CierresView : UserControl
{
    private CierresViewModel? _viewModel;

    public CierresView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_viewModel is not null)
            _viewModel.RequestCierreDialog -= OpenDialog;

        _viewModel = e.NewValue as CierresViewModel;
        if (_viewModel is not null)
            _viewModel.RequestCierreDialog += OpenDialog;
    }

    private async void OpenDialog(CierreProveedorRowViewModel? cierre)
    {
        if (_viewModel is null)
            return;

        try
        {
            var dialogViewModel = _viewModel.CreateDialogViewModel();
            var dialog = new CierreProveedorDialog(dialogViewModel)
            {
                Owner = Window.GetWindow(this)
            };

            if (cierre is not null)
                await dialogViewModel.LoadExistingAsync(cierre);

            dialog.ShowDialog();
            await _viewModel.LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"No se pudo abrir el cierre: {ex.Message}",
                "Cierres",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void Abrir_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel is not null &&
            sender is Button { CommandParameter: CierreProveedorRowViewModel cierre })
        {
            _viewModel.Open(cierre);
        }
    }

    private async void Eliminar_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel is null ||
            sender is not Button { CommandParameter: CierreProveedorRowViewModel cierre })
            return;

        var answer = MessageBox.Show(
            "Se eliminara el borrador y su detalle. Los consumos registrados no se modificaran.",
            "Eliminar borrador",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (answer != MessageBoxResult.Yes)
            return;

        try
        {
            await _viewModel.DeleteAsync(cierre);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"No se pudo eliminar el borrador: {ex.Message}",
                "Cierres",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void Excel_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel is null ||
            sender is not Button { CommandParameter: CierreProveedorRowViewModel cierre })
            return;

        var dialog = new SaveFileDialog
        {
            Title = "Guardar cierre de facturacion",
            Filter = "Libro de Excel (*.xlsx)|*.xlsx",
            FileName = $"Cierre_{cierre.Cierre.FechaDesde:yyyyMMdd}_{cierre.Cierre.FechaHasta:yyyyMMdd}.xlsx"
        };

        if (dialog.ShowDialog(Window.GetWindow(this)) != true)
            return;

        try
        {
            var bytes = await _viewModel.GenerateExcelAsync(cierre);
            await File.WriteAllBytesAsync(dialog.FileName, bytes);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"No se pudo generar el Excel: {ex.Message}",
                "Cierres",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
