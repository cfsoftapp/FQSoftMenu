using System.Windows.Controls;
using Menu.Desktop.ViewModels;

namespace Menu.Desktop.Views;

public partial class EmpleadosView : UserControl
{
    private EmpleadosViewModel? _viewModel;

    public EmpleadosView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (_viewModel is not null)
            _viewModel.RequestFormDialog -= OpenFormDialog;

        _viewModel = e.NewValue as EmpleadosViewModel;

        if (_viewModel is not null)
            _viewModel.RequestFormDialog += OpenFormDialog;
    }

    private void OpenFormDialog()
    {
        if (_viewModel is null)
            return;

        var dialog = new EmpleadoFormDialog(_viewModel)
        {
            Owner = System.Windows.Window.GetWindow(this)
        };

        dialog.ShowDialog();
    }
}
