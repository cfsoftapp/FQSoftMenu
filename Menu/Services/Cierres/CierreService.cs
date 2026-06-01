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

    public async Task<List<CierreProveedorListadoDto>> ObtenerCierresProveedorAsync()
    {
        return await _context.CierresProveedor
            .AsNoTracking()
            .OrderByDescending(x => x.FechaDesde)
            .ThenByDescending(x => x.Id)
            .Select(x => new CierreProveedorListadoDto
            {
                Id = x.Id,
                FechaDesde = x.FechaDesde,
                FechaHasta = x.FechaHasta,
                Estado = x.Estado,
                TotalMenus = x.TotalMenusActivos + x.TotalMenusPlanilla,
                TotalLiquidarProveedor = x.TotalLiquidarProveedor,
                TotalExcluidoRevision = x.TotalExcluidoRevision,
                FechaRegistro = x.FechaConfirmacion,
                UsuarioRegistroNombre = x.UsuarioConfirmacionNombre
            })
            .ToListAsync();
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
            return MapearBorrador(cierreExistente);

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

        var cierre = CrearCierreProveedor(
            desde,
            hasta,
            items,
            observacion: null,
            usuarioId: 0,
            usuarioNombre: "Borrador",
            estado: EstadoCierreProveedor.Borrador);

        _context.CierresProveedor.Add(cierre);
        await _context.SaveChangesAsync();

        return MapearBorrador(cierre);
    }

    public async Task<ResultadoOperacionDto> GuardarBorradorProveedorAsync(ConfirmarCierreProveedorDto input)
    {
        var desde = input.FechaDesde.Date;
        var hasta = input.FechaHasta.Date;

        if (desde > hasta)
            return ResultadoOperacionDto.Fail("La fecha desde no puede ser mayor que la fecha hasta.");

        if (!input.Items.Any())
            return ResultadoOperacionDto.Fail("No hay consumos para guardar en el borrador.");

        var cierre = await _context.CierresProveedor
            .Include(x => x.Detalles)
            .FirstOrDefaultAsync(x => x.FechaDesde == desde && x.FechaHasta == hasta);

        if (cierre is null)
            return ResultadoOperacionDto.Fail("No existe borrador para este rango. Genere el borrador primero.");

        if (cierre.Estado == EstadoCierreProveedor.Confirmado)
            return ResultadoOperacionDto.Fail("La liquidacion ya fue confirmada y no puede modificarse.");

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
            EstadoCierreProveedor.Borrador);

        await _context.SaveChangesAsync();

        return ResultadoOperacionDto.Ok("Borrador guardado correctamente.");
    }

    public async Task<byte[]> GenerarExcelProveedorAsync(int cierreProveedorId)
    {
        var cierre = await _context.CierresProveedor
            .AsNoTracking()
            .Include(x => x.Detalles)
            .FirstOrDefaultAsync(x => x.Id == cierreProveedorId);

        if (cierre is null)
            throw new InvalidOperationException("No se encontro el cierre proveedor.");

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
            Items = cierre.Detalles
                .OrderBy(x => x.Fecha)
                .ThenBy(x => x.EmpleadoNombre)
                .Select(x => new CierreProveedorItemDto
                {
                    ConsumoMenuId = x.ConsumoMenuId,
                    Fecha = x.Fecha,
                    EmpleadoId = x.EmpleadoId,
                    Dni = x.Dni,
                    EmpleadoNombre = x.EmpleadoNombre,
                    TipoServicio = x.TipoServicio,
                    TipoPagoMenu = x.TipoPagoMenu,
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
            return ResultadoOperacionDto.Fail("No hay consumos para liquidar con proveedor.");

        var cierre = await _context.CierresProveedor
            .Include(x => x.Detalles)
            .FirstOrDefaultAsync(x => x.FechaDesde == desde && x.FechaHasta == hasta);

        if (cierre is null)
            return ResultadoOperacionDto.Fail("No existe borrador para confirmar. Genere y guarde el borrador primero.");

        if (cierre.Estado == EstadoCierreProveedor.Confirmado)
            return ResultadoOperacionDto.Fail("La liquidacion ya fue confirmada.");

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

        return ResultadoOperacionDto.Ok($"Liquidacion confirmada por S/ {cierre.TotalLiquidarProveedor:N2}.");
    }

    private async Task<ResultadoOperacionDto> ValidarItemsBorradorAsync(
        ConfirmarCierreProveedorDto input,
        DateTime desde,
        DateTime hasta)
    {
        var consumoIds = input.Items.Select(x => x.ConsumoMenuId).Distinct().ToList();

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

        foreach (var item in input.Items.Where(x => x.ExcluirDeProveedor))
        {
            if (string.IsNullOrWhiteSpace(item.MotivoExclusion))
                return ResultadoOperacionDto.Fail("Debe ingresar motivo para cada excepcion.");
        }

        return ResultadoOperacionDto.Ok("OK");
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
                Fecha = item.Fecha.Date,
                EmpleadoId = item.EmpleadoId,
                Dni = item.Dni,
                EmpleadoNombre = item.EmpleadoNombre,
                TipoServicio = item.TipoServicio,
                TipoPagoMenu = item.TipoPagoMenu,
                Importe = item.Importe,
                IncluidoProveedor = !item.ExcluirDeProveedor,
                ExcluidoPorPagoDirecto = item.ExcluirDeProveedor,
                MotivoExclusion = item.ExcluirDeProveedor ? item.MotivoExclusion?.Trim() : null
            });
        }

        cierre.TotalMenusActivos = cierre.Detalles.Count(x => x.TipoPagoMenu == TipoPagoMenu.Empresa);
        cierre.TotalMenusPlanilla = cierre.Detalles.Count(x => x.TipoPagoMenu == TipoPagoMenu.DescuentoPlanilla);
        cierre.TotalMenusPlanillaExcluidos = cierre.Detalles.Count(x => x.ExcluidoPorPagoDirecto);
        cierre.TotalPersonalActivo = cierre.Detalles
            .Where(x => x.TipoPagoMenu == TipoPagoMenu.Empresa)
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
                </Relationships>
                """);

            AddEntry(archive, "xl/worksheets/sheet1.xml", BuildSheet(GetResumenGeneralRows(cierre)));
            AddEntry(archive, "xl/worksheets/sheet2.xml", BuildSheet(GetResumenEmpleadoRows(cierre)));
            AddEntry(archive, "xl/worksheets/sheet3.xml", BuildSheet(GetDetalleRows(cierre)));
        }

        return stream.ToArray();
    }

    private static List<object?[]> GetResumenGeneralRows(CierreProveedor cierre)
    {
        return new List<object?[]>
        {
            new object?[] { "Liquidacion proveedor", string.Empty },
            new object?[] { "Estado", cierre.Estado.ToString() },
            new object?[] { "Fecha desde", cierre.FechaDesde.ToString("dd/MM/yyyy") },
            new object?[] { "Fecha hasta", cierre.FechaHasta.ToString("dd/MM/yyyy") },
            new object?[] { "Generado/confirmado por", cierre.UsuarioConfirmacionNombre },
            new object?[] { "Fecha registro", cierre.FechaConfirmacion.ToString("dd/MM/yyyy HH:mm") },
            new object?[] { string.Empty, string.Empty },
            new object?[] { "Concepto", "Importe" },
            new object?[] { "Personal activo", cierre.TotalPersonalActivo },
            new object?[] { "Descuento planilla incluido", cierre.TotalPlanilla },
            new object?[] { "Total a liquidar proveedor", cierre.TotalLiquidarProveedor },
            new object?[] { "Excepciones para revision/concesionario", cierre.TotalExcluidoRevision },
            new object?[] { string.Empty, string.Empty },
            new object?[] { "Cantidad activos", cierre.TotalMenusActivos },
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
                "Trabajador",
                "Menus activo",
                "Menus planilla",
                "Excepciones",
                "Total proveedor",
                "Total revision"
            }
        };

        rows.AddRange(cierre.Detalles
            .GroupBy(x => new { x.EmpleadoId, x.Dni, x.EmpleadoNombre })
            .OrderBy(x => x.Key.EmpleadoNombre)
            .Select(x => new object?[]
            {
                x.Key.Dni,
                x.Key.EmpleadoNombre,
                x.Count(d => d.TipoPagoMenu == TipoPagoMenu.Empresa),
                x.Count(d => d.TipoPagoMenu == TipoPagoMenu.DescuentoPlanilla && d.IncluidoProveedor),
                x.Count(d => d.ExcluidoPorPagoDirecto),
                x.Where(d => d.IncluidoProveedor).Sum(d => d.Importe),
                x.Where(d => d.ExcluidoPorPagoDirecto).Sum(d => d.Importe)
            }));

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
                "Trabajador",
                "Servicio",
                "Tipo pago",
                "Incluido proveedor",
                "Revision/concesionario",
                "Motivo excepcion",
                "Importe"
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
                x.TipoServicio.ToString(),
                x.TipoPagoMenu == TipoPagoMenu.Empresa ? "Personal activo" : "Descuento planilla",
                x.IncluidoProveedor ? "Si" : "No",
                x.ExcluidoPorPagoDirecto ? "Si" : "No",
                x.MotivoExclusion ?? string.Empty,
                x.Importe
            }));

        return rows;
    }

    private static string BuildSheet(List<object?[]> rows)
    {
        var builder = new StringBuilder();
        builder.Append("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?><worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><sheetData>""");

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var rowNumber = rowIndex + 1;
            builder.Append($"""<row r="{rowNumber}">""");

            for (var colIndex = 0; colIndex < rows[rowIndex].Length; colIndex++)
            {
                var reference = $"{GetColumnName(colIndex + 1)}{rowNumber}";
                builder.Append(BuildCell(reference, rows[rowIndex][colIndex]));
            }

            builder.Append("</row>");
        }

        builder.Append("</sheetData></worksheet>");

        return builder.ToString();
    }

    private static string BuildCell(string reference, object? value)
    {
        if (value is null)
            return $"""<c r="{reference}"/>""";

        if (value is int or decimal)
        {
            var number = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
            return $"""<c r="{reference}"><v>{number}</v></c>""";
        }

        var text = SecurityElement.Escape(value.ToString()) ?? string.Empty;
        return $"""<c r="{reference}" t="inlineStr"><is><t>{text}</t></is></c>""";
    }

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
