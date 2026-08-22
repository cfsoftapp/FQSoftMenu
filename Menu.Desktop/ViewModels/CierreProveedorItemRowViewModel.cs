using System.Globalization;
using Menu.DTOs.Cierres;

namespace Menu.Desktop.ViewModels;

public sealed class CierreProveedorItemRowViewModel : ObservableObject
{
    private static readonly CultureInfo Culture = new("es-PE");
    private readonly Action _changed;

    public CierreProveedorItemRowViewModel(CierreProveedorItemDto item, Action changed)
    {
        Item = item;
        _changed = changed;
    }

    public CierreProveedorItemDto Item { get; }

    public string Fecha => Item.Fecha.ToString("dd/MM/yyyy");

    public string Dni => Item.Dni;

    public string Comensal => Item.EmpleadoNombre;

    public string Concepto => Item.ConceptoCierre;

    public string TipoPago => Item.EsAdicionalEmpresa
        ? "Empresa cliente"
        : Item.EsPlanilla ? "Planilla" : "Empresa cliente";

    public string ImporteText => Item.Importe.ToString("C2", Culture);

    public bool Excluir
    {
        get => Item.ExcluirDeProveedor;
        set
        {
            if (Item.ExcluirDeProveedor == value)
                return;

            Item.ExcluirDeProveedor = value;
            if (!value)
                Item.MotivoExclusion = null;

            OnPropertyChanged();
            OnPropertyChanged(nameof(Motivo));
            _changed();
        }
    }

    public string? Motivo
    {
        get => Item.MotivoExclusion;
        set
        {
            if (Item.MotivoExclusion == value)
                return;

            Item.MotivoExclusion = value;
            OnPropertyChanged();
            _changed();
        }
    }
}
