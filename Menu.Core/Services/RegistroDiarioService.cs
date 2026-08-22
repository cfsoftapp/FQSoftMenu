using Menu.Data;
using Menu.DTOs;
using Menu.DTOs.RegistroDiario;
using Menu.Enums;
using Menu.Models;
using Microsoft.EntityFrameworkCore;

namespace Menu.Services;

public class RegistroDiarioService
{
    private readonly AppDbContext _context;
    private readonly ConfiguracionMenuService _configuracionMenuService;

    public RegistroDiarioService(
        AppDbContext context,
        ConfiguracionMenuService configuracionMenuService)
    {
        _context = context;
        _configuracionMenuService = configuracionMenuService;
    }

    public async Task<Empleado?> BuscarEmpleadoPorDniAsync(string dni)
    {
        dni = dni.Trim();

        if (string.IsNullOrWhiteSpace(dni))
            return null;

        return await _context.Empleados
            .Include(e => e.EmpresaCliente)
            .Include(e => e.Sucursal)
            .FirstOrDefaultAsync(e => e.Dni == dni && e.Activo);
    }

    public async Task<List<Empleado>> BuscarEmpleadosActivosAsync(string termino, int limite = 8)
    {
        termino = termino.Trim();

        if (string.IsNullOrWhiteSpace(termino))
            return new List<Empleado>();

        var patron = $"%{termino}%";

        return await _context.Empleados
            .AsNoTracking()
            .Include(e => e.EmpresaCliente)
            .Include(e => e.Sucursal)
            .Where(e => e.Activo &&
                        (EF.Functions.Like(e.Dni, patron) ||
                         EF.Functions.Like(e.Nombres, patron) ||
                         EF.Functions.Like(e.Apellidos, patron) ||
                         EF.Functions.Like(e.Nombres + " " + e.Apellidos, patron) ||
                         (e.EmpresaCliente != null && EF.Functions.Like(e.EmpresaCliente.NombreComercial, patron)) ||
                         (e.Sucursal != null && EF.Functions.Like(e.Sucursal.Nombre, patron))))
            .OrderBy(e => e.Dni == termino ? 0 : 1)
            .ThenBy(e => e.Apellidos)
            .ThenBy(e => e.Nombres)
            .Take(limite)
            .ToListAsync();
    }

    public async Task<bool> YaConsumioMenuAsync(int empleadoId, DateTime fecha, TipoServicioMenu tipoServicio)
    {
        var fechaSolo = fecha.Date;

        return await _context.ConsumosMenu
            .AnyAsync(c => c.EmpleadoId == empleadoId &&
                           c.Fecha.Date == fechaSolo &&
                           c.TipoServicio == tipoServicio &&
                           !c.Anulado);
    }

    public async Task<ResultadoOperacionDto> RegistrarAsync(RegistroDiarioInputDto input)
    {
        var empleado = await _context.Empleados
            .Include(e => e.EmpresaCliente)
            .Include(e => e.Sucursal)
            .FirstOrDefaultAsync(e => e.Id == input.EmpleadoId && e.Activo);

        if (empleado is null)
            return ResultadoOperacionDto.Fail("El comensal no existe o está inactivo.");

        var fechaSolo = input.Fecha.Date;

        if (!input.RegistraMenu && input.Adicionales.Count == 0)
            return ResultadoOperacionDto.Fail("Debe registrar menú o al menos un consumo adicional.");

        if (input.Adicionales.Any(x =>
                x.TipoAdicional == TipoAdicional.MenuExtra &&
                x.FormaCobro == FormaCobroAdicional.CreditoComedor))
        {
            return ResultadoOperacionDto.Fail(
                "Los menús extra no pueden quedar como crédito del comensal. Seleccione pago directo o empresa cliente.");
        }

        if (input.Adicionales.Any(x => x.FormaCobro == FormaCobroAdicional.Empresa) &&
            (empleado.EmpresaCliente is null || !empleado.EmpresaCliente.Activo))
        {
            return ResultadoOperacionDto.Fail(
                "No se puede cargar el pedido a empresa cliente porque el comensal no tiene una empresa activa asociada.");
        }

        ConsumoMenu? consumoMenu = null;

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (input.RegistraMenu)
            {
                var yaExiste = await YaConsumioMenuAsync(
                    input.EmpleadoId,
                    fechaSolo,
                    input.TipoServicio);

                if (yaExiste)
                    return ResultadoOperacionDto.Fail(
                        $"El comensal ya tiene registrado {input.TipoServicio} para esta fecha.");

                var config = await _configuracionMenuService.GetActualAsync();

                var tipoPagoMenu = TipoPagoMenu.Empresa;
                FormaPago? formaPagoDirecto = null;
                EstadoCobroAdicional? estadoCobroMenu = null;
                DateTime? fechaPagoMenu = null;

                if (empleado.Estado == EstadoEmpleado.Activo)
                {
                    tipoPagoMenu = TipoPagoMenu.Empresa;
                }
                else
                {
                    if (input.TipoPagoMenuSuspendido is null)
                        return ResultadoOperacionDto.Fail("Debe seleccionar cómo pagará el menú el comensal suspendido.");

                    tipoPagoMenu = input.TipoPagoMenuSuspendido.Value;

                    if (tipoPagoMenu == TipoPagoMenu.Empresa)
                        return ResultadoOperacionDto.Fail("Un comensal suspendido no puede registrar menú con cargo a empresa cliente.");

                    if (tipoPagoMenu == TipoPagoMenu.CreditoComedor)
                    {
                        return ResultadoOperacionDto.Fail(
                            "El menú principal no puede quedar como crédito del comensal. Seleccione descuento por planilla o pago directo.");
                    }

                    if (tipoPagoMenu == TipoPagoMenu.PagoDirecto)
                    {
                        if (input.FormaPagoDirectoMenu is null)
                            return ResultadoOperacionDto.Fail("Debe seleccionar la forma de pago directo del menú.");

                        formaPagoDirecto = input.FormaPagoDirectoMenu.Value;
                        estadoCobroMenu = EstadoCobroAdicional.Pagado;
                        fechaPagoMenu = DateTime.Now;
                    }

                    if (tipoPagoMenu == TipoPagoMenu.DescuentoPlanilla)
                    {
                        estadoCobroMenu = null;
                        fechaPagoMenu = null;
                    }
                }

                consumoMenu = new ConsumoMenu
                {
                    Fecha = fechaSolo,
                    TipoServicio = input.TipoServicio,
                    EmpleadoId = empleado.Id,
                    PrecioMenu = config.PrecioMenu,
                    TipoPagoMenu = tipoPagoMenu,
                    FormaPagoDirecto = formaPagoDirecto,
                    EstadoCobroMenu = estadoCobroMenu,
                    FechaPagoMenu = fechaPagoMenu,
                    Observacion = input.Observacion,
                    UsuarioRegistroId = input.UsuarioRegistroId,
                    UsuarioRegistroNombre = input.UsuarioRegistroNombre,
                    FechaRegistro = DateTime.Now
                };

                _context.ConsumosMenu.Add(consumoMenu);
                await _context.SaveChangesAsync();
            }

            foreach (var adicionalInput in input.Adicionales)
            {
                var adicional = new ConsumoAdicional
                {
                    Fecha = fechaSolo,
                    EmpleadoId = empleado.Id,
                    ConsumoMenuId = consumoMenu?.Id,
                    TipoAdicional = adicionalInput.TipoAdicional,
                    Categoria = adicionalInput.Categoria,
                    Descripcion = adicionalInput.Descripcion.Trim(),
                    Precio = adicionalInput.Precio,
                    FormaCobro = adicionalInput.FormaCobro,
                    EstadoCobro = adicionalInput.FormaCobro == FormaCobroAdicional.CreditoComedor
                        ? EstadoCobroAdicional.Pendiente
                        : EstadoCobroAdicional.Pagado,
                    FechaPago = adicionalInput.FormaCobro == FormaCobroAdicional.CreditoComedor
                        ? null
                        : DateTime.Now,
                    UsuarioRegistroId = input.UsuarioRegistroId,
                    UsuarioRegistroNombre = input.UsuarioRegistroNombre,
                    FechaRegistro = DateTime.Now
                };

                _context.ConsumosAdicionales.Add(adicional);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ResultadoOperacionDto.Ok("Registro guardado correctamente.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ResultadoOperacionDto.Fail($"Error al registrar consumo: {ex.Message}");
        }
    }

    public async Task<List<ConsumoDiaDto>> GetConsumosDelDiaPorEmpleadoAsync(int empleadoId, DateTime fecha)
    {
        var desde = fecha.Date;
        var hasta = desde.AddDays(1);

        var menus = await _context.ConsumosMenu
            .AsNoTracking()
            .Where(x => x.EmpleadoId == empleadoId &&
                        x.Fecha >= desde &&
                        x.Fecha < hasta)
            .OrderBy(x => x.FechaRegistro)
            .Select(x => new ConsumoDiaDto
            {
                Id = x.Id,
                Origen = "MenuPrincipal",
                Tipo = x.TipoServicio.ToString(),
                Descripcion = "Menú principal",
                FormaPago = x.TipoPagoMenu == TipoPagoMenu.PagoDirecto
                    ? x.FormaPagoDirecto!.Value.ToString()
                    : x.TipoPagoMenu.ToString(),
                EstadoCobro = x.EstadoCobroMenu.HasValue
                    ? x.EstadoCobroMenu.Value.ToString()
                    : "-",
                Importe = x.PrecioMenu,
                FechaRegistro = x.FechaRegistro,

                Anulado = x.Anulado,
                MotivoAnulacion = x.MotivoAnulacion,

                PuedeEditar = false,
                PuedeAnular = !x.Anulado
            })
            .ToListAsync();

        var adicionales = await _context.ConsumosAdicionales
            .AsNoTracking()
            .Where(x => x.EmpleadoId == empleadoId &&
                        x.Fecha >= desde &&
                        x.Fecha < hasta)
            .OrderBy(x => x.FechaRegistro)
            .Select(x => new ConsumoDiaDto
            {
                Id = x.Id,
                Origen = "Adicional",
                Tipo = x.TipoAdicional == TipoAdicional.MenuExtra
                    ? "Menú extra"
                    : "Producto adicional",
                Descripcion = string.IsNullOrWhiteSpace(x.Descripcion)
                    ? x.Categoria.ToString()
                    : x.Descripcion,
                FormaPago = x.FormaCobro.ToString(),
                EstadoCobro = x.EstadoCobro.ToString(),
                Importe = x.Precio,
                FechaRegistro = x.FechaRegistro,

                Anulado = x.Anulado,
                MotivoAnulacion = x.MotivoAnulacion,

                PuedeEditar = false,
                PuedeAnular = !x.Anulado
            })
            .ToListAsync();

        return menus
            .Concat(adicionales)
            .OrderByDescending(x => x.FechaRegistro)
            .ToList();
    }

    public async Task<List<RegistroComensalRecienteDto>> GetRegistrosMenuRecientesAsync(
        DateTime fecha,
        int limite = 12)
    {
        var desde = fecha.Date;
        var hasta = desde.AddDays(1);

        return await _context.ConsumosMenu
            .AsNoTracking()
            .Where(x => x.Fecha >= desde && x.Fecha < hasta)
            .OrderByDescending(x => x.FechaRegistro)
            .Take(limite)
            .Select(x => new RegistroComensalRecienteDto
            {
                ConsumoMenuId = x.Id,
                Dni = x.Empleado.Dni,
                NombreCompleto = x.Empleado.Nombres + " " + x.Empleado.Apellidos,
                EmpresaCliente = x.Empleado.EmpresaCliente != null
                    ? x.Empleado.EmpresaCliente.NombreComercial
                    : "Sin empresa",
                Sucursal = x.Empleado.Sucursal != null
                    ? x.Empleado.Sucursal.Nombre
                    : "Sin sucursal",
                TipoServicio = x.TipoServicio.ToString(),
                EstadoComensal = x.Empleado.Estado == EstadoEmpleado.Activo
                    ? "Activo"
                    : "Suspendido",
                FormaCobro = x.TipoPagoMenu == TipoPagoMenu.Empresa
                    ? "Empresa cliente"
                    : x.TipoPagoMenu == TipoPagoMenu.DescuentoPlanilla
                        ? "Descuento planilla"
                        : x.TipoPagoMenu == TipoPagoMenu.CreditoComedor
                            ? "Pendiente del comensal"
                            : x.FormaPagoDirecto.HasValue
                                ? x.FormaPagoDirecto.Value.ToString()
                                : "Pago directo",
                FechaRegistro = x.FechaRegistro,
                Anulado = x.Anulado
            })
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> AnularConsumoDiaAsync(
    string origen,
    int id,
    int usuarioId,
    string usuarioNombre,
    string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            return (false, "Debe ingresar el motivo de anulación.");

        motivo = motivo.Trim();

        if (origen == "MenuPrincipal")
        {
            var consumo = await _context.ConsumosMenu.FindAsync(id);

            if (consumo is null)
                return (false, "No se encontró el consumo de menú.");

            if (consumo.Anulado)
                return (false, "El consumo ya se encuentra anulado.");

            if (consumo.Fecha.Date != DateTime.Today)
                return (false, "Solo se pueden anular consumos del día actual.");

            var estaEnCierreConfirmado = await _context.CierresProveedorDetalle
                .AsNoTracking()
                .AnyAsync(x => x.ConsumoMenuId == consumo.Id &&
                               x.CierreProveedor.Estado == EstadoCierreProveedor.Confirmado);

            if (estaEnCierreConfirmado)
                return (false, "No se puede anular un consumo incluido en una liquidación confirmada.");

            consumo.Anulado = true;
            consumo.FechaAnulacion = DateTime.Now;
            consumo.UsuarioAnulacionId = usuarioId;
            consumo.UsuarioAnulacionNombre = usuarioNombre;
            consumo.MotivoAnulacion = motivo;
        }
        else if (origen == "Adicional")
        {
            var adicional = await _context.ConsumosAdicionales.FindAsync(id);

            if (adicional is null)
                return (false, "No se encontró el consumo adicional.");

            if (adicional.Anulado)
                return (false, "El consumo adicional ya se encuentra anulado.");

            if (adicional.Fecha.Date != DateTime.Today)
                return (false, "Solo se pueden anular consumos del día actual.");

            var estaEnCierreConfirmado = await _context.CierresProveedorDetalle
                .AsNoTracking()
                .AnyAsync(x => x.ConsumoAdicionalId == adicional.Id &&
                               x.CierreProveedor.Estado == EstadoCierreProveedor.Confirmado);

            if (estaEnCierreConfirmado)
                return (false, "No se puede anular un adicional incluido en una liquidación confirmada.");

            adicional.Anulado = true;
            adicional.FechaAnulacion = DateTime.Now;
            adicional.UsuarioAnulacionId = usuarioId;
            adicional.UsuarioAnulacionNombre = usuarioNombre;
            adicional.MotivoAnulacion = motivo;
        }
        else
        {
            return (false, "Tipo de consumo no reconocido.");
        }

        await _context.SaveChangesAsync();

        return (true, "Consumo anulado correctamente.");
    }
}
