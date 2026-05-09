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
public class SaboresController : ControllerBase
{
    private readonly AppDbContext _context;

    public SaboresController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SaborDto>>> GetAll()
    {
        var items = await _context.Sabores
            .Select(s => new SaborDto { IdSabor = s.IdSabor, Nombre = s.Nombre })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SaborDto>> GetById(int id)
    {
        var item = await _context.Sabores.FindAsync(id);
        if (item is null) return NotFound();
        return Ok(new SaborDto { IdSabor = item.IdSabor, Nombre = item.Nombre });
    }

    [HttpPost]
    public async Task<ActionResult<SaborDto>> Create(SaborCreateDto dto)
    {
        var entity = new Sabor { Nombre = dto.Nombre };
        _context.Sabores.Add(entity);
        await _context.SaveChangesAsync();
        var result = new SaborDto { IdSabor = entity.IdSabor, Nombre = entity.Nombre };
        return CreatedAtAction(nameof(GetById), new { id = entity.IdSabor }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaborCreateDto dto)
    {
        var entity = await _context.Sabores.FindAsync(id);
        if (entity is null) return NotFound();
        entity.Nombre = dto.Nombre;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Sabores.FindAsync(id);
        if (entity is null) return NotFound();
        _context.Sabores.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
