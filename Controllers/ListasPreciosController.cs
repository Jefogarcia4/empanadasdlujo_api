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
public class ListasPreciosController : ControllerBase
{
    private readonly AppDbContext _context;

    public ListasPreciosController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ListaPreciosDto>>> GetAll()
    {
        var items = await _context.ListasPrecios
            .Select(l => new ListaPreciosDto { IdLista = l.IdLista, Nombre = l.Nombre })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ListaPreciosDto>> GetById(int id)
    {
        var item = await _context.ListasPrecios.FindAsync(id);
        if (item is null) return NotFound();
        return Ok(new ListaPreciosDto { IdLista = item.IdLista, Nombre = item.Nombre });
    }

    [HttpPost]
    public async Task<ActionResult<ListaPreciosDto>> Create(ListaPreciosCreateDto dto)
    {
        var entity = new ListaPrecios { Nombre = dto.Nombre };
        _context.ListasPrecios.Add(entity);
        await _context.SaveChangesAsync();
        var result = new ListaPreciosDto { IdLista = entity.IdLista, Nombre = entity.Nombre };
        return CreatedAtAction(nameof(GetById), new { id = entity.IdLista }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ListaPreciosCreateDto dto)
    {
        var entity = await _context.ListasPrecios.FindAsync(id);
        if (entity is null) return NotFound();
        entity.Nombre = dto.Nombre;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.ListasPrecios.FindAsync(id);
        if (entity is null) return NotFound();
        _context.ListasPrecios.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
