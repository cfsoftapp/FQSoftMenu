using Menu.DTOs;
using Menu.Enums;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Menu.Desktop.ViewModels;

public sealed class RegistroDiarioAdicionalRowViewModel : ObservableObject
{
    private static readonly Regex PricePattern = new(
        @"^(?:\d+|\d{1,3}(?:,\d{3})+)(?:\.\d{0,2})?$",
        RegexOptions.Compiled);

    private CategoriaConsumoAdicional _categoria;
    private string _descripcion = string.Empty;
    private decimal _precio;
    private string _precioTexto = "0.00";
    private FormaCobroAdicional _formaCobro;

    public RegistroDiarioAdicionalRowViewModel(
        TipoAdicional tipoAdicional,
        CategoriaConsumoAdicional categoria,
        string descripcion)
    {
        TipoAdicional = tipoAdicional;
        Categoria = categoria;
        Descripcion = descripcion;
        FormaCobro = FormaCobroAdicional.Efectivo;
    }

    public TipoAdicional TipoAdicional { get; }

    public string TipoTexto => TipoAdicional == TipoAdicional.MenuExtra
        ? "Menu extra"
        : "Producto";

    public CategoriaConsumoAdicional Categoria
    {
        get => _categoria;
        set
        {
            if (SetProperty(ref _categoria, value))
                OnPropertyChanged(nameof(CategoriaTexto));
        }
    }

    public string CategoriaTexto => Categoria switch
    {
        CategoriaConsumoAdicional.MenuCarta => "Menu carta",
        CategoriaConsumoAdicional.Bebida => "Bebida",
        CategoriaConsumoAdicional.Galleta => "Galleta",
        CategoriaConsumoAdicional.Postre => "Postre",
        CategoriaConsumoAdicional.Snack => "Snack",
        CategoriaConsumoAdicional.Otro => "Otro",
        _ => Categoria.ToString()
    };

    public string Descripcion
    {
        get => _descripcion;
        set => SetProperty(ref _descripcion, value);
    }

    public decimal Precio
    {
        get => _precio;
        set
        {
            if (SetProperty(ref _precio, value))
            {
                _precioTexto = value.ToString("0.##", CultureInfo.InvariantCulture);
                OnPropertyChanged(nameof(PrecioTexto));
            }
        }
    }

    public string PrecioTexto
    {
        get => _precioTexto;
        set
        {
            if (!SetProperty(ref _precioTexto, value))
                return;

            var normalized = (value ?? string.Empty).Trim();

            if (PricePattern.IsMatch(normalized) &&
                decimal.TryParse(
                    normalized,
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands,
                    CultureInfo.InvariantCulture,
                    out var precio))
            {
                SetProperty(ref _precio, precio, nameof(Precio));
            }
            else
            {
                SetProperty(ref _precio, 0m, nameof(Precio));
            }
        }
    }

    public FormaCobroAdicional FormaCobro
    {
        get => _formaCobro;
        set
        {
            if (SetProperty(ref _formaCobro, value))
                OnPropertyChanged(nameof(FormaCobroTexto));
        }
    }

    public string FormaCobroTexto => FormaCobro switch
    {
        FormaCobroAdicional.Efectivo => "Efectivo",
        FormaCobroAdicional.Yape => "Yape",
        FormaCobroAdicional.Plin => "Plin",
        FormaCobroAdicional.CreditoComedor => "Pendiente del comensal",
        FormaCobroAdicional.Empresa => "Empresa cliente",
        _ => FormaCobro.ToString()
    };

    public ConsumoAdicionalInputDto ToInput()
    {
        return new ConsumoAdicionalInputDto
        {
            TipoAdicional = TipoAdicional,
            Categoria = Categoria,
            Descripcion = Descripcion,
            Precio = Precio,
            FormaCobro = FormaCobro
        };
    }
}
