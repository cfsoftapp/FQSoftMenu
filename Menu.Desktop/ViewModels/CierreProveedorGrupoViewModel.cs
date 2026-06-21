using System.Collections.ObjectModel;
using System.Globalization;

namespace Menu.Desktop.ViewModels;

public sealed class CierreProveedorGrupoViewModel : ObservableObject
{
    private static readonly CultureInfo Culture = new("es-PE");
    private bool _isExpanded;

    public CierreProveedorGrupoViewModel(
        int empleadoId,
        string dni,
        string comensal,
        IEnumerable<CierreProveedorItemRowViewModel> items)
    {
        EmpleadoId = empleadoId;
        Dni = dni;
        Comensal = comensal;
        Items = new ObservableCollection<CierreProveedorItemRowViewModel>(items);
        ToggleCommand = new RelayCommand(() => IsExpanded = !IsExpanded);
    }

    public int EmpleadoId { get; }

    public string Dni { get; }

    public string Comensal { get; }

    public string NombreCompleto => $"{Dni} - {Comensal}";

    public ObservableCollection<CierreProveedorItemRowViewModel> Items { get; }

    public RelayCommand ToggleCommand { get; }

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (SetProperty(ref _isExpanded, value))
                OnPropertyChanged(nameof(ExpandIcon));
        }
    }

    public string ExpandIcon => IsExpanded ? "⌃" : "⌄";

    public int CantidadItems => Items.Count;

    public int CantidadExcepciones => Items.Count(x => x.Excluir);

    public string TotalFacturarText => Items
        .Where(x => !x.Excluir)
        .Sum(x => x.Item.Importe)
        .ToString("C2", Culture);

    public string TotalRevisionText => Items
        .Where(x => x.Excluir)
        .Sum(x => x.Item.Importe)
        .ToString("C2", Culture);

    public void NotifyTotals()
    {
        OnPropertyChanged(nameof(CantidadExcepciones));
        OnPropertyChanged(nameof(TotalFacturarText));
        OnPropertyChanged(nameof(TotalRevisionText));
    }
}
