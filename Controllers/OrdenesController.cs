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
    private const string LISTA_DETAL = "Web";
    private const string LISTA_MAYOR = "PVxM";
    private const int UMBRAL_MAYORISTA = 10;

    private readonly AppDbContext _context;

    public OrdenesController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrdenDto>>> GetAll(
        [FromQuery] string? estado = null,
        [FromQuery] int? idCliente = null)
    {
        var query = _context.Ordenes
            .Include(o => o.Cliente)
            .Include(o => o.Detalles)
            .AsQueryable();

        if (!string.IsNullOrEmpty(estado))
            query = query.Where(o => o.Estado == estado);

        if (idCliente.HasValue)
            query = query.Where(o => o.IdCliente == idCliente.Value);

        var items = await query.Select(o => new OrdenDto
        {
            IdOrden       = o.IdOrden,
            IdCliente     = o.IdCliente,
            NombreCliente = o.Cliente.Nombre,
            FechaOrden    = o.FechaOrden,
            Estado        = o.Estado,
            Subtotal      = o.Subtotal,
            Descuento     = o.Descuento,
            Total         = o.Total,
            Observaciones = o.Observaciones,
            Detalles = o.Detalles.Select(d => new OrdenDetalleDto
            {
                IdDetalle          = d.IdDetalle,
                CodigoSku          = d.CodigoSku,
                CantidadPaquetes   = d.CantidadPaquetes,
                PrecioPaqueteDetal = d.PrecioPaqueteDetal,
                PrecioPaquete      = d.PrecioPaquete,
                PrecioPorUnidad    = d.PrecioPorUnidad,
                AplicaMayorista    = d.AplicaMayorista,
                Subtotal           = d.Subtotal
            }).ToList()
        }).ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrdenDto>> GetById(int id)
    {
        var item = await _context.Ordenes
            .Include(o => o.Cliente)
            .Include(o => o.Detalles)
            .FirstOrDefaultAsync(o => o.IdOrden == id);

        if (item is null) return NotFound();

        return Ok(new OrdenDto
        {
            IdOrden       = item.IdOrden,
            IdCliente     = item.IdCliente,
            NombreCliente = item.Cliente?.Nombre,
            FechaOrden    = item.FechaOrden,
            Estado        = item.Estado,
            Subtotal      = item.Subtotal,
            Descuento     = item.Descuento,
            Total         = item.Total,
            Observaciones = item.Observaciones,
            Detalles = item.Detalles.Select(d => new OrdenDetalleDto
            {
                IdDetalle          = d.IdDetalle,
                CodigoSku          = d.CodigoSku,
                CantidadPaquetes   = d.CantidadPaquetes,
                PrecioPaqueteDetal = d.PrecioPaqueteDetal,
                PrecioPaquete      = d.PrecioPaquete,
                PrecioPorUnidad    = d.PrecioPorUnidad,
                AplicaMayorista    = d.AplicaMayorista,
                Subtotal           = d.Subtotal
            }).ToList()
        });
    }

    [HttpPost]
    public async Task<ActionResult<OrdenDto>> Create(OrdenCreateDto dto)
    {
        if (!await _context.Clientes.AnyAsync(c => c.IdCliente == dto.IdCliente))
            return BadRequest("El cliente indicado no existe.");

        if (dto.Detalles.Count == 0)
            return BadRequest("La orden debe tener al menos un detalle.");

        var listaDetal = await _context.ListasPrecios.FirstOrDefaultAsync(l => l.Nombre == LISTA_DETAL);
        var listaMayor = await _context.ListasPrecios.FirstOrDefaultAsync(l => l.Nombre == LISTA_MAYOR);
        if (listaDetal is null)
            return BadRequest($"No existe la lista de precios '{LISTA_DETAL}'.");

        var skuCodigos = dto.Detalles.Select(d => d.CodigoSku).Distinct().ToList();
        var preciosDetal = await _context.PreciosSKU
            .Where(p => skuCodigos.Contains(p.CodigoSku) && p.IdLista == listaDetal.IdLista)
            .ToDictionaryAsync(p => p.CodigoSku);

        var skusSinDetal = skuCodigos.Except(preciosDetal.Keys).ToList();
        if (skusSinDetal.Count > 0)
            return BadRequest($"Sin precio detal en '{LISTA_DETAL}': {string.Join(", ", skusSinDetal)}.");

        var preciosMayor = listaMayor is null
            ? new Dictionary<string, PrecioSKU>()
            : await _context.PreciosSKU
                .Where(p => skuCodigos.Contains(p.CodigoSku) && p.IdLista == listaMayor.IdLista)
                .ToDictionaryAsync(p => p.CodigoSku);

        var totalPaquetes = dto.Detalles.Sum(d => d.CantidadPaquetes);
        var calificaMayorista = totalPaquetes >= UMBRAL_MAYORISTA;

        var orden = new Orden
        {
            IdCliente     = dto.IdCliente,
            FechaOrden    = DateTime.Now,
            Estado        = "PENDIENTE",
            Observaciones = dto.Observaciones
        };

        decimal subtotal = 0;
        decimal total = 0;
        foreach (var det in dto.Detalles)
        {
            var precioDetal = preciosDetal[det.CodigoSku];
            var aplicaMayor = calificaMayorista
                && preciosMayor.TryGetValue(det.CodigoSku, out var pm)
                && pm.PrecioPaquete > 0
                && pm.PrecioPaquete < precioDetal.PrecioPaquete;

            var precioPaqueteFinal = aplicaMayor
                ? preciosMayor[det.CodigoSku].PrecioPaquete
                : precioDetal.PrecioPaquete;
            var precioUnidadFinal = aplicaMayor
                ? preciosMayor[det.CodigoSku].PrecioPorUnidad
                : precioDetal.PrecioPorUnidad;

            var detalle = new OrdenDetalle
            {
                CodigoSku          = det.CodigoSku,
                CantidadPaquetes   = det.CantidadPaquetes,
                PrecioPaqueteDetal = precioDetal.PrecioPaquete,
                PrecioPaquete      = precioPaqueteFinal,
                PrecioPorUnidad    = precioUnidadFinal,
                AplicaMayorista    = aplicaMayor
            };
            orden.Detalles.Add(detalle);

            subtotal += det.CantidadPaquetes * precioDetal.PrecioPaquete;
            total    += det.CantidadPaquetes * precioPaqueteFinal;
        }

        orden.Subtotal  = subtotal;
        orden.Total     = total;
        orden.Descuento = subtotal - total;

        _context.Ordenes.Add(orden);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = orden.IdOrden },
            new OrdenDto
            {
                IdOrden       = orden.IdOrden,
                IdCliente     = orden.IdCliente,
                FechaOrden    = orden.FechaOrden,
                Estado        = orden.Estado,
                Subtotal      = orden.Subtotal,
                Descuento     = orden.Descuento,
                Total         = orden.Total,
                Observaciones = orden.Observaciones,
                Detalles      = orden.Detalles.Select(d => new OrdenDetalleDto
                {
                    IdDetalle          = d.IdDetalle,
                    CodigoSku          = d.CodigoSku,
                    CantidadPaquetes   = d.CantidadPaquetes,
                    PrecioPaqueteDetal = d.PrecioPaqueteDetal,
                    PrecioPaquete      = d.PrecioPaquete,
                    PrecioPorUnidad    = d.PrecioPorUnidad,
                    AplicaMayorista    = d.AplicaMayorista,
                    Subtotal           = d.CantidadPaquetes * d.PrecioPaquete
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
