using Menu.Data;
using Menu.Models;
using Microsoft.EntityFrameworkCore;

namespace Menu.Services;

public class EmpresaClienteService
{
    private readonly AppDbContext _context;

    public EmpresaClienteService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmpresaCliente>> GetAllAsync()
    {
        return await _context.EmpresasCliente
            .AsNoTracking()
            .OrderBy(x => x.NombreComercial)
            .ToListAsync();
    }

    public async Task<List<EmpresaCliente>> GetActivasAsync()
    {
        return await _context.EmpresasCliente
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.NombreComercial)
            .ToListAsync();
    }

    public async Task<(bool Success, string Message)> CreateAsync(EmpresaCliente empresa)
    {
        Normalizar(empresa);

        if (string.IsNullOrWhiteSpace(empresa.NombreComercial))
            return (false, "El nombre comercial de la empresa cliente es obligatorio.");

        if (await ExisteNombreAsync(empresa.NombreComercial))
            return (false, "Ya existe una empresa cliente con ese nombre comercial.");

        if (!string.IsNullOrWhiteSpace(empresa.Ruc) && await ExisteRucAsync(empresa.Ruc))
            return (false, "Ya existe una empresa cliente con ese RUC.");

        empresa.Activo = true;
        empresa.FechaCreacion = DateTime.Now;

        _context.EmpresasCliente.Add(empresa);
        await _context.SaveChangesAsync();

        return (true, "Empresa cliente registrada correctamente.");
    }

    public async Task<(bool Success, string Message)> UpdateAsync(EmpresaCliente empresa)
    {
        var dbEmpresa = await _context.EmpresasCliente.FindAsync(empresa.Id);

        if (dbEmpresa is null)
            return (false, "La empresa cliente no existe.");

        Normalizar(empresa);

        if (string.IsNullOrWhiteSpace(empresa.NombreComercial))
            return (false, "El nombre comercial de la empresa cliente es obligatorio.");

        if (await ExisteNombreAsync(empresa.NombreComercial, empresa.Id))
            return (false, "Ya existe otra empresa cliente con ese nombre comercial.");

        if (!string.IsNullOrWhiteSpace(empresa.Ruc) && await ExisteRucAsync(empresa.Ruc, empresa.Id))
            return (false, "Ya existe otra empresa cliente con ese RUC.");

        dbEmpresa.NombreComercial = empresa.NombreComercial;
        dbEmpresa.RazonSocial = empresa.RazonSocial;
        dbEmpresa.Ruc = empresa.Ruc;
        dbEmpresa.Activo = empresa.Activo;

        await _context.SaveChangesAsync();

        return (true, "Empresa cliente actualizada correctamente.");
    }

    public async Task<(bool Success, string Message)> ToggleActivoAsync(int id)
    {
        var empresa = await _context.EmpresasCliente.FindAsync(id);

        if (empresa is null)
            return (false, "La empresa cliente no existe.");

        empresa.Activo = !empresa.Activo;
        await _context.SaveChangesAsync();

        return (true, empresa.Activo ? "Empresa cliente activada." : "Empresa cliente desactivada.");
    }

    private async Task<bool> ExisteNombreAsync(string nombre, int? excludeId = null)
    {
        return await _context.EmpresasCliente
            .AnyAsync(x => x.NombreComercial.ToLower() == nombre.ToLower() &&
                           (!excludeId.HasValue || x.Id != excludeId.Value));
    }

    private async Task<bool> ExisteRucAsync(string ruc, int? excludeId = null)
    {
        return await _context.EmpresasCliente
            .AnyAsync(x => x.Ruc != null &&
                           x.Ruc.ToLower() == ruc.ToLower() &&
                           (!excludeId.HasValue || x.Id != excludeId.Value));
    }

    private static void Normalizar(EmpresaCliente empresa)
    {
        empresa.NombreComercial = NormalizarTexto(empresa.NombreComercial);
        empresa.RazonSocial = NormalizarOpcional(empresa.RazonSocial);
        empresa.Ruc = NormalizarOpcional(empresa.Ruc);
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
