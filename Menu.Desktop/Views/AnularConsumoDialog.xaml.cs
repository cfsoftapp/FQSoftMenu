using System.Windows;
using System.Windows.Controls;
using Menu.Desktop.ViewModels;
using Menu.Enums;

namespace Menu.Desktop.Views;

public partial class AnularConsumoDialog : Window
{
    public AnularConsumoDialog(RegistroDiarioConsumoRowViewModel consumo)
    {
        InitializeComponent();

        TipoText.Text = consumo.Tipo;
        DescripcionText.Text = consumo.Descripcion;
        ImporteText.Text = $"Importe: {consumo.Importe}";
        HoraText.Text = $"Hora registro: {consumo.FechaRegistro}";

        MotivoCombo.ItemsSource = new[]
        {
            new OptionViewModel<MotivoAnulacionConsumo>(MotivoAnulacionConsumo.ErrorRegistro, "Error de registro"),
            new OptionViewModel<MotivoAnulacionConsumo>(MotivoAnulacionConsumo.TrabajadorEquivocado, "Comensal equivocado"),
            new OptionViewModel<MotivoAnulacionConsumo>(MotivoAnulacionConsumo.TipoServicioEquivocado, "Tipo de servicio equivocado"),
            new OptionViewModel<MotivoAnulacionConsumo>(MotivoAnulacionConsumo.FormaPagoEquivocada, "Forma de pago equivocada"),
            new OptionViewModel<MotivoAnulacionConsumo>(MotivoAnulacionConsumo.ProductoEquivocado, "Producto equivocado"),
            new OptionViewModel<MotivoAnulacionConsumo>(MotivoAnulacionConsumo.Duplicado, "Registro duplicado"),
            new OptionViewModel<MotivoAnulacionConsumo>(MotivoAnulacionConsumo.NoConsumio, "El comensal no consumió"),
            new OptionViewModel<MotivoAnulacionConsumo>(MotivoAnulacionConsumo.Otro, "Otro")
        };

        UpdateConfirmState();
    }

    public string MotivoTexto { get; private set; } = string.Empty;

    private void Confirmar_Click(object sender, RoutedEventArgs e)
    {
        if (MotivoCombo.SelectedItem is not OptionViewModel<MotivoAnulacionConsumo> motivo)
            return;

        var observacion = ObservacionText.Text.Trim();
        MotivoTexto = string.IsNullOrWhiteSpace(observacion)
            ? motivo.Text
            : $"{motivo.Text}: {observacion}";

        DialogResult = true;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void MotivoCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateConfirmState();
    }

    private void ObservacionText_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateConfirmState();
    }

    private void UpdateConfirmState()
    {
        var requiereDetalle =
            MotivoCombo.SelectedValue is MotivoAnulacionConsumo.Otro;

        ConfirmarButton.IsEnabled =
            MotivoCombo.SelectedValue is MotivoAnulacionConsumo &&
            (!requiereDetalle || !string.IsNullOrWhiteSpace(ObservacionText.Text));
    }
}
