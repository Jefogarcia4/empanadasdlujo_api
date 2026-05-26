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

    private static ClienteDto ToDto(Cliente c) => new()
    {
        IdCliente = c.IdCliente,
        Nombre = c.Nombre,
        Apellidos = c.Apellidos,
        Telefono = c.Telefono,
        Email = c.Email,
        Direccion = c.Direccion,
        CasaApartamento = c.CasaApartamento,
        Ciudad = c.Ciudad,
        Departamento = c.Departamento,
        CodigoPostal = c.CodigoPostal,
        Pais = c.Pais,
        Nit = c.Nit,
        Activo = c.Activo,
        GuardarInfo = c.GuardarInfo
    };

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
            Apellidos = c.Apellidos,
            Telefono = c.Telefono,
            Email = c.Email,
            Direccion = c.Direccion,
            CasaApartamento = c.CasaApartamento,
            Ciudad = c.Ciudad,
            Departamento = c.Departamento,
            CodigoPostal = c.CodigoPostal,
            Pais = c.Pais,
            Nit = c.Nit,
            Activo = c.Activo,
            GuardarInfo = c.GuardarInfo
        }).ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteDto>> GetById(int id)
    {
        var item = await _context.Clientes.FindAsync(id);
        if (item is null) return NotFound();
        return Ok(ToDto(item));
    }

    [HttpPost]
    public async Task<ActionResult<ClienteDto>> Create(ClienteCreateDto dto)
    {
        var entity = new Cliente
        {
            Nombre = dto.Nombre,
            Apellidos = dto.Apellidos,
            Telefono = dto.Telefono,
            Email = dto.Email,
            Direccion = dto.Direccion,
            CasaApartamento = dto.CasaApartamento,
            Ciudad = dto.Ciudad,
            Departamento = dto.Departamento,
            CodigoPostal = dto.CodigoPostal,
            Pais = dto.Pais ?? "Colombia",
            Nit = dto.Nit,
            Activo = dto.Activo,
            GuardarInfo = dto.GuardarInfo
        };
        _context.Clientes.Add(entity);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.IdCliente }, ToDto(entity));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ClienteCreateDto dto)
    {
        var entity = await _context.Clientes.FindAsync(id);
        if (entity is null) return NotFound();

        entity.Nombre = dto.Nombre;
        entity.Apellidos = dto.Apellidos;
        entity.Telefono = dto.Telefono;
        entity.Email = dto.Email;
        entity.Direccion = dto.Direccion;
        entity.CasaApartamento = dto.CasaApartamento;
        entity.Ciudad = dto.Ciudad;
        entity.Departamento = dto.Departamento;
        entity.CodigoPostal = dto.CodigoPostal;
        entity.Pais = dto.Pais ?? "Colombia";
        entity.Nit = dto.Nit;
        entity.Activo = dto.Activo;
        entity.GuardarInfo = dto.GuardarInfo;
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
