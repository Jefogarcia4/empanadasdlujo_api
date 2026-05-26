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

    // Lista PVxD = precio detal (retail); lista PVxM = precio mayorista
    private const string LISTA_DETAL = "Web";
    private const string LISTA_MAYOR = "PVxM";
    private const int UMBRAL_MAYORISTA = 10;

    public PedidosController(AppDbContext context) => _context = context;

    /// <summary>
    /// Crea cliente + orden + detalles en una sola transacción.
    /// El cliente envía solo SKU y cantidad. El API consulta los precios PVxD y PVxM
    /// y aplica la regla: si total de paquetes >= 10 → todos a PVxM.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PedidoDto>> Create(PedidoCreateDto dto)
    {
        if (dto.Detalles.Count == 0)
            return BadRequest("La orden debe tener al menos un detalle.");

        // 1. Resolver listas PVxD y PVxM
        var listaDetal = await _context.ListasPrecios.FirstOrDefaultAsync(l => l.Nombre == LISTA_DETAL);
        var listaMayor = await _context.ListasPrecios.FirstOrDefaultAsync(l => l.Nombre == LISTA_MAYOR);
        if (listaDetal is null)
            return BadRequest($"No existe la lista de precios '{LISTA_DETAL}'.");

        // 2. Cargar precios de los SKUs solicitados
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

        // 3. Aplicar regla mayorista: total de paquetes >= UMBRAL
        var totalPaquetes = dto.Detalles.Sum(d => d.CantidadPaquetes);
        var calificaMayorista = totalPaquetes >= UMBRAL_MAYORISTA;

        // 4. Crear cliente
        var cliente = new Cliente
        {
            Nombre          = dto.Cliente.Nombre,
            Apellidos       = dto.Cliente.Apellidos,
            Telefono        = dto.Cliente.Telefono,
            Email           = dto.Cliente.Email,
            Direccion       = dto.Cliente.Direccion,
            CasaApartamento = dto.Cliente.CasaApartamento,
            Ciudad          = dto.Cliente.Ciudad,
            Departamento    = dto.Cliente.Departamento,
            CodigoPostal    = dto.Cliente.CodigoPostal,
            Pais            = dto.Cliente.Pais ?? "Colombia",
            Nit             = dto.Cliente.Nit,
            Activo          = dto.Cliente.Activo,
            GuardarInfo     = dto.Cliente.GuardarInfo
        };
        _context.Clientes.Add(cliente);

        // 5. Crear orden
        var orden = new Orden
        {
            Cliente       = cliente,
            FechaOrden    = DateTime.Now,
            Estado        = "PENDIENTE",
            Observaciones = dto.Observaciones
        };

        // 6. Crear detalles y acumular totales
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

        // 7. Respuesta
        var result = new PedidoDto
        {
            Cliente = new ClienteDto
            {
                IdCliente       = cliente.IdCliente,
                Nombre          = cliente.Nombre,
                Apellidos       = cliente.Apellidos,
                Telefono        = cliente.Telefono,
                Email           = cliente.Email,
                Direccion       = cliente.Direccion,
                CasaApartamento = cliente.CasaApartamento,
                Ciudad          = cliente.Ciudad,
                Departamento    = cliente.Departamento,
                CodigoPostal    = cliente.CodigoPostal,
                Pais            = cliente.Pais,
                Nit             = cliente.Nit,
                Activo          = cliente.Activo,
                GuardarInfo     = cliente.GuardarInfo
            },
            Orden = new OrdenDto
            {
                IdOrden       = orden.IdOrden,
                IdCliente     = orden.IdCliente,
                NombreCliente = cliente.Nombre,
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
            }
        };

        return CreatedAtAction(nameof(Create), new { id = orden.IdOrden }, result);
    }
}
