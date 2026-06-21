using System.Globalization;
using System.Windows;
using Menu.Desktop.ViewModels;
using Menu.Enums;

namespace Menu.Desktop.Views;

public partial class PagoCreditoDialog : Window
{
    public PagoCreditoDialog(string comensal, decimal total)
    {
        InitializeComponent();
        Comensal = comensal;
        TotalTexto = total.ToString("C2", new CultureInfo("es-PE"));
        DataContext = this;
    }

    public string Comensal { get; }

    public string TotalTexto { get; }

    public IReadOnlyList<OptionViewModel<FormaPagoCredito>> FormasPago { get; } =
        new[]
        {
            new OptionViewModel<FormaPagoCredito>(FormaPagoCredito.Efectivo, "Efectivo"),
            new OptionViewModel<FormaPagoCredito>(FormaPagoCredito.Yape, "Yape"),
            new OptionViewModel<FormaPagoCredito>(FormaPagoCredito.Plin, "Plin")
        };

    public FormaPagoCredito FormaPago { get; set; } = FormaPagoCredito.Efectivo;

    public DateTime FechaPago { get; set; } = DateTime.Today;

    public string? Observacion { get; set; }

    private void Confirmar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
