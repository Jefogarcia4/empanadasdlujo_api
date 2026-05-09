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
public class CategoriasController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriasController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetAll()
    {
        var items = await _context.Categorias
            .Select(c => new CategoriaDto { IdCategoria = c.IdCategoria, Nombre = c.Nombre })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoriaDto>> GetById(int id)
    {
        var item = await _context.Categorias.FindAsync(id);
        if (item is null) return NotFound();
        return Ok(new CategoriaDto { IdCategoria = item.IdCategoria, Nombre = item.Nombre });
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaDto>> Create(CategoriaCreateDto dto)
    {
        var entity = new Categoria { Nombre = dto.Nombre };
        _context.Categorias.Add(entity);
        await _context.SaveChangesAsync();
        var result = new CategoriaDto { IdCategoria = entity.IdCategoria, Nombre = entity.Nombre };
        return CreatedAtAction(nameof(GetById), new { id = entity.IdCategoria }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CategoriaCreateDto dto)
    {
        var entity = await _context.Categorias.FindAsync(id);
        if (entity is null) return NotFound();
        entity.Nombre = dto.Nombre;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Categorias.FindAsync(id);
        if (entity is null) return NotFound();
        _context.Categorias.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
