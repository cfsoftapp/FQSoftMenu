using Menu.Enums;

namespace Menu.DTOs;

public class RegistroDiarioInputDto
{
    public int EmpleadoId { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Today;

    public TipoServicioMenu TipoServicio { get; set; } = TipoServicioMenu.Almuerzo;

    public bool RegistraMenu { get; set; } = true;

    public TipoPagoMenu? TipoPagoMenuSuspendido { get; set; }

    public FormaPago? FormaPagoDirectoMenu { get; set; }

    public string? Observacion { get; set; }

    public List<ConsumoAdicionalInputDto> Adicionales { get; set; } = new();

    public int UsuarioRegistroId { get; set; }

    public string UsuarioRegistroNombre { get; set; } = string.Empty;
}