using Menu.Models;

namespace Menu.Desktop.ViewModels;

public sealed class EmpleadoRowViewModel
{
    public EmpleadoRowViewModel(Empleado empleado)
    {
        Empleado = empleado;
        Id = empleado.Id;
        Dni = empleado.Dni;
        NombreCompleto = empleado.NombreCompleto;
        EmpresaCliente = empleado.EmpresaClienteNombre;
        Sucursal = empleado.SucursalNombre;
        Categoria = empleado.TipoEmpleadoNombre;
        Estado = empleado.Estado.ToString();
        EstadoBrush = Estado == "Suspendido" ? "#FF9800" : "#00C853";
        Activo = empleado.Activo ? "Si" : "No";
        ActivoBrush = empleado.Activo ? "#00C853" : "#FF3D3D";
        FechaCreacion = empleado.FechaCreacion.ToString("dd/MM/yyyy");
    }

    public Empleado Empleado { get; }

    public int Id { get; }

    public string Dni { get; }

    public string NombreCompleto { get; }

    public string EmpresaCliente { get; }

    public string Sucursal { get; }

    public string Categoria { get; }

    public string Estado { get; }

    public string EstadoBrush { get; }

    public string Activo { get; }

    public string ActivoBrush { get; }

    public string FechaCreacion { get; }
}
