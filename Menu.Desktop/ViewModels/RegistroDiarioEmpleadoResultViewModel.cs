using Menu.Models;

namespace Menu.Desktop.ViewModels;

public sealed class RegistroDiarioEmpleadoResultViewModel
{
    public RegistroDiarioEmpleadoResultViewModel(Empleado empleado)
    {
        Empleado = empleado;
    }

    public Empleado Empleado { get; }

    public string Dni => Empleado.Dni;

    public string NombreCompleto => Empleado.NombreCompleto;

    public string Estado => Empleado.Estado.ToString();

    public string EmpresaCliente => Empleado.EmpresaClienteNombre;

    public string Sucursal => Empleado.SucursalNombre;
}
