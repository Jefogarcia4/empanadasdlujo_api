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
public class PedidosController : ControllerBase
{
    private readonly AppDbContext _context;

    public PedidosController(AppDbContext context) => _context = context;

    /// <summary>
    /// Crea un cliente nuevo, su orden y los detalles en una sola operación atómica.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PedidoDto>> Create(PedidoCreateDto dto)
    {
        // 1. Validar lista de precios (solo si se indicó)
        ListaPrecios? lista = null;
        if (dto.IdLista.HasValue)
        {
            lista = await _context.ListasPrecios.FindAsync(dto.IdLista.Value);
            if (lista is null)
                return BadRequest("La lista de precios indicada no existe.");
        }

        // 2. Validar que lleguen detalles
        if (dto.Detalles.Count == 0)
            return BadRequest("La orden debe tener al menos un detalle.");

        // 3. Validar que todos los SKUs tengan precio en la lista seleccionada (solo si hay lista)
        var skuCodigos = dto.Detalles.Select(d => d.CodigoSku).Distinct().ToList();
        var precios = dto.IdLista.HasValue
            ? await _context.PreciosSKU
                .Where(p => skuCodigos.Contains(p.CodigoSku) && p.IdLista == dto.IdLista)
                .ToDictionaryAsync(p => p.CodigoSku)
            : new Dictionary<string, Models.PrecioSKU>();

        if (dto.IdLista.HasValue)
        {
            var skusSinPrecio = skuCodigos.Except(precios.Keys).ToList();
            if (skusSinPrecio.Count > 0)
                return BadRequest($"Sin precio en la lista seleccionada: {string.Join(", ", skusSinPrecio)}.");
        }

        // 4. Crear cliente
        var cliente = new Cliente
        {
            Nombre    = dto.Cliente.Nombre,
            Telefono  = dto.Cliente.Telefono,
            Email     = dto.Cliente.Email,
            Direccion = dto.Cliente.Direccion,
            Nit       = dto.Cliente.Nit,
            Activo    = dto.Cliente.Activo
        };
        _context.Clientes.Add(cliente);

        // 5. Crear orden (EF resolverá el IdCliente tras SaveChanges por la relación)
        var orden = new Orden
        {
            Cliente      = cliente,
            IdLista      = dto.IdLista,
            FechaOrden   = DateTime.Now,
            Estado       = "PENDIENTE",
            Observaciones = dto.Observaciones
        };

        // 6. Crear detalles y calcular total
        decimal total = 0;
        foreach (var det in dto.Detalles)
        {
            var detalle = new OrdenDetalle
            {
                CodigoSku        = det.CodigoSku,
                CantidadPaquetes = det.CantidadPaquetes,
                PrecioPaquete    = precios.TryGetValue(det.CodigoSku, out var p) ? p.PrecioPaquete : 0,
                PrecioPorUnidad  = precios.TryGetValue(det.CodigoSku, out var pu) ? pu.PrecioPorUnidad : 0
            };
            orden.Detalles.Add(detalle);
            total += det.CantidadPaquetes * detalle.PrecioPaquete;
        }
        orden.Total = total;

        _context.Ordenes.Add(orden);

        // 7. Persistir todo en una sola transacción
        await _context.SaveChangesAsync();

        // 8. Construir respuesta
        var result = new PedidoDto
        {
            Cliente = new ClienteDto
            {
                IdCliente = cliente.IdCliente,
                Nombre    = cliente.Nombre,
                Telefono  = cliente.Telefono,
                Email     = cliente.Email,
                Direccion = cliente.Direccion,
                Nit       = cliente.Nit,
                Activo    = cliente.Activo
            },
            Orden = new OrdenDto
            {
                IdOrden      = orden.IdOrden,
                IdCliente    = orden.IdCliente,
                NombreCliente = cliente.Nombre,
                IdLista       = orden.IdLista,
                NombreLista   = lista?.Nombre,
                FechaOrden   = orden.FechaOrden,
                Estado       = orden.Estado,
                Total        = orden.Total,
                Observaciones = orden.Observaciones,
                Detalles     = orden.Detalles.Select(d => new OrdenDetalleDto
                {
                    IdDetalle       = d.IdDetalle,
                    CodigoSku       = d.CodigoSku,
                    CantidadPaquetes = d.CantidadPaquetes,
                    PrecioPaquete   = d.PrecioPaquete,
                    PrecioPorUnidad = d.PrecioPorUnidad,
                    Subtotal        = d.CantidadPaquetes * d.PrecioPaquete
                }).ToList()
            }
        };

        return CreatedAtAction(nameof(Create), new { id = orden.IdOrden }, result);
    }
}
