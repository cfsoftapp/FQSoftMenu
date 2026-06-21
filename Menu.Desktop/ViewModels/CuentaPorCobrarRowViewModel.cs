using Menu.DTOs;
using Menu.Enums;

namespace Menu.Desktop.ViewModels;

public sealed class CuentaPorCobrarRowViewModel : ObservableObject
{
    private readonly Func<CuentaPorCobrarRowViewModel, bool, bool> _selectionChanging;
    private readonly Action _selectionChanged;
    private bool _isSelected;

    public CuentaPorCobrarRowViewModel(
        CuentaPorCobrarDto cuenta,
        Func<CuentaPorCobrarRowViewModel, bool, bool> selectionChanging,
        Action selectionChanged)
    {
        Cuenta = cuenta;
        _selectionChanging = selectionChanging;
        _selectionChanged = selectionChanged;
    }

    public CuentaPorCobrarDto Cuenta { get; }

    public DateTime Fecha => Cuenta.Fecha;

    public int EmpleadoId => Cuenta.EmpleadoId;

    public string Dni => Cuenta.Dni;

    public string EmpleadoNombre => Cuenta.EmpleadoNombre;

    public string Concepto => Cuenta.Concepto;

    public string Detalle => Cuenta.Descripcion;

    public string Categoria =>
        Cuenta.Categoria?.ToString() ??
        Cuenta.TipoServicio?.ToString() ??
        "-";

    public decimal Precio => Cuenta.Precio;

    public string UsuarioRegistro => Cuenta.UsuarioRegistroNombre;

    public string ConceptoBrush =>
        Cuenta.TipoCuenta == TipoCuentaPorCobrar.MenuPrincipal
            ? "#FF9800"
            : "#2196F3";

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value || !_selectionChanging(this, value))
                return;

            if (SetProperty(ref _isSelected, value))
                _selectionChanged();
        }
    }

    public void SetSelectedSilently(bool value)
    {
        SetProperty(ref _isSelected, value);
    }
}
