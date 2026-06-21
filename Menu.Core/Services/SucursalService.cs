using Menu.Data;
using Menu.Models;
using Microsoft.EntityFrameworkCore;

namespace Menu.Services;

public class SucursalService
{
    private readonly AppDbContext _context;

    public SucursalService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Sucursal>> GetAllAsync()
    {
        return await _context.Sucursales
            .AsNoTracking()
            .Include(x => x.EmpresaCliente)
            .OrderBy(x => x.Nombre)
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> CreateAsync(Sucursal sucursal)
    {
        Normalizar(sucursal);

        if (string.IsNullOrWhiteSpace(sucursal.Nombre))
            return (false, "El nombre de la sucursal es obligatorio.");

        if (await ExisteNombreAsync(sucursal.Nombre, sucursal.EmpresaClienteId))
            return (false, "Ya existe una sucursal con ese nombre para la empresa cliente seleccionada.");

        if (sucursal.EmpresaClienteId.HasValue && !await ExisteEmpresaAsync(sucursal.EmpresaClienteId.Value))
            return (false, "La empresa cliente seleccionada no existe.");

        sucursal.Activo = true;
        sucursal.FechaCreacion = DateTime.Now;

        _context.Sucursales.Add(sucursal);
        await _context.SaveChangesAsync();

        return (true, "Sucursal registrada correctamente.");
    }

    public async Task<(bool Success, string Message)> UpdateAsync(Sucursal sucursal)
    {
        var dbSucursal = await _context.Sucursales.FindAsync(sucursal.Id);

        if (dbSucursal is null)
            return (false, "La sucursal no existe.");

        Normalizar(sucursal);

        if (string.IsNullOrWhiteSpace(sucursal.Nombre))
            return (false, "El nombre de la sucursal es obligatorio.");

        if (await ExisteNombreAsync(sucursal.Nombre, sucursal.EmpresaClienteId, sucursal.Id))
            return (false, "Ya existe otra sucursal con ese nombre para la empresa cliente seleccionada.");

        if (sucursal.EmpresaClienteId.HasValue && !await ExisteEmpresaAsync(sucursal.EmpresaClienteId.Value))
            return (false, "La empresa cliente seleccionada no existe.");

        dbSucursal.Nombre = sucursal.Nombre;
        dbSucursal.Direccion = sucursal.Direccion;
        dbSucursal.EmpresaClienteId = sucursal.EmpresaClienteId;
        dbSucursal.Activo = sucursal.Activo;

        await _context.SaveChangesAsync();

        return (true, "Sucursal actualizada correctamente.");
    }

    public async Task<(bool Success, string Message)> ToggleActivoAsync(int id)
    {
        var sucursal = await _context.Sucursales.FindAsync(id);

        if (sucursal is null)
            return (false, "La sucursal no existe.");

        sucursal.Activo = !sucursal.Activo;
        await _context.SaveChangesAsync();

        return (true, sucursal.Activo ? "Sucursal activada." : "Sucursal desactivada.");
    }

    private async Task<bool> ExisteNombreAsync(string nombre, int? empresaClienteId, int? excludeId = null)
    {
        return await _context.Sucursales
            .AnyAsync(x => x.Nombre.ToLower() == nombre.ToLower() &&
                           x.EmpresaClienteId == empresaClienteId &&
                           (!excludeId.HasValue || x.Id != excludeId.Value));
    }

    private async Task<bool> ExisteEmpresaAsync(int empresaClienteId)
    {
        return await _context.EmpresasCliente.AnyAsync(x => x.Id == empresaClienteId);
    }

    private static void Normalizar(Sucursal sucursal)
    {
        sucursal.Nombre = NormalizarTexto(sucursal.Nombre);
        sucursal.Direccion = NormalizarOpcional(sucursal.Direccion);
    }

    private static string NormalizarTexto(string? value)
    {
        var texto = (value ?? string.Empty).Trim();
        return string.Join(' ', texto.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string? NormalizarOpcional(string? value)
    {
        var texto = NormalizarTexto(value);
        return string.IsNullOrWhiteSpace(texto) ? null : texto;
    }
}
