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
public class OrdenesController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrdenesController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrdenDto>>> GetAll(
        [FromQuery] string? estado = null,
        [FromQuery] int? idCliente = null)
    {
        var query = _context.Ordenes
            .Include(o => o.Cliente)
            .Include(o => o.ListaPrecios)
            .Include(o => o.Detalles)
            .AsQueryable();

        if (!string.IsNullOrEmpty(estado))
            query = query.Where(o => o.Estado == estado);

        if (idCliente.HasValue)
            query = query.Where(o => o.IdCliente == idCliente.Value);

        var items = await query.Select(o => new OrdenDto
        {
            IdOrden = o.IdOrden,
            IdCliente = o.IdCliente,
            NombreCliente = o.Cliente.Nombre,
            IdLista = o.IdLista,
            NombreLista = o.ListaPrecios.Nombre,
            FechaOrden = o.FechaOrden,
            Estado = o.Estado,
            Total = o.Total,
            Observaciones = o.Observaciones,
            Detalles = o.Detalles.Select(d => new OrdenDetalleDto
            {
                IdDetalle = d.IdDetalle,
                CodigoSku = d.CodigoSku,
                CantidadPaquetes = d.CantidadPaquetes,
                PrecioPaquete = d.PrecioPaquete,
                PrecioPorUnidad = d.PrecioPorUnidad,
                Subtotal = d.Subtotal
            }).ToList()
        }).ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrdenDto>> GetById(int id)
    {
        var item = await _context.Ordenes
            .Include(o => o.Cliente)
            .Include(o => o.ListaPrecios)
            .Include(o => o.Detalles)
            .FirstOrDefaultAsync(o => o.IdOrden == id);

        if (item is null) return NotFound();

        return Ok(new OrdenDto
        {
            IdOrden = item.IdOrden,
            IdCliente = item.IdCliente,
            NombreCliente = item.Cliente?.Nombre,
            IdLista = item.IdLista,
            NombreLista = item.ListaPrecios?.Nombre,
            FechaOrden = item.FechaOrden,
            Estado = item.Estado,
            Total = item.Total,
            Observaciones = item.Observaciones,
            Detalles = item.Detalles.Select(d => new OrdenDetalleDto
            {
                IdDetalle = d.IdDetalle,
                CodigoSku = d.CodigoSku,
                CantidadPaquetes = d.CantidadPaquetes,
                PrecioPaquete = d.PrecioPaquete,
                PrecioPorUnidad = d.PrecioPorUnidad,
                Subtotal = d.Subtotal
            }).ToList()
        });
    }

    [HttpPost]
    public async Task<ActionResult<OrdenDto>> Create(OrdenCreateDto dto)
    {
        if (!await _context.Clientes.AnyAsync(c => c.IdCliente == dto.IdCliente))
            return BadRequest("El cliente indicado no existe.");

        if (!await _context.ListasPrecios.AnyAsync(l => l.IdLista == dto.IdLista))
            return BadRequest("La lista de precios indicada no existe.");

        if (dto.Detalles.Count == 0)
            return BadRequest("La orden debe tener al menos un detalle.");

        // Validate all SKUs and load their prices
        var skuCodigos = dto.Detalles.Select(d => d.CodigoSku).Distinct().ToList();
        var precios = await _context.PreciosSKU
            .Where(p => skuCodigos.Contains(p.CodigoSku) && p.IdLista == dto.IdLista)
            .ToDictionaryAsync(p => p.CodigoSku);

        foreach (var det in dto.Detalles)
        {
            if (!precios.ContainsKey(det.CodigoSku))
                return BadRequest($"No existe precio para el SKU '{det.CodigoSku}' en la lista seleccionada.");
        }

        var orden = new Orden
        {
            IdCliente = dto.IdCliente,
            IdLista = dto.IdLista,
            FechaOrden = DateTime.Now,
            Estado = "PENDIENTE",
            Observaciones = dto.Observaciones
        };

        decimal total = 0;
        foreach (var det in dto.Detalles)
        {
            var precio = precios[det.CodigoSku];
            var detalle = new OrdenDetalle
            {
                CodigoSku = det.CodigoSku,
                CantidadPaquetes = det.CantidadPaquetes,
                PrecioPaquete = precio.PrecioPaquete,
                PrecioPorUnidad = precio.PrecioPorUnidad
            };
            orden.Detalles.Add(detalle);
            total += det.CantidadPaquetes * precio.PrecioPaquete;
        }
        orden.Total = total;

        _context.Ordenes.Add(orden);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = orden.IdOrden },
            new OrdenDto
            {
                IdOrden = orden.IdOrden,
                IdCliente = orden.IdCliente,
                IdLista = orden.IdLista,
                FechaOrden = orden.FechaOrden,
                Estado = orden.Estado,
                Total = orden.Total,
                Observaciones = orden.Observaciones,
                Detalles = orden.Detalles.Select(d => new OrdenDetalleDto
                {
                    IdDetalle = d.IdDetalle,
                    CodigoSku = d.CodigoSku,
                    CantidadPaquetes = d.CantidadPaquetes,
                    PrecioPaquete = d.PrecioPaquete,
                    PrecioPorUnidad = d.PrecioPorUnidad,
                    Subtotal = d.CantidadPaquetes * d.PrecioPaquete
                }).ToList()
            });
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> UpdateEstado(int id, OrdenUpdateEstadoDto dto)
    {
        var entity = await _context.Ordenes.FindAsync(id);
        if (entity is null) return NotFound();

        entity.Estado = dto.Estado;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Ordenes
            .Include(o => o.Detalles)
            .FirstOrDefaultAsync(o => o.IdOrden == id);

        if (entity is null) return NotFound();
        if (entity.Estado != "PENDIENTE")
            return BadRequest("Solo se pueden eliminar órdenes en estado PENDIENTE.");

        _context.Ordenes.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
