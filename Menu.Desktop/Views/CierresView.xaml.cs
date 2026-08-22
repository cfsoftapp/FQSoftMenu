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
        Loaded += (_, _) => EnsureViewModel();
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_viewModel is not null)
            _viewModel.RequestCierreDialog -= OpenDialog;

        _viewModel = e.NewValue as CierresViewModel;
        if (_viewModel is not null)
            _viewModel.RequestCierreDialog += OpenDialog;
    }

    private CierresViewModel? EnsureViewModel()
    {
        if (_viewModel is not null)
            return _viewModel;

        _viewModel = DataContext as CierresViewModel;
        if (_viewModel is not null)
            _viewModel.RequestCierreDialog += OpenDialog;

        return _viewModel;
    }

    private async void OpenDialog(CierreProveedorRowViewModel? cierre) =>
        await OpenDialogAsync(cierre, readOnly: false);

    private async Task OpenDialogAsync(CierreProveedorRowViewModel? cierre, bool readOnly)
    {
        var viewModel = EnsureViewModel();
        if (viewModel is null)
            return;

        try
        {
            var dialogViewModel = viewModel.CreateDialogViewModel();
            var dialog = new CierreProveedorDialog(dialogViewModel);
            var owner = Window.GetWindow(this);
            if (owner is not null && owner.IsLoaded)
                dialog.Owner = owner;

            if (cierre is not null)
                await dialogViewModel.LoadExistingAsync(cierre, readOnly);

            dialog.ShowDialog();
            await viewModel.LoadAsync();
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

    private async void Ver_Click(object sender, RoutedEventArgs e)
    {
        if (EnsureViewModel() is null ||
            sender is not Button { CommandParameter: CierreProveedorRowViewModel cierre })
            return;

        await OpenDialogAsync(cierre, readOnly: true);
    }

    private async void Editar_Click(object sender, RoutedEventArgs e)
    {
        if (EnsureViewModel() is not { CanManage: true } ||
            sender is not Button { CommandParameter: CierreProveedorRowViewModel cierre })
            return;

        await OpenDialogAsync(cierre, readOnly: false);
    }

    private async void Eliminar_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = EnsureViewModel();
        if (viewModel is not { CanManage: true } ||
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
            await viewModel.DeleteAsync(cierre);
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
        var viewModel = EnsureViewModel();
        if (viewModel is null ||
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
            var bytes = await viewModel.GenerateExcelAsync(cierre);
            await File.WriteAllBytesAsync(dialog.FileName, bytes);
            MessageBox.Show(
                "El archivo Excel se genero correctamente.",
                "Cierres",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
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
