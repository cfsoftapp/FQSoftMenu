using Menu.Data;
using Menu.DTOs;
using Menu.DTOs.Cierres;
using Menu.Enums;
using Menu.Models;
using Microsoft.EntityFrameworkCore;

namespace Menu.Services.Cierres;

public class CierreService : ICierreService
{
    private readonly AppDbContext _context;

    public CierreService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CierreResumenDto> ObtenerResumenAsync(CierreFiltroDto filtro)
    {
        var cierres = await ObtenerCierresAsync(filtro);
        var desde = (filtro.FechaDesde ?? DateTime.Today).Date;
        var hasta = (filtro.FechaHasta ?? DateTime.Today).Date;

        return new CierreResumenDto
        {
            FechaDesde = desde,
            FechaHasta = hasta,
            DiasConMovimiento = cierres.Count,
            TotalMenus = cierres.Sum(x => x.TotalMenus),
            TotalAlmuerzos = cierres.Sum(x => x.TotalAlmuerzos),
            TotalCenas = cierres.Sum(x => x.TotalCenas),
            TotalAdicionales = cierres.Sum(x => x.TotalAdicionales),
            TotalAnulados = cierres.Sum(x => x.TotalAnulados),
            TotalEmpresa = cierres.Sum(x => x.TotalEmpresa),
            TotalPlanilla = cierres.Sum(x => x.TotalPlanilla),
            TotalPagoDirecto = cierres.Sum(x => x.TotalPagoDirecto),
            TotalCreditoPendiente = cierres.Sum(x => x.TotalCreditoPendiente),
            TotalCreditoPagado = cierres.Sum(x => x.TotalCreditoPagado),
            CobradoEfectivo = cierres.Sum(x => x.CobradoEfectivo),
            CobradoYape = cierres.Sum(x => x.CobradoYape),
            CobradoPlin = cierres.Sum(x => x.CobradoPlin)
        };
    }

    public async Task<List<CierreDto>> ObtenerCierresAsync(CierreFiltroDto filtro)
    {
        var desde = (filtro.FechaDesde ?? DateTime.Today).Date;
        var hasta = (filtro.FechaHasta ?? DateTime.Today).Date.AddDays(1);

        var menus = await _context.ConsumosMenu
            .AsNoTracking()
            .Where(x => x.Fecha >= desde && x.Fecha < hasta)
            .ToListAsync();

        var adicionales = await _context.ConsumosAdicionales
            .AsNoTracking()
            .Where(x => x.Fecha >= desde && x.Fecha < hasta)
            .ToListAsync();

        var fechas = menus
            .Select(x => x.Fecha.Date)
            .Union(adicionales.Select(x => x.Fecha.Date))
            .Distinct()
            .OrderByDescending(x => x)
            .ToList();

        var resultado = new List<CierreDto>();

        foreach (var fecha in fechas)
        {
            var menusFecha = menus.Where(x => x.Fecha.Date == fecha).ToList();
            var adicionalesFecha = adicionales.Where(x => x.Fecha.Date == fecha).ToList();

            var menusActivos = menusFecha.Where(x => !x.Anulado).ToList();
            var adicionalesActivos = adicionalesFecha.Where(x => !x.Anulado).ToList();

            var cierre = new CierreDto
            {
                Fecha = fecha,
                TotalMenus = menusActivos.Count,
                TotalAlmuerzos = menusActivos.Count(x => x.TipoServicio == TipoServicioMenu.Almuerzo),
                TotalCenas = menusActivos.Count(x => x.TipoServicio == TipoServicioMenu.Cena),
                TotalAdicionales = adicionalesActivos.Count,
                TotalAnulados = menusFecha.Count(x => x.Anulado) + adicionalesFecha.Count(x => x.Anulado),
                TotalEmpresa = menusActivos
                    .Where(x => x.TipoPagoMenu == TipoPagoMenu.Empresa)
                    .Sum(x => x.PrecioMenu),
                TotalPlanilla = menusActivos
                    .Where(x => x.TipoPagoMenu == TipoPagoMenu.DescuentoPlanilla)
                    .Sum(x => x.PrecioMenu),
                TotalPagoDirecto = menusActivos
                    .Where(x => x.TipoPagoMenu == TipoPagoMenu.PagoDirecto)
                    .Sum(x => x.PrecioMenu),
                TotalCreditoPendiente =
                    menusActivos
                        .Where(x => x.TipoPagoMenu == TipoPagoMenu.CreditoComedor &&
                                    x.EstadoCobroMenu == EstadoCobroAdicional.Pendiente)
                        .Sum(x => x.PrecioMenu)
                    +
                    adicionalesActivos
                        .Where(x => x.FormaCobro == FormaCobroAdicional.CreditoComedor &&
                                    x.EstadoCobro == EstadoCobroAdicional.Pendiente)
                        .Sum(x => x.Precio),
                TotalCreditoPagado =
                    menusActivos
                        .Where(x => x.TipoPagoMenu == TipoPagoMenu.CreditoComedor &&
                                    x.EstadoCobroMenu == EstadoCobroAdicional.Pagado)
                        .Sum(x => x.PrecioMenu)
                    +
                    adicionalesActivos
                        .Where(x => x.FormaCobro == FormaCobroAdicional.CreditoComedor &&
                                    x.EstadoCobro == EstadoCobroAdicional.Pagado)
                        .Sum(x => x.Precio),
                CobradoEfectivo =
                    menusActivos
                        .Where(x => x.TipoPagoMenu == TipoPagoMenu.PagoDirecto &&
                                    x.FormaPagoDirecto == FormaPago.Efectivo)
                        .Sum(x => x.PrecioMenu)
                    +
                    adicionalesActivos
                        .Where(x => x.FormaCobro == FormaCobroAdicional.Efectivo &&
                                    x.EstadoCobro == EstadoCobroAdicional.Pagado)
                        .Sum(x => x.Precio),
                CobradoYape =
                    menusActivos
                        .Where(x => x.TipoPagoMenu == TipoPagoMenu.PagoDirecto &&
                                    x.FormaPagoDirecto == FormaPago.Yape)
                        .Sum(x => x.PrecioMenu)
                    +
                    adicionalesActivos
                        .Where(x => x.FormaCobro == FormaCobroAdicional.Yape &&
                                    x.EstadoCobro == EstadoCobroAdicional.Pagado)
                        .Sum(x => x.Precio),
                CobradoPlin =
                    menusActivos
                        .Where(x => x.TipoPagoMenu == TipoPagoMenu.PagoDirecto &&
                                    x.FormaPagoDirecto == FormaPago.Plin)
                        .Sum(x => x.PrecioMenu)
                    +
                    adicionalesActivos
                        .Where(x => x.FormaCobro == FormaCobroAdicional.Plin &&
                                    x.EstadoCobro == EstadoCobroAdicional.Pagado)
                        .Sum(x => x.Precio)
            };

            cierre.Detalles = ConstruirDetalles(cierre);
            resultado.Add(cierre);
        }

        return resultado;
    }

    public async Task<CierreProveedorBorradorDto> GenerarBorradorProveedorAsync(CierreFiltroDto filtro)
    {
        var desde = (filtro.FechaDesde ?? DateTime.Today).Date;
        var hasta = (filtro.FechaHasta ?? DateTime.Today).Date;
        var hastaExclusivo = hasta.AddDays(1);

        var cierreConfirmado = await _context.CierresProveedor
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FechaDesde == desde && x.FechaHasta == hasta);

        var items = await _context.ConsumosMenu
            .AsNoTracking()
            .Include(x => x.Empleado)
            .Where(x => !x.Anulado)
            .Where(x => x.Fecha >= desde && x.Fecha < hastaExclusivo)
            .Where(x => x.TipoPagoMenu == TipoPagoMenu.Empresa ||
                        x.TipoPagoMenu == TipoPagoMenu.DescuentoPlanilla)
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.Empleado.Apellidos)
            .ThenBy(x => x.Empleado.Nombres)
            .Select(x => new CierreProveedorItemDto
            {
                ConsumoMenuId = x.Id,
                Fecha = x.Fecha,
                EmpleadoId = x.EmpleadoId,
                Dni = x.Empleado.Dni,
                EmpleadoNombre = x.Empleado.NombreCompleto,
                TipoServicio = x.TipoServicio,
                TipoPagoMenu = x.TipoPagoMenu,
                Importe = x.PrecioMenu
            })
            .ToListAsync();

        return new CierreProveedorBorradorDto
        {
            FechaDesde = desde,
            FechaHasta = hasta,
            YaConfirmado = cierreConfirmado is not null,
            CierreConfirmadoId = cierreConfirmado?.Id,
            Items = items
        };
    }

    public async Task<ResultadoOperacionDto> ConfirmarLiquidacionProveedorAsync(ConfirmarCierreProveedorDto input)
    {
        var desde = input.FechaDesde.Date;
        var hasta = input.FechaHasta.Date;

        if (desde > hasta)
            return ResultadoOperacionDto.Fail("La fecha desde no puede ser mayor que la fecha hasta.");

        if (input.UsuarioConfirmacionId <= 0 || string.IsNullOrWhiteSpace(input.UsuarioConfirmacionNombre))
            return ResultadoOperacionDto.Fail("Debe iniciar sesion para confirmar el cierre.");

        if (!input.Items.Any())
            return ResultadoOperacionDto.Fail("No hay consumos para liquidar con proveedor.");

        var yaExiste = await _context.CierresProveedor
            .AnyAsync(x => x.FechaDesde == desde && x.FechaHasta == hasta);

        if (yaExiste)
            return ResultadoOperacionDto.Fail("Ya existe una liquidacion confirmada para ese rango de fechas.");

        var consumoIds = input.Items.Select(x => x.ConsumoMenuId).Distinct().ToList();

        var consumos = await _context.ConsumosMenu
            .AsNoTracking()
            .Include(x => x.Empleado)
            .Where(x => consumoIds.Contains(x.Id))
            .ToListAsync();

        if (consumos.Count != consumoIds.Count)
            return ResultadoOperacionDto.Fail("Algunos consumos del borrador ya no existen. Vuelva a generar el borrador.");

        var excluidos = input.Items
            .Where(x => x.ExcluirDeProveedor)
            .ToDictionary(x => x.ConsumoMenuId, x => x.MotivoExclusion?.Trim());

        var detalles = new List<CierreProveedorDetalle>();

        foreach (var consumo in consumos.OrderBy(x => x.Fecha).ThenBy(x => x.Empleado.Apellidos).ThenBy(x => x.Empleado.Nombres))
        {
            if (consumo.Anulado ||
                consumo.Fecha.Date < desde ||
                consumo.Fecha.Date > hasta ||
                (consumo.TipoPagoMenu != TipoPagoMenu.Empresa &&
                 consumo.TipoPagoMenu != TipoPagoMenu.DescuentoPlanilla))
            {
                return ResultadoOperacionDto.Fail("El borrador cambio. Vuelva a generar la liquidacion antes de confirmar.");
            }

            var excluido = excluidos.ContainsKey(consumo.Id);

            if (excluido && consumo.TipoPagoMenu != TipoPagoMenu.DescuentoPlanilla)
                return ResultadoOperacionDto.Fail("Solo los consumos con descuento planilla pueden excluirse de la liquidacion.");

            if (excluido && string.IsNullOrWhiteSpace(excluidos[consumo.Id]))
                return ResultadoOperacionDto.Fail("Debe ingresar motivo para cada excepcion de planilla.");

            detalles.Add(new CierreProveedorDetalle
            {
                ConsumoMenuId = consumo.Id,
                Fecha = consumo.Fecha.Date,
                EmpleadoId = consumo.EmpleadoId,
                Dni = consumo.Empleado.Dni,
                EmpleadoNombre = consumo.Empleado.NombreCompleto,
                TipoServicio = consumo.TipoServicio,
                TipoPagoMenu = consumo.TipoPagoMenu,
                Importe = consumo.PrecioMenu,
                IncluidoProveedor = !excluido,
                ExcluidoPorPagoDirecto = excluido,
                MotivoExclusion = excluido ? excluidos[consumo.Id] : null
            });
        }

        var cierre = new CierreProveedor
        {
            FechaDesde = desde,
            FechaHasta = hasta,
            TotalMenusActivos = detalles.Count(x => x.TipoPagoMenu == TipoPagoMenu.Empresa),
            TotalMenusPlanilla = detalles.Count(x => x.TipoPagoMenu == TipoPagoMenu.DescuentoPlanilla),
            TotalMenusPlanillaExcluidos = detalles.Count(x => x.ExcluidoPorPagoDirecto),
            TotalPersonalActivo = detalles
                .Where(x => x.TipoPagoMenu == TipoPagoMenu.Empresa)
                .Sum(x => x.Importe),
            TotalPlanilla = detalles
                .Where(x => x.TipoPagoMenu == TipoPagoMenu.DescuentoPlanilla && x.IncluidoProveedor)
                .Sum(x => x.Importe),
            TotalExcluidoRevision = detalles
                .Where(x => x.ExcluidoPorPagoDirecto)
                .Sum(x => x.Importe),
            Observacion = input.Observacion?.Trim(),
            UsuarioConfirmacionId = input.UsuarioConfirmacionId,
            UsuarioConfirmacionNombre = input.UsuarioConfirmacionNombre.Trim(),
            FechaConfirmacion = DateTime.Now,
            Detalles = detalles
        };

        cierre.TotalLiquidarProveedor = cierre.TotalPersonalActivo + cierre.TotalPlanilla;

        _context.CierresProveedor.Add(cierre);
        await _context.SaveChangesAsync();

        return ResultadoOperacionDto.Ok($"Liquidacion confirmada por S/ {cierre.TotalLiquidarProveedor:N2}.");
    }

    private static List<CierreDetalleDto> ConstruirDetalles(CierreDto cierre)
    {
        return new List<CierreDetalleDto>
        {
            new() { Fecha = cierre.Fecha, Tipo = "Menu", Concepto = "Almuerzos", Cantidad = cierre.TotalAlmuerzos },
            new() { Fecha = cierre.Fecha, Tipo = "Menu", Concepto = "Cenas", Cantidad = cierre.TotalCenas },
            new() { Fecha = cierre.Fecha, Tipo = "Proveedor", Concepto = "Pago empresa", Importe = cierre.TotalEmpresa },
            new() { Fecha = cierre.Fecha, Tipo = "Proveedor", Concepto = "Descuento planilla", Importe = cierre.TotalPlanilla },
            new() { Fecha = cierre.Fecha, Tipo = "Caja", Concepto = "Efectivo", Importe = cierre.CobradoEfectivo },
            new() { Fecha = cierre.Fecha, Tipo = "Caja", Concepto = "Yape", Importe = cierre.CobradoYape },
            new() { Fecha = cierre.Fecha, Tipo = "Caja", Concepto = "Plin", Importe = cierre.CobradoPlin },
            new() { Fecha = cierre.Fecha, Tipo = "Credito", Concepto = "Credito pagado", Importe = cierre.TotalCreditoPagado },
            new() { Fecha = cierre.Fecha, Tipo = "Credito", Concepto = "Credito pendiente", Importe = cierre.TotalCreditoPendiente },
            new() { Fecha = cierre.Fecha, Tipo = "Control", Concepto = "Adicionales registrados", Cantidad = cierre.TotalAdicionales },
            new() { Fecha = cierre.Fecha, Tipo = "Control", Concepto = "Consumos anulados", Cantidad = cierre.TotalAnulados }
        };
    }
}
