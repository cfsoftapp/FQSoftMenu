using Menu.Data;
using Menu.Enums;
using Menu.Models;
using Microsoft.EntityFrameworkCore;

namespace Menu.Services;

public class EmpleadoService
{
    private readonly AppDbContext _context;

    public EmpleadoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Empleado>> GetAllAsync()
    {
        return await _context.Empleados
            .OrderBy(e => e.Nombres)
            .ThenBy(e => e.Apellidos)
            .ToListAsync();
    }

    public async Task<Empleado?> GetByIdAsync(int id)
    {
        return await _context.Empleados
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Empleado?> GetByDniAsync(string dni)
    {
        dni = dni.Trim();

        return await _context.Empleados
            .FirstOrDefaultAsync(e => e.Dni == dni);
    }

    public async Task<bool> ExistsDniAsync(string dni, int? excludeId = null)
    {
        dni = dni.Trim();

        return await _context.Empleados
            .AnyAsync(e => e.Dni == dni &&
                           (!excludeId.HasValue || e.Id != excludeId.Value));
    }

    public async Task<(bool Success, string Message)> CreateAsync(Empleado empleado)
    {
        empleado.Dni = empleado.Dni.Trim();
        empleado.Nombres = empleado.Nombres.Trim();
        empleado.Apellidos = empleado.Apellidos.Trim();

        if (string.IsNullOrWhiteSpace(empleado.Dni))
            return (false, "El DNI es obligatorio.");

        if (string.IsNullOrWhiteSpace(empleado.Nombres))
            return (false, "Los nombres son obligatorios.");

        if (string.IsNullOrWhiteSpace(empleado.Apellidos))
            return (false, "Los apellidos son obligatorios.");

        if (await ExistsDniAsync(empleado.Dni))
            return (false, "Ya existe un empleado con ese DNI.");

        empleado.FechaCreacion = DateTime.Now;
        empleado.Activo = true;

        _context.Empleados.Add(empleado);
        await _context.SaveChangesAsync();

        return (true, "Empleado registrado correctamente.");
    }

    public async Task<(bool Success, string Message)> UpdateAsync(Empleado empleado)
    {
        var dbEmpleado = await _context.Empleados.FindAsync(empleado.Id);

        if (dbEmpleado is null)
            return (false, "El empleado no existe.");

        empleado.Dni = empleado.Dni.Trim();
        empleado.Nombres = empleado.Nombres.Trim();
        empleado.Apellidos = empleado.Apellidos.Trim();

        if (string.IsNullOrWhiteSpace(empleado.Dni))
            return (false, "El DNI es obligatorio.");

        if (string.IsNullOrWhiteSpace(empleado.Nombres))
            return (false, "Los nombres son obligatorios.");

        if (string.IsNullOrWhiteSpace(empleado.Apellidos))
            return (false, "Los apellidos son obligatorios.");

        if (await ExistsDniAsync(empleado.Dni, empleado.Id))
            return (false, "Ya existe otro empleado con ese DNI.");

        dbEmpleado.Dni = empleado.Dni;
        dbEmpleado.Nombres = empleado.Nombres;
        dbEmpleado.Apellidos = empleado.Apellidos;
        dbEmpleado.Estado = empleado.Estado;
        dbEmpleado.Categoria = empleado.Categoria;
        dbEmpleado.Activo = empleado.Activo;

        await _context.SaveChangesAsync();

        return (true, "Empleado actualizado correctamente.");
    }

    public async Task<(bool Success, string Message)> ToggleActivoAsync(int id)
    {
        var empleado = await _context.Empleados.FindAsync(id);

        if (empleado is null)
            return (false, "El empleado no existe.");

        empleado.Activo = !empleado.Activo;

        await _context.SaveChangesAsync();

        return (true, empleado.Activo ? "Empleado activado." : "Empleado desactivado.");
    }

    public async Task<(bool Success, string Message)> ToggleEstadoBeneficioAsync(int id)
    {
        var empleado = await _context.Empleados.FindAsync(id);

        if (empleado is null)
            return (false, "El empleado no existe.");

        empleado.Estado = empleado.Estado == EstadoEmpleado.Activo
            ? EstadoEmpleado.Suspendido
            : EstadoEmpleado.Activo;

        await _context.SaveChangesAsync();

        return empleado.Estado == EstadoEmpleado.Activo
            ? (true, "Beneficio de menú activado.")
            : (true, "Beneficio de menú suspendido.");
    }
}