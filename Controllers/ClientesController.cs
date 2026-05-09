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
public class ClientesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ClientesController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> GetAll([FromQuery] bool? activo = null)
    {
        var query = _context.Clientes.AsQueryable();

        if (activo.HasValue)
            query = query.Where(c => c.Activo == activo.Value);

        var items = await query.Select(c => new ClienteDto
        {
            IdCliente = c.IdCliente,
            Nombre = c.Nombre,
            Telefono = c.Telefono,
            Email = c.Email,
            Direccion = c.Direccion,
            Nit = c.Nit,
            Activo = c.Activo
        }).ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteDto>> GetById(int id)
    {
        var item = await _context.Clientes.FindAsync(id);
        if (item is null) return NotFound();
        return Ok(new ClienteDto
        {
            IdCliente = item.IdCliente,
            Nombre = item.Nombre,
            Telefono = item.Telefono,
            Email = item.Email,
            Direccion = item.Direccion,
            Nit = item.Nit,
            Activo = item.Activo
        });
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Create(ClienteCreateDto dto)
    {
        var entity = new Cliente
        {
            Nombre = dto.Nombre,
            Telefono = dto.Telefono,
            Email = dto.Email,
            Direccion = dto.Direccion,
            Nit = dto.Nit,
            Activo = dto.Activo
        };
        _context.Clientes.Add(entity);
        await _context.SaveChangesAsync();

        var result = new ClienteDto
        {
            IdCliente = entity.IdCliente,
            Nombre = entity.Nombre,
            Telefono = entity.Telefono,
            Email = entity.Email,
            Direccion = entity.Direccion,
            Nit = entity.Nit,
            Activo = entity.Activo
        };
        return CreatedAtAction(nameof(GetById), new { id = entity.IdCliente }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ClienteCreateDto dto)
    {
        var entity = await _context.Clientes.FindAsync(id);
        if (entity is null) return NotFound();

        entity.Nombre = dto.Nombre;
        entity.Telefono = dto.Telefono;
        entity.Email = dto.Email;
        entity.Direccion = dto.Direccion;
        entity.Nit = dto.Nit;
        entity.Activo = dto.Activo;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Clientes.FindAsync(id);
        if (entity is null) return NotFound();
        _context.Clientes.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
