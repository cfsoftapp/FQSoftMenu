using System.Globalization;
using Menu.DTOs.Cierres;
using Menu.Enums;

namespace Menu.Desktop.ViewModels;

public sealed class CierreProveedorRowViewModel
{
    private static readonly CultureInfo Culture = new("es-PE");

    public CierreProveedorRowViewModel(CierreProveedorListadoDto cierre)
    {
        Cierre = cierre;
    }

    public CierreProveedorListadoDto Cierre { get; }

    public int Id => Cierre.Id;

    public string Periodo => $"{Cierre.FechaDesde:dd/MM/yyyy} al {Cierre.FechaHasta:dd/MM/yyyy}";

    public string Estado => Cierre.Estado.ToString();

    public bool EsBorrador => Cierre.Estado == EstadoCierreProveedor.Borrador;

    public string EstadoBackground => EsBorrador ? "#FF9800" : "#00C853";

    public int TotalItems => Cierre.TotalMenus;

    public string TotalText => Cierre.TotalLiquidarProveedor.ToString("C2", Culture);

    public string RevisionText => Cierre.TotalExcluidoRevision.ToString("C2", Culture);

    public string FechaRegistro => Cierre.FechaRegistro.ToString("dd/MM/yyyy HH:mm");

    public string Usuario => Cierre.UsuarioRegistroNombre;
}
