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
    private readonly IConfiguration _configuration;

    // Lista PVxD = precio detal (retail); lista PVxM = precio mayorista
    private const string LISTA_DETAL = "Web";
    private const string LISTA_MAYOR = "PVxM";
    private const int UMBRAL_MAYORISTA = 10;

    // Valor fijo del domicilio (COP). Siempre se suma al total de todo pedido con productos.
    private const decimal DELIVERY_FEE = 12000m;

    public PedidosController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

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

        // 0. Validar que cada detalle sea SKU o combo (nunca ambos ni ninguno)
        foreach (var d in dto.Detalles)
        {
            var tieneSku = !string.IsNullOrWhiteSpace(d.CodigoSku);
            var tieneCombo = d.IdCombo.HasValue;
            if (tieneSku == tieneCombo)
                return BadRequest("Cada detalle debe traer codigoSku o idCombo, nunca ambos ni ninguno.");
        }

        var skuDetalles = dto.Detalles.Where(d => !string.IsNullOrWhiteSpace(d.CodigoSku)).ToList();
        var comboDetalles = dto.Detalles.Where(d => d.IdCombo.HasValue).ToList();

        // 1. Resolver listas PVxD y PVxM
        var listaDetal = await _context.ListasPrecios.FirstOrDefaultAsync(l => l.Nombre == LISTA_DETAL);
        var listaMayor = await _context.ListasPrecios.FirstOrDefaultAsync(l => l.Nombre == LISTA_MAYOR);
        if (listaDetal is null)
            return BadRequest($"No existe la lista de precios '{LISTA_DETAL}'.");

        // 2. Cargar precios de los SKUs solicitados
        var skuCodigos = skuDetalles.Select(d => d.CodigoSku!).Distinct().ToList();
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

        // 2b. Cargar combos solicitados (activos)
        var comboIds = comboDetalles.Select(d => d.IdCombo!.Value).Distinct().ToList();
        var combos = await _context.Combos
            .Where(c => comboIds.Contains(c.IdCombo))
            .ToDictionaryAsync(c => c.IdCombo);

        var combosInvalidos = comboIds.Where(id => !combos.ContainsKey(id) || !combos[id].Activo).ToList();
        if (combosInvalidos.Count > 0)
            return BadRequest($"Combos inexistentes o inactivos: {string.Join(", ", combosInvalidos)}.");

        // 3. Aplicar regla mayorista: SOLO los SKUs sueltos cuentan para el umbral.
        //    Los combos tienen precio fijo y no participan de la regla.
        var totalPaquetes = skuDetalles.Sum(d => d.CantidadPaquetes);
        var calificaMayorista = totalPaquetes >= UMBRAL_MAYORISTA;

        // 4. Resolver cliente: si ya existe (por teléfono, luego email) se reutiliza y se
        //    actualiza solo con los datos entrantes que vengan con valor; si no, se crea.
        var telefono = string.IsNullOrWhiteSpace(dto.Cliente.Telefono) ? null : dto.Cliente.Telefono.Trim();
        var email    = string.IsNullOrWhiteSpace(dto.Cliente.Email) ? null : dto.Cliente.Email.Trim();

        Cliente? cliente = null;
        if (telefono is not null)
            cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Telefono == telefono);
        if (cliente is null && email is not null)
            cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Email == email);

        if (cliente is null)
        {
            cliente = new Cliente
            {
                Nombre          = dto.Cliente.Nombre,
                Apellidos       = dto.Cliente.Apellidos,
                Telefono        = telefono,
                Email           = email,
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
        }
        else
        {
            // Actualiza solo los campos que llegan con valor; no borra datos previos con vacíos.
            if (!string.IsNullOrWhiteSpace(dto.Cliente.Nombre))          cliente.Nombre          = dto.Cliente.Nombre.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Cliente.Apellidos))       cliente.Apellidos       = dto.Cliente.Apellidos.Trim();
            if (telefono is not null)                                     cliente.Telefono        = telefono;
            if (email is not null)                                        cliente.Email           = email;
            if (!string.IsNullOrWhiteSpace(dto.Cliente.Direccion))       cliente.Direccion       = dto.Cliente.Direccion.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Cliente.CasaApartamento)) cliente.CasaApartamento = dto.Cliente.CasaApartamento.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Cliente.Ciudad))          cliente.Ciudad          = dto.Cliente.Ciudad.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Cliente.Departamento))    cliente.Departamento    = dto.Cliente.Departamento.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Cliente.CodigoPostal))    cliente.CodigoPostal    = dto.Cliente.CodigoPostal.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Cliente.Pais))            cliente.Pais            = dto.Cliente.Pais.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Cliente.Nit))             cliente.Nit             = dto.Cliente.Nit.Trim();
            // El cliente vuelve a estar activo al hacer un nuevo pedido; conserva GuardarInfo si ya estaba en true.
            cliente.Activo = true;
            if (dto.Cliente.GuardarInfo) cliente.GuardarInfo = true;
        }

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

        // 6a. Detalles de SKU suelto (precio detal/mayorista según umbral)
        foreach (var det in skuDetalles)
        {
            var precioDetal = preciosDetal[det.CodigoSku!];
            var aplicaMayor = calificaMayorista
                && preciosMayor.TryGetValue(det.CodigoSku!, out var pm)
                && pm.PrecioPaquete > 0
                && pm.PrecioPaquete < precioDetal.PrecioPaquete;

            var precioPaqueteFinal = aplicaMayor
                ? preciosMayor[det.CodigoSku!].PrecioPaquete
                : precioDetal.PrecioPaquete;
            var precioUnidadFinal = aplicaMayor
                ? preciosMayor[det.CodigoSku!].PrecioPorUnidad
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

        // 6b. Detalles de combo (precio fijo; el ahorro frente al precio normal va como descuento)
        foreach (var det in comboDetalles)
        {
            var combo = combos[det.IdCombo!.Value];
            var detalle = new OrdenDetalle
            {
                IdCombo            = combo.IdCombo,
                CantidadPaquetes   = det.CantidadPaquetes,
                PrecioPaqueteDetal = combo.PrecioNormal,
                PrecioPaquete      = combo.PrecioCombo,
                PrecioPorUnidad    = combo.PrecioCombo,
                AplicaMayorista    = false
            };
            orden.Detalles.Add(detalle);

            subtotal += det.CantidadPaquetes * combo.PrecioNormal;
            total    += det.CantidadPaquetes * combo.PrecioCombo;
        }

        // El descuento se calcula sobre los productos; el domicilio se suma al total final.
        orden.Subtotal  = subtotal;
        orden.Descuento = subtotal - total;
        orden.Total     = total + DELIVERY_FEE;

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
                    IdCombo            = d.IdCombo,
                    CodigoCombo        = d.IdCombo.HasValue ? combos[d.IdCombo.Value].CodigoCombo : null,
                    NombreCombo        = d.IdCombo.HasValue ? combos[d.IdCombo.Value].Nombre : null,
                    EsCombo            = d.IdCombo.HasValue,
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

    /// <summary>
    /// Genera un carrito borrador desde el flujo de WhatsApp. Contrato simplificado:
    /// teléfono + nombre + items (SKU o combo con cantidad). NO crea Cliente ni Orden:
    /// guarda la info en <see cref="CarritoWhatsApp"/> y devuelve un link para que el
    /// comprador complete la compra en la web (allí se dispara el pixel de Meta y recién
    /// entonces se crea el pedido real vía <see cref="Create"/>).
    /// </summary>
    [HttpPost("whatsapp")]
    public async Task<ActionResult<CarritoCreatedDto>> CreateWhatsApp(PedidoWhatsAppCreateDto dto)
    {
        if (dto.Items.Count == 0)
            return BadRequest("El carrito debe tener al menos un producto.");

        // 0. Cada item debe traer SKU o combo, nunca ambos ni ninguno.
        foreach (var i in dto.Items)
        {
            var tieneSku = !string.IsNullOrWhiteSpace(i.CodigoSku);
            var tieneCombo = i.IdCombo.HasValue;
            if (tieneSku == tieneCombo)
                return BadRequest("Cada item debe traer codigoSku o idCombo, nunca ambos ni ninguno.");
        }

        // 1. Validar existencia para no guardar basura (SKU con precio Web; combo activo).
        var skuCodigos = dto.Items
            .Where(i => !string.IsNullOrWhiteSpace(i.CodigoSku))
            .Select(i => i.CodigoSku!)
            .Distinct()
            .ToList();

        if (skuCodigos.Count > 0)
        {
            var listaDetal = await _context.ListasPrecios.FirstOrDefaultAsync(l => l.Nombre == LISTA_DETAL);
            if (listaDetal is null)
                return BadRequest($"No existe la lista de precios '{LISTA_DETAL}'.");

            var skusConPrecio = await _context.PreciosSKU
                .Where(p => skuCodigos.Contains(p.CodigoSku) && p.IdLista == listaDetal.IdLista)
                .Select(p => p.CodigoSku)
                .ToListAsync();

            var skusInvalidos = skuCodigos.Except(skusConPrecio).ToList();
            if (skusInvalidos.Count > 0)
                return BadRequest($"SKUs inexistentes o sin precio detal: {string.Join(", ", skusInvalidos)}.");
        }

        var comboIds = dto.Items
            .Where(i => i.IdCombo.HasValue)
            .Select(i => i.IdCombo!.Value)
            .Distinct()
            .ToList();

        if (comboIds.Count > 0)
        {
            var combosActivos = await _context.Combos
                .Where(c => comboIds.Contains(c.IdCombo) && c.Activo)
                .Select(c => c.IdCombo)
                .ToListAsync();

            var combosInvalidos = comboIds.Except(combosActivos).ToList();
            if (combosInvalidos.Count > 0)
                return BadRequest($"Combos inexistentes o inactivos: {string.Join(", ", combosInvalidos)}.");
        }

        // 2. Crear el carrito borrador (token lo genera la base de datos por defecto, pero lo
        //    fijamos aquí para poder construir el link sin un segundo round-trip).
        var carrito = new CarritoWhatsApp
        {
            Token           = Guid.NewGuid(),
            Nombre          = string.IsNullOrWhiteSpace(dto.Nombre) ? null : dto.Nombre.Trim(),
            Apellidos       = dto.Apellidos,
            Telefono        = dto.Telefono,
            Email           = dto.Email,
            Direccion       = dto.Direccion,
            CasaApartamento = dto.CasaApartamento,
            Ciudad          = dto.Ciudad,
            Departamento    = dto.Departamento,
            CodigoPostal    = dto.CodigoPostal,
            Pais            = "Colombia",
            Observaciones   = dto.Observaciones,
            Estado          = "ACTIVO"
        };

        foreach (var i in dto.Items)
        {
            carrito.Detalles.Add(new CarritoWhatsAppDetalle
            {
                CodigoSku = string.IsNullOrWhiteSpace(i.CodigoSku) ? null : i.CodigoSku,
                IdCombo   = i.IdCombo,
                Cantidad  = i.Cantidad
            });
        }

        _context.CarritosWhatsApp.Add(carrito);
        await _context.SaveChangesAsync();

        // 3. Construir el link de la web precargada.
        var baseUrl = (_configuration["Frontend:BaseUrl"] ?? string.Empty).TrimEnd('/');
        var url = $"{baseUrl}/carrito/{carrito.Token}";

        return Ok(new CarritoCreatedDto { Token = carrito.Token, Url = url });
    }
}
