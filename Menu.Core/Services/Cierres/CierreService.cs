using Menu.Data;
using Menu.DTOs;
using Menu.DTOs.Cierres;
using Menu.Enums;
using Menu.Models;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Security;
using System.Text;

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
                    .Sum(x => x.PrecioMenu)
                    +
                    adicionalesActivos
                        .Where(x => x.FormaCobro == FormaCobroAdicional.Empresa)
                        .Sum(x => x.Precio),
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

    public async Task<List<CierreProveedorListadoDto>> ObtenerCierresProveedorAsync()
    {
        return await _context.CierresProveedor
            .AsNoTracking()
            .Where(x => x.Detalles.Any())
            .OrderByDescending(x => x.FechaDesde)
            .ThenByDescending(x => x.Id)
            .Select(x => new CierreProveedorListadoDto
            {
                Id = x.Id,
                FechaDesde = x.FechaDesde,
                FechaHasta = x.FechaHasta,
                Estado = x.Estado,
                TotalMenus = x.TotalMenusActivos + x.TotalMenusPlanilla + x.Detalles.Count(d => d.ConsumoAdicionalId.HasValue),
                TotalLiquidarProveedor = x.TotalLiquidarProveedor,
                TotalExcluidoRevision = x.TotalExcluidoRevision,
                FechaRegistro = x.FechaConfirmacion,
                UsuarioRegistroNombre = x.UsuarioConfirmacionNombre
            })
            .ToListAsync();
    }

    public async Task<CierreProveedorBorradorDto> ObtenerCierreProveedorAsync(int cierreProveedorId)
    {
        var cierre = await _context.CierresProveedor
            .AsNoTracking()
            .Include(x => x.Detalles)
            .FirstOrDefaultAsync(x => x.Id == cierreProveedorId);

        if (cierre is null)
            throw new InvalidOperationException("No se encontro el cierre de facturacion.");

        return MapearBorrador(cierre);
    }

    public async Task<CierreProveedorBorradorDto> GenerarBorradorProveedorAsync(CierreFiltroDto filtro)
    {
        var desde = (filtro.FechaDesde ?? DateTime.Today).Date;
        var hasta = (filtro.FechaHasta ?? DateTime.Today).Date;
        var hastaExclusivo = hasta.AddDays(1);

        var cierreExistente = await _context.CierresProveedor
            .Include(x => x.Detalles)
            .FirstOrDefaultAsync(x => x.FechaDesde == desde && x.FechaHasta == hasta);

        if (cierreExistente is not null)
        {
            if (cierreExistente.Estado == EstadoCierreProveedor.Borrador &&
                cierreExistente.Detalles.Count == 0)
            {
                _context.CierresProveedor.Remove(cierreExistente);
                await _context.SaveChangesAsync();
            }
            else
            {
                return MapearBorrador(cierreExistente);
            }
        }

        if (await ExisteCierreProveedorSolapadoAsync(desde, hasta))
            throw new InvalidOperationException("Ya existe un cierre de facturación con fechas que se cruzan con este rango.");

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
                Concepto = x.TipoServicio.ToString(),
                Importe = x.PrecioMenu
            })
            .ToListAsync();

        var adicionalesEmpresa = await _context.ConsumosAdicionales
            .AsNoTracking()
            .Include(x => x.Empleado)
            .Where(x => !x.Anulado)
            .Where(x => x.Fecha >= desde && x.Fecha < hastaExclusivo)
            .Where(x => x.FormaCobro == FormaCobroAdicional.Empresa)
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.Empleado.Apellidos)
            .ThenBy(x => x.Empleado.Nombres)
            .Select(x => new CierreProveedorItemDto
            {
                ConsumoMenuId = 0,
                ConsumoAdicionalId = x.Id,
                Fecha = x.Fecha,
                EmpleadoId = x.EmpleadoId,
                Dni = x.Empleado.Dni,
                EmpleadoNombre = x.Empleado.NombreCompleto,
                TipoServicio = TipoServicioMenu.Almuerzo,
                TipoPagoMenu = TipoPagoMenu.Empresa,
                TipoAdicional = x.TipoAdicional,
                Concepto = (x.TipoAdicional == TipoAdicional.MenuExtra ? "Menu extra" : "Producto adicional") +
                           (string.IsNullOrWhiteSpace(x.Descripcion) ? string.Empty : $" - {x.Descripcion}"),
                Importe = x.Precio
            })
            .ToListAsync();

        items.AddRange(adicionalesEmpresa);

        if (items.Count == 0)
        {
            return new CierreProveedorBorradorDto
            {
                FechaDesde = desde,
                FechaHasta = hasta,
                Items = items
            };
        }

        return new CierreProveedorBorradorDto
        {
            FechaDesde = desde,
            FechaHasta = hasta,
            Items = items
        };
    }

    public async Task<ResultadoOperacionDto> GuardarBorradorProveedorAsync(ConfirmarCierreProveedorDto input)
    {
        var desde = input.FechaDesde.Date;
        var hasta = input.FechaHasta.Date;

        if (desde > hasta)
            return ResultadoOperacionDto.Fail("La fecha desde no puede ser mayor que la fecha hasta.");

        if (!input.Items.Any())
            return ResultadoOperacionDto.Fail("No hay consumos para guardar en el borrador.");

        var cierre = input.CierreProveedorId.HasValue
            ? await _context.CierresProveedor
                .Include(x => x.Detalles)
                .FirstOrDefaultAsync(x => x.Id == input.CierreProveedorId.Value)
            : await _context.CierresProveedor
                .Include(x => x.Detalles)
                .FirstOrDefaultAsync(x => x.FechaDesde == desde && x.FechaHasta == hasta);

        if (input.CierreProveedorId.HasValue && cierre is null)
            return ResultadoOperacionDto.Fail("El borrador ya no existe. Actualice el historial de cierres.");

        if (cierre is not null &&
            (cierre.FechaDesde.Date != desde || cierre.FechaHasta.Date != hasta))
        {
            return ResultadoOperacionDto.Fail("El periodo del borrador no coincide con el cierre seleccionado.");
        }

        if (cierre?.Estado == EstadoCierreProveedor.Confirmado)
            return ResultadoOperacionDto.Fail("La liquidacion ya fue confirmada y no puede modificarse.");

        if (await ExisteCierreProveedorSolapadoAsync(desde, hasta, cierre?.Id))
            return ResultadoOperacionDto.Fail("Ya existe otro cierre de facturación con fechas que se cruzan con este rango.");

        var validacion = await ValidarItemsBorradorAsync(input, desde, hasta);

        if (!validacion.Success)
            return validacion;

        if (cierre is null)
        {
            cierre = CrearCierreProveedor(
                desde,
                hasta,
                input.Items,
                input.Observacion,
                input.UsuarioConfirmacionId,
                input.UsuarioConfirmacionNombre,
                EstadoCierreProveedor.Borrador);
            _context.CierresProveedor.Add(cierre);
        }
        else
        {
            ActualizarCierreProveedor(
                cierre,
                desde,
                hasta,
                input.Items,
                input.Observacion,
                input.UsuarioConfirmacionId,
                input.UsuarioConfirmacionNombre,
                EstadoCierreProveedor.Borrador);
        }

        await _context.SaveChangesAsync();

        return ResultadoOperacionDto.Ok("Borrador guardado correctamente.");
    }

    public async Task<ResultadoOperacionDto> EliminarBorradorProveedorAsync(int cierreProveedorId)
    {
        var cierre = await _context.CierresProveedor
            .FirstOrDefaultAsync(x => x.Id == cierreProveedorId);

        if (cierre is null)
            return ResultadoOperacionDto.Fail("No se encontro el cierre de facturación.");

        if (cierre.Estado != EstadoCierreProveedor.Borrador)
            return ResultadoOperacionDto.Fail("Solo se pueden eliminar cierres en borrador.");

        _context.CierresProveedor.Remove(cierre);
        await _context.SaveChangesAsync();

        return ResultadoOperacionDto.Ok("Borrador eliminado correctamente.");
    }

    public async Task<byte[]> GenerarExcelProveedorAsync(int cierreProveedorId)
    {
        var cierre = await _context.CierresProveedor
            .AsNoTracking()
            .Include(x => x.Detalles)
            .FirstOrDefaultAsync(x => x.Id == cierreProveedorId);

        if (cierre is null)
            throw new InvalidOperationException("No se encontro el cierre de facturación.");

        return CrearExcelProveedor(cierre);
    }

    private static CierreProveedorBorradorDto MapearBorrador(CierreProveedor cierre)
    {
        return new CierreProveedorBorradorDto
        {
            FechaDesde = cierre.FechaDesde,
            FechaHasta = cierre.FechaHasta,
            YaConfirmado = cierre.Estado == EstadoCierreProveedor.Confirmado,
            CierreProveedorId = cierre.Id,
            Observacion = cierre.Observacion,
            Items = cierre.Detalles
                .OrderBy(x => x.Fecha)
                .ThenBy(x => x.EmpleadoNombre)
                .Select(x => new CierreProveedorItemDto
                {
                    ConsumoMenuId = x.ConsumoMenuId,
                    ConsumoAdicionalId = x.ConsumoAdicionalId,
                    Fecha = x.Fecha,
                    EmpleadoId = x.EmpleadoId,
                    Dni = x.Dni,
                    EmpleadoNombre = x.EmpleadoNombre,
                    TipoServicio = x.TipoServicio,
                    TipoPagoMenu = x.TipoPagoMenu,
                    TipoAdicional = x.TipoAdicional,
                    Concepto = x.Concepto,
                    Importe = x.Importe,
                    ExcluirDeProveedor = x.ExcluidoPorPagoDirecto,
                    MotivoExclusion = x.MotivoExclusion
                })
                .ToList()
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
            return ResultadoOperacionDto.Fail("No hay consumos para facturar.");

        var cierre = input.CierreProveedorId.HasValue
            ? await _context.CierresProveedor
                .Include(x => x.Detalles)
                .FirstOrDefaultAsync(x => x.Id == input.CierreProveedorId.Value)
            : await _context.CierresProveedor
                .Include(x => x.Detalles)
                .FirstOrDefaultAsync(x => x.FechaDesde == desde && x.FechaHasta == hasta);

        if (cierre is null)
            return ResultadoOperacionDto.Fail("No existe borrador para confirmar. Genere y guarde el borrador primero.");

        if (cierre.FechaDesde.Date != desde || cierre.FechaHasta.Date != hasta)
            return ResultadoOperacionDto.Fail("El periodo del borrador no coincide con el cierre seleccionado.");

        if (cierre.Estado == EstadoCierreProveedor.Confirmado)
            return ResultadoOperacionDto.Fail("La liquidacion ya fue confirmada.");

        if (await ExisteCierreProveedorSolapadoAsync(desde, hasta, cierre.Id))
            return ResultadoOperacionDto.Fail("Ya existe otro cierre de facturación con fechas que se cruzan con este rango.");

        var validacion = await ValidarItemsBorradorAsync(input, desde, hasta);

        if (!validacion.Success)
            return validacion;

        ActualizarCierreProveedor(
            cierre,
            desde,
            hasta,
            input.Items,
            input.Observacion,
            input.UsuarioConfirmacionId,
            input.UsuarioConfirmacionNombre,
            EstadoCierreProveedor.Confirmado);

        await _context.SaveChangesAsync();

        return ResultadoOperacionDto.Ok($"Facturación confirmada por S/ {cierre.TotalLiquidarProveedor:N2}.");
    }

    private async Task<ResultadoOperacionDto> ValidarItemsBorradorAsync(
        ConfirmarCierreProveedorDto input,
        DateTime desde,
        DateTime hasta)
    {
        if (input.Items.Any(x =>
                x.EmpleadoId <= 0 ||
                string.IsNullOrWhiteSpace(x.Dni) ||
                string.IsNullOrWhiteSpace(x.EmpleadoNombre) ||
                x.Importe <= 0))
        {
            return ResultadoOperacionDto.Fail("El borrador contiene datos incompletos o importes no validos.");
        }

        var menusInput = input.Items
            .Where(x => !x.EsAdicionalEmpresa)
            .ToList();

        if (menusInput.Any(x => x.ConsumoMenuId <= 0) ||
            menusInput.Select(x => x.ConsumoMenuId).Distinct().Count() != menusInput.Count)
        {
            return ResultadoOperacionDto.Fail("El borrador contiene menus duplicados o sin identificador.");
        }

        var adicionalesInput = input.Items
            .Where(x => x.EsAdicionalEmpresa)
            .ToList();

        if (adicionalesInput.Any(x => x.ConsumoAdicionalId is null or <= 0) ||
            adicionalesInput.Select(x => x.ConsumoAdicionalId!.Value).Distinct().Count() != adicionalesInput.Count)
        {
            return ResultadoOperacionDto.Fail("El borrador contiene adicionales duplicados o sin identificador.");
        }

        var consumoIds = input.Items
            .Where(x => !x.EsAdicionalEmpresa)
            .Select(x => x.ConsumoMenuId)
            .Distinct()
            .ToList();

        var adicionalIds = input.Items
            .Where(x => x.EsAdicionalEmpresa)
            .Select(x => x.ConsumoAdicionalId!.Value)
            .Distinct()
            .ToList();

        var consumos = await _context.ConsumosMenu
            .AsNoTracking()
            .Where(x => consumoIds.Contains(x.Id))
            .ToListAsync();

        if (consumos.Count != consumoIds.Count)
            return ResultadoOperacionDto.Fail("Algunos consumos del borrador ya no existen. Vuelva a generar el borrador.");

        foreach (var consumo in consumos)
        {
            if (consumo.Anulado ||
                consumo.Fecha.Date < desde ||
                consumo.Fecha.Date > hasta ||
                (consumo.TipoPagoMenu != TipoPagoMenu.Empresa &&
                 consumo.TipoPagoMenu != TipoPagoMenu.DescuentoPlanilla))
            {
                return ResultadoOperacionDto.Fail("El borrador cambio. Vuelva a generar la liquidacion antes de guardar.");
            }
        }

        var adicionales = await _context.ConsumosAdicionales
            .AsNoTracking()
            .Where(x => adicionalIds.Contains(x.Id))
            .ToListAsync();

        if (adicionales.Count != adicionalIds.Count)
            return ResultadoOperacionDto.Fail("Algunos adicionales del borrador ya no existen. Vuelva a generar el borrador.");

        foreach (var adicional in adicionales)
        {
            if (adicional.Anulado ||
                adicional.Fecha.Date < desde ||
                adicional.Fecha.Date > hasta ||
                adicional.FormaCobro != FormaCobroAdicional.Empresa)
            {
                return ResultadoOperacionDto.Fail("El borrador cambio. Vuelva a generar la liquidacion antes de guardar.");
            }
        }

        foreach (var item in input.Items.Where(x => x.ExcluirDeProveedor))
        {
            if (string.IsNullOrWhiteSpace(item.MotivoExclusion))
                return ResultadoOperacionDto.Fail("Debe ingresar motivo para cada excepcion.");
        }

        return ResultadoOperacionDto.Ok("OK");
    }

    private async Task<bool> ExisteCierreProveedorSolapadoAsync(DateTime desde, DateTime hasta, int? excluirId = null)
    {
        return await _context.CierresProveedor
            .AsNoTracking()
            .AnyAsync(x =>
                (!excluirId.HasValue || x.Id != excluirId.Value) &&
                x.Detalles.Any() &&
                x.FechaDesde <= hasta &&
                x.FechaHasta >= desde);
    }

    private static CierreProveedor CrearCierreProveedor(
        DateTime desde,
        DateTime hasta,
        List<CierreProveedorItemDto> items,
        string? observacion,
        int usuarioId,
        string usuarioNombre,
        EstadoCierreProveedor estado)
    {
        var cierre = new CierreProveedor
        {
            FechaDesde = desde,
            FechaHasta = hasta
        };

        ActualizarCierreProveedor(cierre, desde, hasta, items, observacion, usuarioId, usuarioNombre, estado);

        return cierre;
    }

    private static void ActualizarCierreProveedor(
        CierreProveedor cierre,
        DateTime desde,
        DateTime hasta,
        List<CierreProveedorItemDto> items,
        string? observacion,
        int usuarioId,
        string usuarioNombre,
        EstadoCierreProveedor estado)
    {
        cierre.FechaDesde = desde;
        cierre.FechaHasta = hasta;
        cierre.Estado = estado;
        cierre.Observacion = observacion?.Trim();
        cierre.UsuarioConfirmacionId = usuarioId;
        cierre.UsuarioConfirmacionNombre = string.IsNullOrWhiteSpace(usuarioNombre)
            ? "Borrador"
            : usuarioNombre.Trim();
        cierre.FechaConfirmacion = DateTime.Now;

        cierre.Detalles.Clear();

        foreach (var item in items.OrderBy(x => x.Fecha).ThenBy(x => x.EmpleadoNombre))
        {
            cierre.Detalles.Add(new CierreProveedorDetalle
            {
                ConsumoMenuId = item.ConsumoMenuId,
                ConsumoAdicionalId = item.ConsumoAdicionalId,
                Fecha = item.Fecha.Date,
                EmpleadoId = item.EmpleadoId,
                Dni = item.Dni,
                EmpleadoNombre = item.EmpleadoNombre,
                TipoServicio = item.TipoServicio,
                TipoPagoMenu = item.TipoPagoMenu,
                TipoAdicional = item.TipoAdicional,
                Concepto = string.IsNullOrWhiteSpace(item.Concepto) ? item.TipoServicio.ToString() : item.Concepto.Trim(),
                Importe = item.Importe,
                IncluidoProveedor = !item.ExcluirDeProveedor,
                ExcluidoPorPagoDirecto = item.ExcluirDeProveedor,
                MotivoExclusion = item.ExcluirDeProveedor ? item.MotivoExclusion?.Trim() : null
            });
        }

        cierre.TotalMenusActivos = cierre.Detalles.Count(x => x.TipoPagoMenu == TipoPagoMenu.Empresa && x.ConsumoAdicionalId is null);
        cierre.TotalMenusPlanilla = cierre.Detalles.Count(x => x.TipoPagoMenu == TipoPagoMenu.DescuentoPlanilla);
        cierre.TotalMenusPlanillaExcluidos = cierre.Detalles.Count(x => x.ExcluidoPorPagoDirecto);
        cierre.TotalPersonalActivo = cierre.Detalles
            .Where(x => x.TipoPagoMenu == TipoPagoMenu.Empresa && x.IncluidoProveedor)
            .Sum(x => x.Importe);
        cierre.TotalPlanilla = cierre.Detalles
            .Where(x => x.TipoPagoMenu == TipoPagoMenu.DescuentoPlanilla && x.IncluidoProveedor)
            .Sum(x => x.Importe);
        cierre.TotalExcluidoRevision = cierre.Detalles
            .Where(x => x.ExcluidoPorPagoDirecto)
            .Sum(x => x.Importe);
        cierre.TotalLiquidarProveedor = cierre.TotalPersonalActivo + cierre.TotalPlanilla;
    }

    private static byte[] CrearExcelProveedor(CierreProveedor cierre)
    {
        using var stream = new MemoryStream();

        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            AddEntry(archive, "[Content_Types].xml", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                  <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>
                  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                  <Override PartName="/xl/worksheets/sheet2.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                  <Override PartName="/xl/worksheets/sheet3.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                </Types>
                """);

            AddEntry(archive, "_rels/.rels", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);

            AddEntry(archive, "xl/workbook.xml", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                  <sheets>
                    <sheet name="Resumen general" sheetId="1" r:id="rId1"/>
                    <sheet name="Resumen empleado" sheetId="2" r:id="rId2"/>
                    <sheet name="Detalle" sheetId="3" r:id="rId3"/>
                  </sheets>
                </workbook>
                """);

            AddEntry(archive, "xl/_rels/workbook.xml.rels", """
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                  <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet2.xml"/>
                  <Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet3.xml"/>
                  <Relationship Id="rId4" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>
                </Relationships>
                """);

            AddEntry(archive, "xl/styles.xml", GetWorkbookStyles());
            AddEntry(archive, "xl/worksheets/sheet1.xml", BuildSheet(
                GetResumenGeneralRows(cierre),
                new SheetBuildOptions(
                    HeaderRow: 8,
                    AutoFilterRange: null,
                    FreezeRows: 0,
                    CurrencyColumns: new[] { 2 },
                    TotalLabelColumn: 1,
                    TotalText: "Total a facturar",
                    ColumnWidths: new double[] { 34, 22 })));
            AddEntry(archive, "xl/worksheets/sheet2.xml", BuildSheet(
                GetResumenEmpleadoRows(cierre),
                new SheetBuildOptions(
                    HeaderRow: 1,
                    AutoFilterRange: $"A1:H{Math.Max(1, cierre.Detalles.Select(x => x.EmpleadoId).Distinct().Count() + 2)}",
                    FreezeRows: 1,
                    CurrencyColumns: new[] { 7, 8 },
                    TotalLabelColumn: 2,
                    TotalText: "TOTAL",
                    ColumnWidths: new double[] { 14, 34, 14, 18, 15, 13, 18, 16 })));
            AddEntry(archive, "xl/worksheets/sheet3.xml", BuildSheet(
                GetDetalleRows(cierre),
                new SheetBuildOptions(
                    HeaderRow: 1,
                    AutoFilterRange: $"A1:I{Math.Max(1, cierre.Detalles.Count + 1)}",
                    FreezeRows: 1,
                    CurrencyColumns: new[] { 9 },
                    TotalLabelColumn: 8,
                    TotalText: "TOTAL",
                    ColumnWidths: new double[] { 14, 14, 34, 28, 22, 18, 24, 32, 15 })));
        }

        return stream.ToArray();
    }

    private static List<object?[]> GetResumenGeneralRows(CierreProveedor cierre)
    {
        return new List<object?[]>
        {
            new object?[] { "Liquidacion de facturacion", string.Empty },
            new object?[] { "Estado", cierre.Estado.ToString() },
            new object?[] { "Fecha desde", cierre.FechaDesde.ToString("dd/MM/yyyy") },
            new object?[] { "Fecha hasta", cierre.FechaHasta.ToString("dd/MM/yyyy") },
            new object?[] { "Generado/confirmado por", cierre.UsuarioConfirmacionNombre },
            new object?[] { "Fecha registro", cierre.FechaConfirmacion.ToString("dd/MM/yyyy HH:mm") },
            new object?[] { string.Empty, string.Empty },
            new object?[] { "Concepto", "Importe (S/)" },
            new object?[] { "Personal activo", cierre.TotalPersonalActivo },
            new object?[] { "Descuento planilla incluido", cierre.TotalPlanilla },
            new object?[] { "Total a facturar", cierre.TotalLiquidarProveedor },
            new object?[] { "Excepciones para revision/concesionario", cierre.TotalExcluidoRevision },
            new object?[] { string.Empty, string.Empty },
            new object?[] { "Cantidad menus activos", cierre.TotalMenusActivos },
            new object?[] { "Cantidad adicionales empresa", cierre.Detalles.Count(x => x.ConsumoAdicionalId.HasValue) },
            new object?[] { "Cantidad planilla", cierre.TotalMenusPlanilla },
            new object?[] { "Cantidad excepciones", cierre.TotalMenusPlanillaExcluidos },
            new object?[] { "Observacion", cierre.Observacion ?? string.Empty }
        };
    }

    private static List<object?[]> GetResumenEmpleadoRows(CierreProveedor cierre)
    {
        var rows = new List<object?[]>
        {
            new object?[]
            {
                "DNI",
                "Comensal",
                "Menus activo",
                "Adicionales empresa cliente",
                "Menus planilla",
                "Excepciones",
                "Total a facturar (S/)",
                "Total revision (S/)"
            }
        };

        rows.AddRange(cierre.Detalles
            .GroupBy(x => new { x.EmpleadoId, x.Dni, x.EmpleadoNombre })
            .OrderBy(x => x.Key.EmpleadoNombre)
            .Select(x => new object?[]
            {
                x.Key.Dni,
                x.Key.EmpleadoNombre,
                x.Count(d => d.TipoPagoMenu == TipoPagoMenu.Empresa && d.ConsumoAdicionalId is null),
                x.Count(d => d.ConsumoAdicionalId.HasValue),
                x.Count(d => d.TipoPagoMenu == TipoPagoMenu.DescuentoPlanilla && d.IncluidoProveedor),
                x.Count(d => d.ExcluidoPorPagoDirecto),
                x.Where(d => d.IncluidoProveedor).Sum(d => d.Importe),
                x.Where(d => d.ExcluidoPorPagoDirecto).Sum(d => d.Importe)
            }));

        rows.Add(new object?[]
        {
            string.Empty,
            "TOTAL",
            cierre.Detalles.Count(x => x.TipoPagoMenu == TipoPagoMenu.Empresa && x.ConsumoAdicionalId is null),
            cierre.Detalles.Count(x => x.ConsumoAdicionalId.HasValue),
            cierre.Detalles.Count(x => x.TipoPagoMenu == TipoPagoMenu.DescuentoPlanilla && x.IncluidoProveedor),
            cierre.Detalles.Count(x => x.ExcluidoPorPagoDirecto),
            cierre.Detalles.Where(x => x.IncluidoProveedor).Sum(x => x.Importe),
            cierre.Detalles.Where(x => x.ExcluidoPorPagoDirecto).Sum(x => x.Importe)
        });

        return rows;
    }

    private static List<object?[]> GetDetalleRows(CierreProveedor cierre)
    {
        var rows = new List<object?[]>
        {
            new object?[]
            {
                "Fecha",
                "DNI",
                "Comensal",
                "Concepto",
                "Tipo pago",
                "Incluido en facturacion",
                "Revision/concesionario",
                "Motivo excepcion",
                "Importe (S/)"
            }
        };

        rows.AddRange(cierre.Detalles
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.EmpleadoNombre)
            .Select(x => new object?[]
            {
                x.Fecha.ToString("dd/MM/yyyy"),
                x.Dni,
                x.EmpleadoNombre,
                string.IsNullOrWhiteSpace(x.Concepto) ? x.TipoServicio.ToString() : x.Concepto,
                x.ConsumoAdicionalId.HasValue
                    ? "Empresa cliente adicional"
                    : x.TipoPagoMenu == TipoPagoMenu.Empresa ? "Personal activo" : "Descuento planilla",
                x.IncluidoProveedor ? "Si" : "No",
                x.ExcluidoPorPagoDirecto ? "Si" : "No",
                x.MotivoExclusion ?? string.Empty,
                x.Importe
            }));

        rows.Add(new object?[]
        {
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            "TOTAL",
            cierre.Detalles.Where(x => x.IncluidoProveedor).Sum(x => x.Importe)
        });

        return rows;
    }

    private static string BuildSheet(List<object?[]> rows, SheetBuildOptions options)
    {
        var builder = new StringBuilder();
        builder.Append("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?><worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">""");

        if (options.FreezeRows > 0)
        {
            var topLeftCell = $"A{options.FreezeRows + 1}";
            builder.Append($"""<sheetViews><sheetView workbookViewId="0"><pane ySplit="{options.FreezeRows}" topLeftCell="{topLeftCell}" activePane="bottomLeft" state="frozen"/><selection pane="bottomLeft" activeCell="{topLeftCell}" sqref="{topLeftCell}"/></sheetView></sheetViews>""");
        }

        if (options.ColumnWidths.Length > 0)
        {
            builder.Append("<cols>");

            for (var colIndex = 0; colIndex < options.ColumnWidths.Length; colIndex++)
            {
                var columnNumber = colIndex + 1;
                var width = Convert.ToString(options.ColumnWidths[colIndex], System.Globalization.CultureInfo.InvariantCulture);
                builder.Append($"""<col min="{columnNumber}" max="{columnNumber}" width="{width}" customWidth="1"/>""");
            }

            builder.Append("</cols>");
        }

        builder.Append("<sheetData>");

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var rowNumber = rowIndex + 1;
            var rowStyle = rowNumber == options.HeaderRow ? 2 : 0;
            var isTitleRow = rowNumber == 1 && options.HeaderRow > 1;
            var isTotalRow = IsTotalRow(rows[rowIndex], options);
            var rowAttributes = rowStyle > 0
                ? $" r=\"{rowNumber}\" s=\"{rowStyle}\" customFormat=\"1\""
                : $" r=\"{rowNumber}\"";

            builder.Append($"""<row{rowAttributes}>""");

            for (var colIndex = 0; colIndex < rows[rowIndex].Length; colIndex++)
            {
                var reference = $"{GetColumnName(colIndex + 1)}{rowNumber}";
                var styleIndex = GetCellStyle(rowNumber, colIndex + 1, rows[rowIndex], options, isTitleRow, isTotalRow);
                builder.Append(BuildCell(reference, rows[rowIndex][colIndex], styleIndex));
            }

            builder.Append("</row>");
        }

        builder.Append("</sheetData>");

        if (!string.IsNullOrWhiteSpace(options.AutoFilterRange))
            builder.Append($"""<autoFilter ref="{options.AutoFilterRange}"/>""");

        builder.Append("</worksheet>");

        return builder.ToString();
    }

    private static string BuildCell(string reference, object? value, int styleIndex)
    {
        var style = styleIndex > 0 ? $" s=\"{styleIndex}\"" : string.Empty;

        if (value is null)
            return $"""<c r="{reference}"{style}/>""";

        if (value is int or decimal)
        {
            var number = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
            return $"""<c r="{reference}"{style}><v>{number}</v></c>""";
        }

        var text = SecurityElement.Escape(value.ToString()) ?? string.Empty;
        return $"""<c r="{reference}"{style} t="inlineStr"><is><t>{text}</t></is></c>""";
    }

    private static bool IsTotalRow(object?[] row, SheetBuildOptions options)
    {
        if (options.TotalLabelColumn <= 0 || options.TotalLabelColumn > row.Length)
            return false;

        return string.Equals(row[options.TotalLabelColumn - 1]?.ToString(), options.TotalText, StringComparison.OrdinalIgnoreCase);
    }

    private static int GetCellStyle(
        int rowNumber,
        int columnNumber,
        object?[] row,
        SheetBuildOptions options,
        bool isTitleRow,
        bool isTotalRow)
    {
        if (isTitleRow)
            return 1;

        if (rowNumber == options.HeaderRow)
            return 2;

        if (isTotalRow)
            return options.CurrencyColumns.Contains(columnNumber) ? 5 : 4;

        return options.CurrencyColumns.Contains(columnNumber) ? 3 : 0;
    }

    private static string GetWorkbookStyles()
    {
        return """
            <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
              <fonts count="3">
                <font><sz val="11"/><name val="Calibri"/></font>
                <font><b/><sz val="14"/><color rgb="FF1F2937"/><name val="Calibri"/></font>
                <font><b/><sz val="11"/><color rgb="FFFFFFFF"/><name val="Calibri"/></font>
              </fonts>
              <fills count="4">
                <fill><patternFill patternType="none"/></fill>
                <fill><patternFill patternType="gray125"/></fill>
                <fill><patternFill patternType="solid"><fgColor rgb="FF1F4E78"/><bgColor indexed="64"/></patternFill></fill>
                <fill><patternFill patternType="solid"><fgColor rgb="FFE2F0D9"/><bgColor indexed="64"/></patternFill></fill>
              </fills>
              <borders count="2">
                <border><left/><right/><top/><bottom/><diagonal/></border>
                <border><left style="thin"><color rgb="FFD9D9D9"/></left><right style="thin"><color rgb="FFD9D9D9"/></right><top style="thin"><color rgb="FFD9D9D9"/></top><bottom style="thin"><color rgb="FFD9D9D9"/></bottom><diagonal/></border>
              </borders>
              <cellStyleXfs count="1">
                <xf numFmtId="0" fontId="0" fillId="0" borderId="0"/>
              </cellStyleXfs>
              <cellXfs count="6">
                <xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/>
                <xf numFmtId="0" fontId="1" fillId="0" borderId="0" xfId="0" applyFont="1"/>
                <xf numFmtId="0" fontId="2" fillId="2" borderId="1" xfId="0" applyFont="1" applyFill="1" applyBorder="1"/>
                <xf numFmtId="4" fontId="0" fillId="0" borderId="0" xfId="0" applyNumberFormat="1"/>
                <xf numFmtId="0" fontId="0" fillId="3" borderId="1" xfId="0" applyFill="1" applyBorder="1"/>
                <xf numFmtId="4" fontId="0" fillId="3" borderId="1" xfId="0" applyNumberFormat="1" applyFill="1" applyBorder="1"/>
              </cellXfs>
              <cellStyles count="1">
                <cellStyle name="Normal" xfId="0" builtinId="0"/>
              </cellStyles>
              <dxfs count="0"/>
              <tableStyles count="0" defaultTableStyle="TableStyleMedium2" defaultPivotStyle="PivotStyleLight16"/>
            </styleSheet>
            """;
    }

    private sealed record SheetBuildOptions(
        int HeaderRow,
        string? AutoFilterRange,
        int FreezeRows,
        int[] CurrencyColumns,
        int TotalLabelColumn,
        string TotalText,
        double[] ColumnWidths);

    private static string GetColumnName(int columnNumber)
    {
        var dividend = columnNumber;
        var columnName = string.Empty;

        while (dividend > 0)
        {
            var modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar('A' + modulo) + columnName;
            dividend = (dividend - modulo) / 26;
        }

        return columnName;
    }

    private static void AddEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Fastest);

        using var entryStream = entry.Open();
        using var writer = new StreamWriter(entryStream, new UTF8Encoding(false));
        writer.Write(content);
    }

    private static List<CierreDetalleDto> ConstruirDetalles(CierreDto cierre)
    {
        return new List<CierreDetalleDto>
        {
            new() { Fecha = cierre.Fecha, Tipo = "Menu", Concepto = "Almuerzos", Cantidad = cierre.TotalAlmuerzos },
            new() { Fecha = cierre.Fecha, Tipo = "Menu", Concepto = "Cenas", Cantidad = cierre.TotalCenas },
            new() { Fecha = cierre.Fecha, Tipo = "Proveedor", Concepto = "Cargo a empresa cliente", Importe = cierre.TotalEmpresa },
            new() { Fecha = cierre.Fecha, Tipo = "Proveedor", Concepto = "Descuento planilla", Importe = cierre.TotalPlanilla },
            new() { Fecha = cierre.Fecha, Tipo = "Caja", Concepto = "Efectivo", Importe = cierre.CobradoEfectivo },
            new() { Fecha = cierre.Fecha, Tipo = "Caja", Concepto = "Yape", Importe = cierre.CobradoYape },
            new() { Fecha = cierre.Fecha, Tipo = "Caja", Concepto = "Plin", Importe = cierre.CobradoPlin },
            new() { Fecha = cierre.Fecha, Tipo = "Credito", Concepto = "Credito pagado", Importe = cierre.TotalCreditoPagado },
            new() { Fecha = cierre.Fecha, Tipo = "Credito", Concepto = "Pendiente del comensal", Importe = cierre.TotalCreditoPendiente },
            new() { Fecha = cierre.Fecha, Tipo = "Control", Concepto = "Adicionales registrados", Cantidad = cierre.TotalAdicionales },
            new() { Fecha = cierre.Fecha, Tipo = "Control", Concepto = "Consumos anulados", Cantidad = cierre.TotalAnulados }
        };
    }
}
