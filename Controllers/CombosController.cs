using EmpanadasDLujo.API.Data;
using EmpanadasDLujo.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmpanadasDLujo.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CombosController : ControllerBase
{
    private readonly AppDbContext _context;

    public CombosController(AppDbContext context) => _context = context;

    /// <summary>
    /// Devuelve los combos con su precio fijo, ahorro y la lista de SKUs que incluyen.
    /// El combo tiene precio fijo (precio_combo) y no participa de la regla mayorista.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ComboDto>>> GetCombos([FromQuery] bool? activo = null)
    {
        var query = _context.Combos
            .Include(c => c.Componentes).ThenInclude(cc => cc.SKU).ThenInclude(s => s.Producto)
            .Include(c => c.Componentes).ThenInclude(cc => cc.SKU).ThenInclude(s => s.Sabor)
            .AsQueryable();

        if (activo.HasValue)
            query = query.Where(c => c.Activo == activo.Value);

        var combos = await query
            .OrderBy(c => c.Orden ?? int.MaxValue)
            .ThenBy(c => c.CodigoCombo)
            .ToListAsync();

        var resultado = combos.Select(MapCombo).ToList();
        return Ok(resultado);
    }

    [HttpGet("{codigo}")]
    public async Task<ActionResult<ComboDto>> GetByCodigo(string codigo)
    {
        var combo = await _context.Combos
            .Include(c => c.Componentes).ThenInclude(cc => cc.SKU).ThenInclude(s => s.Producto)
            .Include(c => c.Componentes).ThenInclude(cc => cc.SKU).ThenInclude(s => s.Sabor)
            .FirstOrDefaultAsync(c => c.CodigoCombo == codigo);

        if (combo is null) return NotFound();

        return Ok(MapCombo(combo));
    }

    private static ComboDto MapCombo(Models.Combo c) => new()
    {
        IdCombo          = c.IdCombo,
        CodigoCombo      = c.CodigoCombo,
        Nombre           = c.Nombre,
        Subcategoria     = c.Subcategoria,
        DescripcionCorta = c.DescripcionCorta,
        DescripcionLarga = c.DescripcionLarga,
        PrecioNormal     = c.PrecioNormal,
        PrecioCombo      = c.PrecioCombo,
        Ahorro           = c.PrecioNormal - c.PrecioCombo,
        PesoTotalG       = c.PesoTotalG,
        UnidadesTotales  = c.UnidadesTotales,
        Activo           = c.Activo,
        Orden            = c.Orden,
        UrlImage         = c.UrlImage,
        BadgeDescripcion = c.BadgeDescripcion,
        Componentes = c.Componentes
            .Select(cc => new ComboComponenteDto
            {
                CodigoSku          = cc.CodigoSku,
                Producto           = cc.SKU?.Producto?.Nombre,
                Sabor              = cc.SKU?.Sabor?.Nombre,
                GramajeG           = cc.SKU?.GramajeG ?? 0,
                UnidadesPorPaquete = cc.SKU?.UnidadesPorPaquete ?? 0,
                CantidadPaquetes   = cc.CantidadPaquetes
            }).ToList()
    };
}
