using Menu.Data;
using Menu.DTOs.Empleados;
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

    public Task<EmpleadoCargaMasivaResumenDto> PrevisualizarCargaMasivaAsync(List<EmpleadoCargaMasivaFilaDto> filas)
    {
        return ProcesarCargaMasivaAsync(filas, guardar: false);
    }

    public Task<EmpleadoCargaMasivaResumenDto> ImportarCargaMasivaAsync(List<EmpleadoCargaMasivaFilaDto> filas)
    {
        return ProcesarCargaMasivaAsync(filas, guardar: true);
    }

    private async Task<EmpleadoCargaMasivaResumenDto> ProcesarCargaMasivaAsync(List<EmpleadoCargaMasivaFilaDto> filas, bool guardar)
    {
        var resumen = new EmpleadoCargaMasivaResumenDto
        {
            TotalFilas = filas.Count
        };

        if (filas.Count == 0)
            return resumen;

        var dnisArchivo = filas
            .Select(x => Normalizar(x.Dni))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        var dnisDuplicadosArchivo = dnisArchivo
            .GroupBy(x => x)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToHashSet();

        var empleadosExistentes = await _context.Empleados
            .Where(x => dnisArchivo.Contains(x.Dni))
            .ToDictionaryAsync(x => x.Dni);

        var nuevos = new List<Empleado>();

        foreach (var fila in filas)
        {
            var dni = Normalizar(fila.Dni);
            var nombres = NormalizarTexto(fila.Nombres);
            var apellidos = NormalizarTexto(fila.Apellidos);
            var trabajador = $"{nombres} {apellidos}".Trim();

            var resultado = new EmpleadoCargaMasivaResultadoDto
            {
                NumeroFila = fila.NumeroFila,
                Dni = dni,
                Trabajador = trabajador
            };

            if (string.IsNullOrWhiteSpace(dni))
            {
                resultado.Estado = "Observado";
                resultado.Mensaje = "DNI obligatorio.";
                resumen.Resultados.Add(resultado);
                continue;
            }

            if (string.IsNullOrWhiteSpace(nombres) || string.IsNullOrWhiteSpace(apellidos))
            {
                resultado.Estado = "Observado";
                resultado.Mensaje = "Nombres y apellidos son obligatorios.";
                resumen.Resultados.Add(resultado);
                continue;
            }

            if (dnisDuplicadosArchivo.Contains(dni))
            {
                resultado.Estado = "Duplicado archivo";
                resultado.Mensaje = "El DNI aparece mas de una vez en el archivo.";
                resumen.Resultados.Add(resultado);
                continue;
            }

            if (empleadosExistentes.TryGetValue(dni, out var existente))
            {
                resultado.Trabajador = existente.NombreCompleto;
                resultado.Estado = "Ya registrado";
                resultado.Mensaje = $"Beneficio: {existente.Estado}. Empleado: {(existente.Activo ? "Activo" : "Inactivo")}.";
                resumen.Resultados.Add(resultado);
                continue;
            }

            if (!TryParseTipoPersonal(fila.TipoPersonalTexto, out var tipoPersonal))
            {
                resultado.Estado = "Observado";
                resultado.Mensaje = "Tipo de personal invalido. Use Empleado, Obrero, Practicante, Tercero, Visitante, Gerencia u Otro.";
                resumen.Resultados.Add(resultado);
                continue;
            }

            if (!TryParseEstado(fila.EstadoTexto, out var estado))
            {
                resultado.Estado = "Observado";
                resultado.Mensaje = "Estado invalido. Use Activo o Suspendido.";
                resumen.Resultados.Add(resultado);
                continue;
            }

            var activo = ParseActivo(fila.ActivoTexto);

            nuevos.Add(new Empleado
            {
                Dni = dni,
                Nombres = nombres,
                Apellidos = apellidos,
                Categoria = tipoPersonal,
                Estado = estado,
                Activo = activo,
                FechaCreacion = DateTime.Now
            });

            resultado.Estado = guardar ? "Importado" : "Pendiente";
            resultado.Mensaje = guardar
                ? "Empleado registrado correctamente."
                : "Listo para importar.";
            resultado.Importado = guardar;
            resumen.Resultados.Add(resultado);
        }

        if (guardar && nuevos.Count > 0)
        {
            _context.Empleados.AddRange(nuevos);
            await _context.SaveChangesAsync();
        }

        resumen.Importados = guardar ? nuevos.Count : 0;
        return resumen;
    }

    private static string Normalizar(string? value)
    {
        return (value ?? string.Empty).Trim();
    }

    private static string NormalizarTexto(string? value)
    {
        var texto = Normalizar(value);
        return string.Join(' ', texto.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static bool TryParseTipoPersonal(string? value, out CategoriaEmpleado tipoPersonal)
    {
        var texto = Normalizar(value);

        if (string.IsNullOrWhiteSpace(texto))
        {
            tipoPersonal = CategoriaEmpleado.Obrero;
            return true;
        }

        if (string.Equals(texto, "Administrativo", StringComparison.OrdinalIgnoreCase))
        {
            tipoPersonal = CategoriaEmpleado.Empleado;
            return true;
        }

        if (string.Equals(texto, "Operativo", StringComparison.OrdinalIgnoreCase))
        {
            tipoPersonal = CategoriaEmpleado.Obrero;
            return true;
        }

        return Enum.TryParse(texto, ignoreCase: true, out tipoPersonal);
    }

    private static bool TryParseEstado(string? value, out EstadoEmpleado estado)
    {
        var texto = Normalizar(value);

        if (string.IsNullOrWhiteSpace(texto))
        {
            estado = EstadoEmpleado.Activo;
            return true;
        }

        return Enum.TryParse(texto, ignoreCase: true, out estado);
    }

    private static bool ParseActivo(string? value)
    {
        var texto = Normalizar(value).ToLowerInvariant();

        return string.IsNullOrWhiteSpace(texto) ||
               texto is "si" or "s" or "true" or "1" or "activo";
    }
}
