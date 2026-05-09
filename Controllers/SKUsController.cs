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
public class SKUsController : ControllerBase
{
    private readonly AppDbContext _context;

    public SKUsController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SKUDto>>> GetAll([FromQuery] bool? activo = null)
    {
        var query = _context.SKUs
            .Include(s => s.Producto)
            .Include(s => s.Sabor)
            .AsQueryable();

        if (activo.HasValue)
            query = query.Where(s => s.Activo == activo.Value);

        var items = await query.Select(s => new SKUDto
        {
            CodigoSku = s.CodigoSku,
            IdProducto = s.IdProducto,
            NombreProducto = s.Producto.Nombre,
            IdSabor = s.IdSabor,
            NombreSabor = s.Sabor.Nombre,
            GramajeG = s.GramajeG,
            UnidadesPorPaquete = s.UnidadesPorPaquete,
            Activo = s.Activo
        }).ToListAsync();

        return Ok(items);
    }

    [HttpGet("{codigo}")]
    public async Task<ActionResult<SKUDto>> GetByCodigo(string codigo)
    {
        var item = await _context.SKUs
            .Include(s => s.Producto)
            .Include(s => s.Sabor)
            .FirstOrDefaultAsync(s => s.CodigoSku == codigo);

        if (item is null) return NotFound();

        return Ok(new SKUDto
        {
            CodigoSku = item.CodigoSku,
            IdProducto = item.IdProducto,
            NombreProducto = item.Producto?.Nombre,
            IdSabor = item.IdSabor,
            NombreSabor = item.Sabor?.Nombre,
            GramajeG = item.GramajeG,
            UnidadesPorPaquete = item.UnidadesPorPaquete,
            Activo = item.Activo
        });
    }

    [HttpPost]
    public async Task<ActionResult<SKUDto>> Create(SKUCreateDto dto)
    {
        if (await _context.SKUs.AnyAsync(s => s.CodigoSku == dto.CodigoSku))
            return Conflict("El código SKU ya existe.");

        if (!await _context.Productos.AnyAsync(p => p.IdProducto == dto.IdProducto))
            return BadRequest("El producto indicado no existe.");

        if (!await _context.Sabores.AnyAsync(s => s.IdSabor == dto.IdSabor))
            return BadRequest("El sabor indicado no existe.");

        var entity = new SKU
        {
            CodigoSku = dto.CodigoSku,
            IdProducto = dto.IdProducto,
            IdSabor = dto.IdSabor,
            GramajeG = dto.GramajeG,
            UnidadesPorPaquete = dto.UnidadesPorPaquete,
            Activo = dto.Activo
        };
        _context.SKUs.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByCodigo), new { codigo = entity.CodigoSku },
            new SKUDto
            {
                CodigoSku = entity.CodigoSku,
                IdProducto = entity.IdProducto,
                IdSabor = entity.IdSabor,
                GramajeG = entity.GramajeG,
                UnidadesPorPaquete = entity.UnidadesPorPaquete,
                Activo = entity.Activo
            });
    }

    [HttpPut("{codigo}")]
    public async Task<IActionResult> Update(string codigo, SKUUpdateDto dto)
    {
        var entity = await _context.SKUs.FindAsync(codigo);
        if (entity is null) return NotFound();

        if (!await _context.Productos.AnyAsync(p => p.IdProducto == dto.IdProducto))
            return BadRequest("El producto indicado no existe.");

        if (!await _context.Sabores.AnyAsync(s => s.IdSabor == dto.IdSabor))
            return BadRequest("El sabor indicado no existe.");

        entity.IdProducto = dto.IdProducto;
        entity.IdSabor = dto.IdSabor;
        entity.GramajeG = dto.GramajeG;
        entity.UnidadesPorPaquete = dto.UnidadesPorPaquete;
        entity.Activo = dto.Activo;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{codigo}")]
    public async Task<IActionResult> Delete(string codigo)
    {
        var entity = await _context.SKUs.FindAsync(codigo);
        if (entity is null) return NotFound();
        _context.SKUs.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
