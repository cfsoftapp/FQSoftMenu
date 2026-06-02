using Menu.Data;
using Menu.DTOs.Reportes;
using Menu.Enums;
using Microsoft.EntityFrameworkCore;

namespace Menu.Services.Reportes;

public class ReporteService : IReporteService
{
    private readonly AppDbContext _context;

    public ReporteService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ReporteResumenDto> ObtenerResumenAsync(ReporteFiltroDto filtro)
    {
        var desde = (filtro.FechaDesde ?? DateTime.Today).Date;
        var hasta = (filtro.FechaHasta ?? DateTime.Today).Date.AddDays(1);

        var consumosMenu = await _context.ConsumosMenu
            .AsNoTracking()
            .Where(x => !x.Anulado)
            .Where(x => x.Fecha >= desde && x.Fecha < hasta)
            .ToListAsync();

        var adicionales = await _context.ConsumosAdicionales
            .AsNoTracking()
            .Where(x => !x.Anulado)
            .Where(x => x.Fecha >= desde && x.Fecha < hasta)
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(filtro.DniTrabajador) ||
            !string.IsNullOrWhiteSpace(filtro.NombreTrabajador))
        {
            var texto = (filtro.DniTrabajador ?? filtro.NombreTrabajador ?? string.Empty)
                .Trim()
                .ToLower();

            var empleados = await _context.Empleados
                .AsNoTracking()
                .Where(e =>
                    e.Dni.ToLower().Contains(texto) ||
                    e.Nombres.ToLower().Contains(texto) ||
                    e.Apellidos.ToLower().Contains(texto))
                .Select(e => e.Id)
                .ToListAsync();

            consumosMenu = consumosMenu
                .Where(x => empleados.Contains(x.EmpleadoId))
                .ToList();

            adicionales = adicionales
                .Where(x => empleados.Contains(x.EmpleadoId))
                .ToList();
        }

        var resumen = new ReporteResumenDto
        {
            TotalMenus = consumosMenu.Count,

            TotalEmpresa = consumosMenu
                .Where(x => x.TipoPagoMenu == TipoPagoMenu.Empresa)
                .Sum(x => x.PrecioMenu)
                +
                adicionales
                    .Where(x => x.FormaCobro == FormaCobroAdicional.Empresa)
                    .Sum(x => x.Precio),

            TotalPlanilla = consumosMenu
                .Where(x => x.TipoPagoMenu == TipoPagoMenu.DescuentoPlanilla)
                .Sum(x => x.PrecioMenu),

            TotalMenuPagoDirecto = consumosMenu
                .Where(x => x.TipoPagoMenu == TipoPagoMenu.PagoDirecto)
                .Sum(x => x.PrecioMenu),

            CobradoEfectivo =
                consumosMenu
                    .Where(x => x.TipoPagoMenu == TipoPagoMenu.PagoDirecto &&
                                x.FormaPagoDirecto == FormaPago.Efectivo)
                    .Sum(x => x.PrecioMenu)
                +
                adicionales
                    .Where(x => x.EstadoCobro == EstadoCobroAdicional.Pagado &&
                                x.FormaCobro == FormaCobroAdicional.Efectivo)
                    .Sum(x => x.Precio),

            CobradoYape =
                consumosMenu
                    .Where(x => x.TipoPagoMenu == TipoPagoMenu.PagoDirecto &&
                                x.FormaPagoDirecto == FormaPago.Yape)
                    .Sum(x => x.PrecioMenu)
                +
                adicionales
                    .Where(x => x.EstadoCobro == EstadoCobroAdicional.Pagado &&
                                x.FormaCobro == FormaCobroAdicional.Yape)
                    .Sum(x => x.Precio),

            CobradoPlin =
                consumosMenu
                    .Where(x => x.TipoPagoMenu == TipoPagoMenu.PagoDirecto &&
                                x.FormaPagoDirecto == FormaPago.Plin)
                    .Sum(x => x.PrecioMenu)
                +
                adicionales
                    .Where(x => x.EstadoCobro == EstadoCobroAdicional.Pagado &&
                                x.FormaCobro == FormaCobroAdicional.Plin)
                    .Sum(x => x.Precio),

            PendienteMenuExtra = adicionales
                .Where(x => x.TipoAdicional == TipoAdicional.MenuExtra &&
                            x.EstadoCobro == EstadoCobroAdicional.Pendiente)
                .Sum(x => x.Precio),

            PendienteProducto = adicionales
                .Where(x => x.TipoAdicional == TipoAdicional.Producto &&
                            x.EstadoCobro == EstadoCobroAdicional.Pendiente)
                .Sum(x => x.Precio)
        };

        // Pendientes/pagados de menú principal crédito comedor.
        // Ajustar EstadoCobroMenu si tu enum tiene otro nombre.
        resumen.TotalMenuCreditoPendiente = consumosMenu
            .Where(x => x.TipoPagoMenu == TipoPagoMenu.CreditoComedor &&
                        x.EstadoCobroMenu == EstadoCobroAdicional.Pendiente)
            .Sum(x => x.PrecioMenu);

        resumen.TotalMenuCreditoPagado = consumosMenu
            .Where(x => x.TipoPagoMenu == TipoPagoMenu.CreditoComedor &&
                        x.EstadoCobroMenu == EstadoCobroAdicional.Pagado)
            .Sum(x => x.PrecioMenu);

        resumen.PendienteMenuPrincipal = resumen.TotalMenuCreditoPendiente;

        resumen.CreditoPagado =
            resumen.TotalMenuCreditoPagado
            +
            adicionales
                .Where(x => x.EstadoCobro == EstadoCobroAdicional.Pagado &&
                            x.FormaCobro == FormaCobroAdicional.CreditoComedor)
                .Sum(x => x.Precio);

        resumen.TotalProveedor =
            resumen.TotalEmpresa +
            resumen.TotalPlanilla;

        return resumen;
    }

    public async Task<List<ReporteEmpleadoDto>> ObtenerDetalleEmpleadosAsync(ReporteFiltroDto filtro)
    {
        var desde = (filtro.FechaDesde ?? DateTime.Today).Date;
        var hasta = (filtro.FechaHasta ?? DateTime.Today).Date.AddDays(1);

        var consumosMenu = await _context.ConsumosMenu
            .AsNoTracking()
            .Include(x => x.Empleado)
            .Where(x => !x.Anulado)
            .Where(x => x.Fecha >= desde && x.Fecha < hasta)
            .ToListAsync();

        var adicionales = await _context.ConsumosAdicionales
            .AsNoTracking()
            .Include(x => x.Empleado)
            .Where(x => !x.Anulado)
            .Where(x => x.Fecha >= desde && x.Fecha < hasta)
            .ToListAsync();

        if (!string.IsNullOrWhiteSpace(filtro.DniTrabajador) ||
            !string.IsNullOrWhiteSpace(filtro.NombreTrabajador))
        {
            var texto = (filtro.DniTrabajador ?? filtro.NombreTrabajador ?? string.Empty)
                .Trim()
                .ToLower();

            consumosMenu = consumosMenu
                .Where(x =>
                    x.Empleado.Dni.ToLower().Contains(texto) ||
                    x.Empleado.Nombres.ToLower().Contains(texto) ||
                    x.Empleado.Apellidos.ToLower().Contains(texto))
                .ToList();

            adicionales = adicionales
                .Where(x =>
                    x.Empleado.Dni.ToLower().Contains(texto) ||
                    x.Empleado.Nombres.ToLower().Contains(texto) ||
                    x.Empleado.Apellidos.ToLower().Contains(texto))
                .ToList();
        }

        var empleadoIds = consumosMenu
            .Select(x => x.EmpleadoId)
            .Union(adicionales.Select(x => x.EmpleadoId))
            .Distinct()
            .ToList();

        var resultado = new List<ReporteEmpleadoDto>();

        foreach (var empleadoId in empleadoIds)
        {
            var menusEmpleado = consumosMenu
                .Where(x => x.EmpleadoId == empleadoId)
                .ToList();

            var adicionalesEmpleado = adicionales
                .Where(x => x.EmpleadoId == empleadoId)
                .ToList();

            var empleado = menusEmpleado.FirstOrDefault()?.Empleado
                ?? adicionalesEmpleado.FirstOrDefault()?.Empleado;

            if (empleado is null)
                continue;

            var dto = new ReporteEmpleadoDto
            {
                TrabajadorId = empleado.Id,
                Dni = empleado.Dni,
                Trabajador = empleado.NombreCompleto,

                Categoria = empleado.Categoria.ToString(),
                Estado = empleado.Estado.ToString(),

                TotalMenus = menusEmpleado.Count,

                TotalAlmuerzos = menusEmpleado
                    .Count(x => x.TipoServicio == TipoServicioMenu.Almuerzo),

                TotalCenas = menusEmpleado
                    .Count(x => x.TipoServicio == TipoServicioMenu.Cena),

                TotalEmpresa = menusEmpleado
                    .Where(x => x.TipoPagoMenu == TipoPagoMenu.Empresa)
                    .Sum(x => x.PrecioMenu)
                    +
                    adicionalesEmpleado
                        .Where(x => x.FormaCobro == FormaCobroAdicional.Empresa)
                        .Sum(x => x.Precio),

                TotalPlanilla = menusEmpleado
                    .Where(x => x.TipoPagoMenu == TipoPagoMenu.DescuentoPlanilla)
                    .Sum(x => x.PrecioMenu),

                TotalPagoDirecto = menusEmpleado
                    .Where(x => x.TipoPagoMenu == TipoPagoMenu.PagoDirecto)
                    .Sum(x => x.PrecioMenu),

                TotalCreditoPendiente = menusEmpleado
                    .Where(x => x.TipoPagoMenu == TipoPagoMenu.CreditoComedor &&
                                x.EstadoCobroMenu == EstadoCobroAdicional.Pendiente)
                    .Sum(x => x.PrecioMenu)
                    +
                    adicionalesEmpleado
                        .Where(x => x.EstadoCobro == EstadoCobroAdicional.Pendiente)
                        .Sum(x => x.Precio),

                TotalCreditoPagado = menusEmpleado
                        .Where(x => x.TipoPagoMenu == TipoPagoMenu.CreditoComedor &&
                                    x.EstadoCobroMenu == EstadoCobroAdicional.Pagado)
                        .Sum(x => x.PrecioMenu)
                        +
                    adicionalesEmpleado
                        .Where(x => x.EstadoCobro == EstadoCobroAdicional.Pagado &&
                                    x.FormaCobro == FormaCobroAdicional.CreditoComedor)
                        .Sum(x => x.Precio),

                TotalMenuExtra = adicionalesEmpleado
                    .Where(x => x.TipoAdicional == TipoAdicional.MenuExtra)
                    .Sum(x => x.Precio),

                TotalProductos = adicionalesEmpleado
                    .Where(x => x.TipoAdicional == TipoAdicional.Producto)
                    .Sum(x => x.Precio),

                TotalExtrasProductos = adicionalesEmpleado
                    .Sum(x => x.Precio)
            };

            dto.TotalCobrado =
                dto.TotalPagoDirecto +
                dto.TotalCreditoPagado +
                adicionalesEmpleado
                    .Where(x => x.EstadoCobro == EstadoCobroAdicional.Pagado &&
                                (x.FormaCobro == FormaCobroAdicional.Efectivo ||
                                 x.FormaCobro == FormaCobroAdicional.Yape ||
                                 x.FormaCobro == FormaCobroAdicional.Plin))
                    .Sum(x => x.Precio);

            dto.TotalPendiente = dto.TotalCreditoPendiente;

            dto.TotalGeneral =
                dto.TotalEmpresa +
                dto.TotalPlanilla +
                dto.TotalCobrado +
                dto.TotalPendiente;

            dto.Detalles = ConstruirDetallesPorFecha(menusEmpleado, adicionalesEmpleado);

            resultado.Add(dto);
        }

        return resultado
            .OrderBy(x => x.Trabajador)
            .ToList();
    }

    private static List<ReporteEmpleadoFechaDto> ConstruirDetallesPorFecha(
        List<Menu.Models.ConsumoMenu> menusEmpleado,
        List<Menu.Models.ConsumoAdicional> adicionalesEmpleado)
    {
        menusEmpleado = menusEmpleado
           .Where(x => !x.Anulado)
           .ToList();

        adicionalesEmpleado = adicionalesEmpleado
            .Where(x => !x.Anulado)
            .ToList();

        var fechas = menusEmpleado
            .Select(x => x.Fecha.Date)
            .Union(adicionalesEmpleado.Select(x => x.Fecha.Date))
            .Distinct()
            .OrderByDescending(x => x)
            .ToList();

        var detalles = new List<ReporteEmpleadoFechaDto>();

        foreach (var fecha in fechas)
        {
            var menusFecha = menusEmpleado
                .Where(x => x.Fecha.Date == fecha)
                .ToList();

            var adicionalesFecha = adicionalesEmpleado
                .Where(x => x.Fecha.Date == fecha)
                .ToList();

            var menuPrincipal = menusFecha.FirstOrDefault();

            var detalle = new ReporteEmpleadoFechaDto
            {
                FechaConsumo = fecha,

                ConsumioMenuPrincipal = menuPrincipal is not null,

                EstadoTrabajador = menuPrincipal?.Empleado?.Estado.ToString() ?? string.Empty,

                TipoPagoMenuPrincipal = menuPrincipal?.TipoPagoMenu.ToString() ?? string.Empty,

                MedioPago = menuPrincipal?.FormaPagoDirecto?.ToString() ?? string.Empty,

                ImporteEmpresa = menusFecha
                    .Where(x => x.TipoPagoMenu == TipoPagoMenu.Empresa)
                    .Sum(x => x.PrecioMenu)
                    +
                    adicionalesFecha
                        .Where(x => x.FormaCobro == FormaCobroAdicional.Empresa)
                        .Sum(x => x.Precio),

                ImportePlanilla = menusFecha
                    .Where(x => x.TipoPagoMenu == TipoPagoMenu.DescuentoPlanilla)
                    .Sum(x => x.PrecioMenu),

                ImportePagoDirecto = menusFecha
                    .Where(x => x.TipoPagoMenu == TipoPagoMenu.PagoDirecto)
                    .Sum(x => x.PrecioMenu),

                ImporteCreditoPendiente = menusFecha
                    .Where(x => x.TipoPagoMenu == TipoPagoMenu.CreditoComedor &&
                                x.EstadoCobroMenu == EstadoCobroAdicional.Pendiente)
                    .Sum(x => x.PrecioMenu),

                ImporteCreditoPagado = menusFecha
                    .Where(x => x.TipoPagoMenu == TipoPagoMenu.CreditoComedor &&
                                x.EstadoCobroMenu == EstadoCobroAdicional.Pagado)
                    .Sum(x => x.PrecioMenu),

                ImporteMenuExtra = adicionalesFecha
                    .Where(x => x.TipoAdicional == TipoAdicional.MenuExtra)
                    .Sum(x => x.Precio),

                ImporteProductos = adicionalesFecha
                    .Where(x => x.TipoAdicional == TipoAdicional.Producto)
                    .Sum(x => x.Precio),

                Adicionales = adicionalesFecha
                    .OrderBy(x => x.TipoAdicional)
                    .ThenBy(x => x.Categoria)
                    .ThenBy(x => x.Descripcion)
                    .Select(x => new ReporteAdicionalDetalleDto
                    {
                        Tipo = x.TipoAdicional == TipoAdicional.MenuExtra
                            ? "Menú extra"
                            : "Producto adicional",

                        Categoria = x.Categoria.ToString(),
                        Descripcion = x.Descripcion,
                        FormaCobro = x.FormaCobro.ToString(),
                        EstadoCobro = x.EstadoCobro.ToString(),
                        Importe = x.Precio
                    })
                    .ToList()
            };

            detalle.TotalCobrado =
                detalle.ImportePagoDirecto +
                detalle.ImporteCreditoPagado +
                adicionalesFecha
                    .Where(x => x.EstadoCobro == EstadoCobroAdicional.Pagado &&
                                (x.FormaCobro == FormaCobroAdicional.Efectivo ||
                                 x.FormaCobro == FormaCobroAdicional.Yape ||
                                 x.FormaCobro == FormaCobroAdicional.Plin))
                    .Sum(x => x.Precio);

            detalle.TotalPendiente =
                detalle.ImporteCreditoPendiente +
                adicionalesFecha
                    .Where(x => x.EstadoCobro == EstadoCobroAdicional.Pendiente)
                    .Sum(x => x.Precio);

            detalles.Add(detalle);
        }

        return detalles;
    }
}
