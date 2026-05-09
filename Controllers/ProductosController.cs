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
public class ProductosController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductosController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductoDto>>> GetAll()
    {
        var items = await _context.Productos
            .Include(p => p.Categoria)
            .Select(p => new ProductoDto
            {
                IdProducto = p.IdProducto,
                Nombre = p.Nombre,
                IdCategoria = p.IdCategoria,
                NombreCategoria = p.Categoria.Nombre
            })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoDto>> GetById(int id)
    {
        var item = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.IdProducto == id);
        if (item is null) return NotFound();
        return Ok(new ProductoDto
        {
            IdProducto = item.IdProducto,
            Nombre = item.Nombre,
            IdCategoria = item.IdCategoria,
            NombreCategoria = item.Categoria?.Nombre
        });
    }

    [HttpPost]
    public async Task<ActionResult<ProductoDto>> Create(ProductoCreateDto dto)
    {
        if (!await _context.Categorias.AnyAsync(c => c.IdCategoria == dto.IdCategoria))
            return BadRequest("La categoría indicada no existe.");

        var entity = new Producto { Nombre = dto.Nombre, IdCategoria = dto.IdCategoria };
        _context.Productos.Add(entity);
        await _context.SaveChangesAsync();
        await _context.Entry(entity).Reference(p => p.Categoria).LoadAsync();

        var result = new ProductoDto
        {
            IdProducto = entity.IdProducto,
            Nombre = entity.Nombre,
            IdCategoria = entity.IdCategoria,
            NombreCategoria = entity.Categoria?.Nombre
        };
        return CreatedAtAction(nameof(GetById), new { id = entity.IdProducto }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProductoCreateDto dto)
    {
        var entity = await _context.Productos.FindAsync(id);
        if (entity is null) return NotFound();

        if (!await _context.Categorias.AnyAsync(c => c.IdCategoria == dto.IdCategoria))
            return BadRequest("La categoría indicada no existe.");

        entity.Nombre = dto.Nombre;
        entity.IdCategoria = dto.IdCategoria;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Productos.FindAsync(id);
        if (entity is null) return NotFound();
        _context.Productos.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
