using Menu.Data;
using Menu.DTOs;
using Menu.Enums;
using Menu.Models;
using Microsoft.EntityFrameworkCore;

namespace Menu.Services;

public class CuentaPorCobrarService
{
    private readonly AppDbContext _context;

    public CuentaPorCobrarService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CuentaPorCobrarDto>> GetPendientesAsync(
    DateTime fechaInicio,
    DateTime fechaFin,
    int? empleadoId = null)
    {
        var desde = fechaInicio.Date;
        var hasta = fechaFin.Date.AddDays(1);

        var queryMenus = _context.ConsumosMenu
            .Include(x => x.Empleado)
            .Where(x =>
                x.TipoPagoMenu == TipoPagoMenu.CreditoComedor &&
                x.EstadoCobroMenu == EstadoCobroAdicional.Pendiente &&
                x.Fecha >= desde &&
                x.Fecha < hasta)
            .Where(x => !x.Anulado);

        if (empleadoId.HasValue && empleadoId.Value > 0)
            queryMenus = queryMenus.Where(x => x.EmpleadoId == empleadoId.Value);

        var pendientesMenus = await queryMenus
            .Select(x => new CuentaPorCobrarDto
            {
                TipoCuenta = TipoCuentaPorCobrar.MenuPrincipal,
                ConsumoMenuId = x.Id,
                ConsumoAdicionalId = 0,
                Fecha = x.Fecha,
                EmpleadoId = x.EmpleadoId,
                Dni = x.Empleado.Dni,
                EmpleadoNombre = x.Empleado.NombreCompleto,
                Concepto = "Menú principal",
                TipoServicio = x.TipoServicio,
                TipoAdicional = null,
                Categoria = null,
                Descripcion = x.TipoServicio.ToString(),
                Precio = x.PrecioMenu,
                UsuarioRegistroNombre = x.UsuarioRegistroNombre,
                Seleccionado = false
            })
            .ToListAsync();

        var queryAdicionales = _context.ConsumosAdicionales
            .Include(x => x.Empleado)
            .Where(x =>
                x.FormaCobro == FormaCobroAdicional.CreditoComedor &&
                x.EstadoCobro == EstadoCobroAdicional.Pendiente &&
                x.Fecha >= desde &&
                x.Fecha < hasta)
            .Where(x => !x.Anulado);

        if (empleadoId.HasValue && empleadoId.Value > 0)
            queryAdicionales = queryAdicionales.Where(x => x.EmpleadoId == empleadoId.Value);

        var pendientesAdicionales = await queryAdicionales
            .Select(x => new CuentaPorCobrarDto
            {
                TipoCuenta = TipoCuentaPorCobrar.Adicional,
                ConsumoMenuId = 0,
                ConsumoAdicionalId = x.Id,
                Fecha = x.Fecha,
                EmpleadoId = x.EmpleadoId,
                Dni = x.Empleado.Dni,
                EmpleadoNombre = x.Empleado.NombreCompleto,
                Concepto = x.TipoAdicional == TipoAdicional.MenuExtra
                    ? "Menú extra"
                    : "Producto adicional",
                TipoServicio = null,
                TipoAdicional = x.TipoAdicional,
                Categoria = x.Categoria,
                Descripcion = x.Descripcion,
                Precio = x.Precio,
                UsuarioRegistroNombre = x.UsuarioRegistroNombre,
                Seleccionado = false
            })
            .ToListAsync();

        return pendientesMenus
            .Concat(pendientesAdicionales)
            .OrderBy(x => x.EmpleadoNombre)
            .ThenBy(x => x.Fecha)
            .ThenBy(x => x.Concepto)
            .ToList();
    }

    public async Task<ResultadoOperacionDto> RegistrarPagoAsync(PagoCreditoInputDto input)
    {
        if (input.EmpleadoId <= 0)
            return ResultadoOperacionDto.Fail("Debe seleccionar un comensal.");

        if (input.ConsumoMenuIds.Count == 0 && input.ConsumoAdicionalIds.Count == 0)
            return ResultadoOperacionDto.Fail("Debe seleccionar al menos un consumo pendiente.");

        var menusPendientes = await _context.ConsumosMenu
            .Where(x =>
                input.ConsumoMenuIds.Contains(x.Id) &&
                x.EmpleadoId == input.EmpleadoId &&
                x.TipoPagoMenu == TipoPagoMenu.CreditoComedor &&
                x.EstadoCobroMenu == EstadoCobroAdicional.Pendiente &&
                !x.Anulado)
            .ToListAsync();

        var adicionalesPendientes = await _context.ConsumosAdicionales
            .Where(x =>
                input.ConsumoAdicionalIds.Contains(x.Id) &&
                x.EmpleadoId == input.EmpleadoId &&
                x.FormaCobro == FormaCobroAdicional.CreditoComedor &&
                x.EstadoCobro == EstadoCobroAdicional.Pendiente &&
                !x.Anulado)
            .ToListAsync();

        if (menusPendientes.Count != input.ConsumoMenuIds.Count)
            return ResultadoOperacionDto.Fail("Algunos menús seleccionados ya no están pendientes o no pertenecen al comensal.");

        if (adicionalesPendientes.Count != input.ConsumoAdicionalIds.Count)
            return ResultadoOperacionDto.Fail("Algunos adicionales seleccionados ya no están pendientes o no pertenecen al comensal.");

        var totalMenus = menusPendientes.Sum(x => x.PrecioMenu);
        var totalAdicionales = adicionalesPendientes.Sum(x => x.Precio);
        var total = totalMenus + totalAdicionales;

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var pago = new PagoConsumoAdicional
            {
                EmpleadoId = input.EmpleadoId,
                FechaPago = input.FechaPago,
                FormaPago = input.FormaPago,
                MontoPagado = total,
                Observacion = input.Observacion,
                UsuarioRegistroId = input.UsuarioRegistroId,
                UsuarioRegistroNombre = input.UsuarioRegistroNombre,
                FechaRegistro = DateTime.Now
            };

            _context.PagosConsumosAdicionales.Add(pago);
            await _context.SaveChangesAsync();

            foreach (var menu in menusPendientes)
            {
                menu.EstadoCobroMenu = EstadoCobroAdicional.Pagado;
                menu.FechaPagoMenu = input.FechaPago;

                /*
                 * Por ahora no tenemos una tabla detalle para asociar pagos con ConsumoMenu.
                 * El menú queda marcado como pagado directamente en ConsumoMenu.
                 * Más adelante podemos crear PagoConsumoMenuDetalle si quieres trazabilidad exacta.
                 */
            }

            foreach (var consumo in adicionalesPendientes)
            {
                consumo.EstadoCobro = EstadoCobroAdicional.Pagado;
                consumo.FechaPago = input.FechaPago;

                _context.PagosConsumosAdicionalesDetalle.Add(new PagoConsumoAdicionalDetalle
                {
                    PagoConsumoAdicionalId = pago.Id,
                    ConsumoAdicionalId = consumo.Id,
                    MontoAplicado = consumo.Precio
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ResultadoOperacionDto.Ok($"Pago registrado correctamente por S/ {total:N2}.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ResultadoOperacionDto.Fail($"Error al registrar pago: {ex.Message}");
        }
    }
}
