using Menu.Data;
using Menu.Models;
using Microsoft.EntityFrameworkCore;

namespace Menu.Services;

public class ConfiguracionMenuService
{
    private readonly AppDbContext _context;

    public ConfiguracionMenuService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ConfiguracionMenu> GetActualAsync()
    {
        var config = await _context.ConfiguracionesMenu
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        if (config is not null)
            return config;

        config = new ConfiguracionMenu
        {
            PrecioMenu = 12.00m,
            Moneda = "PEN",
            FechaActualizacion = DateTime.Now
        };

        _context.ConfiguracionesMenu.Add(config);
        await _context.SaveChangesAsync();

        return config;
    }

    public async Task<(bool Success, string Message)> UpdatePrecioAsync(decimal precioMenu)
    {
        if (precioMenu <= 0)
            return (false, "El precio del menú debe ser mayor a cero.");

        var config = await GetActualAsync();

        config.PrecioMenu = precioMenu;
        config.Moneda = "PEN";
        config.FechaActualizacion = DateTime.Now;

        await _context.SaveChangesAsync();

        return (true, "Precio del menú actualizado correctamente.");
    }
}