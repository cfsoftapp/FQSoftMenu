using Menu.Data;
using Menu.Models;
using Microsoft.EntityFrameworkCore;

namespace Menu.Services;

public class TipoEmpleadoService
{
    private readonly AppDbContext _context;

    public TipoEmpleadoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TipoEmpleado>> GetAllAsync()
    {
        return await _context.TiposEmpleado
            .AsNoTracking()
            .OrderBy(x => x.Nombre)
            .ToListAsync();
    }

    public async Task<List<TipoEmpleado>> GetActivosAsync()
    {
        return await _context.TiposEmpleado
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre)
            .ToListAsync();
    }

    public async Task<TipoEmpleado?> GetDefaultAsync()
    {
        return await _context.TiposEmpleado
            .Where(x => x.Activo)
            .OrderBy(x => x.Nombre == "Obrero" ? 0 : 1)
            .ThenBy(x => x.Nombre)
            .FirstOrDefaultAsync();
    }

    public async Task<TipoEmpleado?> GetByNombreAsync(string nombre)
    {
        nombre = Normalizar(nombre);

        return await _context.TiposEmpleado
            .FirstOrDefaultAsync(x => x.Nombre.ToLower() == nombre.ToLower());
    }

    public async Task<(bool Success, string Message)> CreateAsync(TipoEmpleado tipo)
    {
        tipo.Nombre = Normalizar(tipo.Nombre);
        tipo.Descripcion = NormalizarOpcional(tipo.Descripcion);

        if (string.IsNullOrWhiteSpace(tipo.Nombre))
            return (false, "El nombre del tipo de comensal es obligatorio.");

        if (await ExisteNombreAsync(tipo.Nombre))
            return (false, "Ya existe un tipo de comensal con ese nombre.");

        tipo.Activo = true;
        tipo.FechaCreacion = DateTime.Now;

        _context.TiposEmpleado.Add(tipo);
        await _context.SaveChangesAsync();

        return (true, "Tipo de comensal registrado correctamente.");
    }

    public async Task<(bool Success, string Message)> UpdateAsync(TipoEmpleado tipo)
    {
        var dbTipo = await _context.TiposEmpleado.FindAsync(tipo.Id);

        if (dbTipo is null)
            return (false, "El tipo de comensal no existe.");

        tipo.Nombre = Normalizar(tipo.Nombre);
        tipo.Descripcion = NormalizarOpcional(tipo.Descripcion);

        if (string.IsNullOrWhiteSpace(tipo.Nombre))
            return (false, "El nombre del tipo de comensal es obligatorio.");

        if (await ExisteNombreAsync(tipo.Nombre, tipo.Id))
            return (false, "Ya existe otro tipo de comensal con ese nombre.");

        dbTipo.Nombre = tipo.Nombre;
        dbTipo.Descripcion = tipo.Descripcion;
        dbTipo.Activo = tipo.Activo;

        await _context.SaveChangesAsync();

        return (true, "Tipo de comensal actualizado correctamente.");
    }

    public async Task<(bool Success, string Message)> ToggleActivoAsync(int id)
    {
        var tipo = await _context.TiposEmpleado.FindAsync(id);

        if (tipo is null)
            return (false, "El tipo de comensal no existe.");

        if (tipo.Activo && await EstaEnUsoAsync(id))
            return (false, "No se puede desactivar un tipo de comensal asignado a comensales.");

        tipo.Activo = !tipo.Activo;
        await _context.SaveChangesAsync();

        return (true, tipo.Activo ? "Tipo de comensal activado." : "Tipo de comensal desactivado.");
    }

    public async Task<TipoEmpleado> GetOrCreateAsync(string nombre)
    {
        nombre = Normalizar(nombre);

        if (string.IsNullOrWhiteSpace(nombre))
            nombre = "Obrero";

        var existente = await GetByNombreAsync(nombre);

        if (existente is not null)
            return existente;

        var tipo = new TipoEmpleado
        {
            Nombre = nombre,
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        _context.TiposEmpleado.Add(tipo);
        await _context.SaveChangesAsync();
        return tipo;
    }

    private async Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null)
    {
        return await _context.TiposEmpleado
            .AnyAsync(x => x.Nombre.ToLower() == nombre.ToLower() &&
                           (!excludeId.HasValue || x.Id != excludeId.Value));
    }

    private async Task<bool> EstaEnUsoAsync(int id)
    {
        return await _context.Empleados.AnyAsync(x => x.TipoEmpleadoId == id);
    }

    private static string Normalizar(string? value)
    {
        var texto = (value ?? string.Empty).Trim();
        return string.Join(' ', texto.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string? NormalizarOpcional(string? value)
    {
        var texto = Normalizar(value);
        return string.IsNullOrWhiteSpace(texto) ? null : texto;
    }
}
