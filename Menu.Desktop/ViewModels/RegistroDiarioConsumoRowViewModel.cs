using System.Globalization;
using Menu.DTOs.RegistroDiario;

namespace Menu.Desktop.ViewModels;

public sealed class RegistroDiarioConsumoRowViewModel
{
    private static readonly CultureInfo Culture = new("es-PE");

    public RegistroDiarioConsumoRowViewModel(ConsumoDiaDto consumo)
    {
        Id = consumo.Id;
        Origen = consumo.Origen;
        Tipo = FormatToken(consumo.Tipo);
        Descripcion = consumo.Descripcion;
        FormaPago = FormatToken(consumo.FormaPago);
        EstadoCobro = FormatToken(consumo.EstadoCobro);
        Importe = consumo.Importe.ToString("C2", Culture);
        FechaRegistro = consumo.FechaRegistro.ToString("HH:mm");
        Estado = consumo.Anulado ? "Anulado" : "Activo";
        Anulado = consumo.Anulado;
        MotivoAnulacion = consumo.MotivoAnulacion ?? string.Empty;
        PuedeAnular = consumo.PuedeAnular;
        PagoTexto = Anulado
            ? $"Pago: {FormaPago} - Estado: Anulado"
            : EstadoCobro == "-"
            ? $"Pago: {FormaPago}"
            : $"Pago: {FormaPago} - Estado: {EstadoCobro}";
        MarkerBrush = Anulado ? "#EF5350" : Tipo.Contains("Producto", StringComparison.OrdinalIgnoreCase) ? "#2196F3" : "#4CAF50";
        BadgeBackground = Anulado ? "#F5F5F5" : Tipo.Contains("Producto", StringComparison.OrdinalIgnoreCase) ? "#E3F2FD" : "#E8F5E9";
        BadgeBorderBrush = MarkerBrush;
        BadgeForeground = MarkerBrush;
        CardBackground = Anulado ? "#FAFAFA" : "#FAFBFF";
        PrimaryForeground = Anulado ? "#888888" : "#333333";
        AmountForeground = Anulado ? "#888888" : "#333333";
        MotivoTexto = string.IsNullOrWhiteSpace(MotivoAnulacion) ? string.Empty : $"Motivo: {MotivoAnulacion}";
        HasMotivo = !string.IsNullOrWhiteSpace(MotivoTexto);
    }

    public int Id { get; }

    public string Origen { get; }

    public string Tipo { get; }

    public string Descripcion { get; }

    public string FormaPago { get; }

    public string EstadoCobro { get; }

    public string Importe { get; }

    public string FechaRegistro { get; }

    public string Estado { get; }

    public bool Anulado { get; }

    public string MotivoAnulacion { get; }

    public bool PuedeAnular { get; }

    public string PagoTexto { get; }

    public string MarkerBrush { get; }

    public string BadgeBackground { get; }

    public string BadgeBorderBrush { get; }

    public string BadgeForeground { get; }

    public string CardBackground { get; }

    public string PrimaryForeground { get; }

    public string AmountForeground { get; }

    public string MotivoTexto { get; }

    public bool HasMotivo { get; }

    private static string FormatToken(string value)
    {
        return value switch
        {
            "DescuentoPlanilla" => "Descuento planilla",
            "PagoDirecto" => "Pago directo",
            "CreditoComedor" => "Pendiente del comensal",
            "MenuPrincipal" => "Menu principal",
            "MenuExtra" => "Menu extra",
            _ => value
        };
    }
}
