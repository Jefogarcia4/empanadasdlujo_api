using EmpanadasDLujo.API.Data;
using EmpanadasDLujo.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmpanadasDLujo.API.Controllers;

// Carrito borrador generado desde WhatsApp. La página /carrito/{token} de la web lo consume
// para precargar el checkout; al completar la compra marca el carrito como CONVERTIDO.
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CarritoController : ControllerBase
{
    private readonly AppDbContext _context;

    public CarritoController(AppDbContext context) => _context = context;

    // GET api/carrito/{token} — devuelve el borrador para precargar la web. 404 si no existe.
    [HttpGet("{token:guid}")]
    public async Task<ActionResult<CarritoDto>> Get(Guid token)
    {
        var carrito = await _context.CarritosWhatsApp
            .Include(c => c.Detalles)
            .FirstOrDefaultAsync(c => c.Token == token);

        if (carrito is null)
            return NotFound();

        return Ok(new CarritoDto
        {
            Token           = carrito.Token,
            Estado          = carrito.Estado,
            IdOrden         = carrito.IdOrden,
            Nombre          = carrito.Nombre,
            Apellidos       = carrito.Apellidos,
            Telefono        = carrito.Telefono,
            Email           = carrito.Email,
            Direccion       = carrito.Direccion,
            CasaApartamento = carrito.CasaApartamento,
            Ciudad          = carrito.Ciudad,
            Departamento    = carrito.Departamento,
            CodigoPostal    = carrito.CodigoPostal,
            Pais            = carrito.Pais,
            Observaciones   = carrito.Observaciones,
            Items = carrito.Detalles.Select(d => new CarritoItemDto
            {
                CodigoSku = d.CodigoSku,
                IdCombo   = d.IdCombo,
                Cantidad  = d.Cantidad
            }).ToList()
        });
    }

    // POST api/carrito/{token}/convertir — marca el carrito como CONVERTIDO y guarda la orden
    // creada. Idempotente: si ya estaba convertido, responde 200 sin re-escribir.
    [HttpPost("{token:guid}/convertir")]
    public async Task<IActionResult> Convertir(Guid token, CarritoConvertirDto dto)
    {
        var carrito = await _context.CarritosWhatsApp.FirstOrDefaultAsync(c => c.Token == token);
        if (carrito is null)
            return NotFound();

        if (carrito.Estado != "CONVERTIDO")
        {
            carrito.Estado          = "CONVERTIDO";
            carrito.IdOrden         = dto.IdOrden;
            carrito.FechaConversion = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        return Ok(new { carrito.Token, carrito.Estado, carrito.IdOrden });
    }
}
