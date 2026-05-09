using EmpanadasDLujo.API.Data;
using EmpanadasDLujo.API.DTOs;
using EmpanadasDLujo.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmpanadasDLujo.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PreciosSKUController : ControllerBase
{
    private readonly AppDbContext _context;

    public PreciosSKUController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PrecioSKUDto>>> GetAll(
        [FromQuery] string? codigoSku = null,
        [FromQuery] int? idLista = null)
    {
        var query = _context.PreciosSKU
            .Include(p => p.ListaPrecios)
            .AsQueryable();

        if (!string.IsNullOrEmpty(codigoSku))
            query = query.Where(p => p.CodigoSku == codigoSku);

        if (idLista.HasValue)
            query = query.Where(p => p.IdLista == idLista.Value);

        var items = await query.Select(p => new PrecioSKUDto
        {
            IdPrecio = p.IdPrecio,
            CodigoSku = p.CodigoSku,
            IdLista = p.IdLista,
            NombreLista = p.ListaPrecios.Nombre,
            PrecioPaquete = p.PrecioPaquete,
            PrecioPorUnidad = p.PrecioPorUnidad
        }).ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PrecioSKUDto>> GetById(int id)
    {
        var item = await _context.PreciosSKU
            .Include(p => p.ListaPrecios)
            .FirstOrDefaultAsync(p => p.IdPrecio == id);

        if (item is null) return NotFound();

        return Ok(new PrecioSKUDto
        {
            IdPrecio = item.IdPrecio,
            CodigoSku = item.CodigoSku,
            IdLista = item.IdLista,
            NombreLista = item.ListaPrecios?.Nombre,
            PrecioPaquete = item.PrecioPaquete,
            PrecioPorUnidad = item.PrecioPorUnidad
        });
    }

    [HttpPost]
    public async Task<ActionResult<PrecioSKUDto>> Create(PrecioSKUCreateDto dto)
    {
        if (!await _context.SKUs.AnyAsync(s => s.CodigoSku == dto.CodigoSku))
            return BadRequest("El SKU indicado no existe.");

        if (!await _context.ListasPrecios.AnyAsync(l => l.IdLista == dto.IdLista))
            return BadRequest("La lista de precios indicada no existe.");

        if (await _context.PreciosSKU.AnyAsync(p => p.CodigoSku == dto.CodigoSku && p.IdLista == dto.IdLista))
            return Conflict("Ya existe un precio para ese SKU en esa lista.");

        var entity = new PrecioSKU
        {
            CodigoSku = dto.CodigoSku,
            IdLista = dto.IdLista,
            PrecioPaquete = dto.PrecioPaquete,
            PrecioPorUnidad = dto.PrecioPorUnidad
        };
        _context.PreciosSKU.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.IdPrecio },
            new PrecioSKUDto
            {
                IdPrecio = entity.IdPrecio,
                CodigoSku = entity.CodigoSku,
                IdLista = entity.IdLista,
                PrecioPaquete = entity.PrecioPaquete,
                PrecioPorUnidad = entity.PrecioPorUnidad
            });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, PrecioSKUUpdateDto dto)
    {
        var entity = await _context.PreciosSKU.FindAsync(id);
        if (entity is null) return NotFound();

        entity.PrecioPaquete = dto.PrecioPaquete;
        entity.PrecioPorUnidad = dto.PrecioPorUnidad;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.PreciosSKU.FindAsync(id);
        if (entity is null) return NotFound();
        _context.PreciosSKU.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
