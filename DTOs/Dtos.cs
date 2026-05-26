using System.ComponentModel.DataAnnotations;

namespace EmpanadasDLujo.API.DTOs;

// ─── Categoria ───────────────────────────────────────────────
public class CategoriaDto
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class CategoriaCreateDto
{
    [Required] [MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;
}

// ─── Sabor ───────────────────────────────────────────────────
public class SaborDto
{
    public int IdSabor { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class SaborCreateDto
{
    [Required] [MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;
}

// ─── Producto ────────────────────────────────────────────────
public class ProductoDto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int IdCategoria { get; set; }
    public string? NombreCategoria { get; set; }
}

public class ProductoCreateDto
{
    [Required] [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public int IdCategoria { get; set; }
}

// ─── SKU ─────────────────────────────────────────────────────
public class SKUDto
{
    public string CodigoSku { get; set; } = string.Empty;
    public int IdProducto { get; set; }
    public string? NombreProducto { get; set; }
    public int IdSabor { get; set; }
    public string? NombreSabor { get; set; }
    public decimal GramajeG { get; set; }
    public int UnidadesPorPaquete { get; set; }
    public bool Activo { get; set; }
    public int? Orden { get; set; }
    public string? BadgeDescripcion { get; set; }
}

public class SKUCreateDto
{
    [Required] [MaxLength(20)]
    public string CodigoSku { get; set; } = string.Empty;

    [Required]
    public int IdProducto { get; set; }

    [Required]
    public int IdSabor { get; set; }

    [Required]
    public decimal GramajeG { get; set; }

    [Required]
    public int UnidadesPorPaquete { get; set; }

    public bool Activo { get; set; } = true;

    public int? Orden { get; set; }

    [MaxLength(100)]
    public string? BadgeDescripcion { get; set; }
}

public class SKUUpdateDto
{
    [Required]
    public int IdProducto { get; set; }

    [Required]
    public int IdSabor { get; set; }

    [Required]
    public decimal GramajeG { get; set; }

    [Required]
    public int UnidadesPorPaquete { get; set; }

    public bool Activo { get; set; }

    public int? Orden { get; set; }

    [MaxLength(100)]
    public string? BadgeDescripcion { get; set; }
}

// ─── ListaPrecios ────────────────────────────────────────────
public class ListaPreciosDto
{
    public int IdLista { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class ListaPreciosCreateDto
{
    [Required] [MaxLength(20)]
    public string Nombre { get; set; } = string.Empty;
}

// ─── PrecioSKU ───────────────────────────────────────────────
public class PrecioSKUDto
{
    public int IdPrecio { get; set; }
    public string CodigoSku { get; set; } = string.Empty;
    public int IdLista { get; set; }
    public string? NombreLista { get; set; }
    public decimal PrecioPaquete { get; set; }
    public decimal PrecioPorUnidad { get; set; }
}

public class PrecioSKUCreateDto
{
    [Required] [MaxLength(20)]
    public string CodigoSku { get; set; } = string.Empty;

    [Required]
    public int IdLista { get; set; }

    [Required]
    public decimal PrecioPaquete { get; set; }

    [Required]
    public decimal PrecioPorUnidad { get; set; }
}

public class PrecioSKUUpdateDto
{
    [Required]
    public decimal PrecioPaquete { get; set; }

    [Required]
    public decimal PrecioPorUnidad { get; set; }
}

// ─── Cliente ─────────────────────────────────────────────────
public class ClienteDto
{
    public int IdCliente { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Apellidos { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? CasaApartamento { get; set; }
    public string? Ciudad { get; set; }
    public string? Departamento { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Pais { get; set; }
    public string? Nit { get; set; }
    public bool Activo { get; set; }
    public bool GuardarInfo { get; set; }
}

public class ClienteCreateDto
{
    [Required] [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Apellidos { get; set; }

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(200)]
    public string? Direccion { get; set; }

    [MaxLength(100)]
    public string? CasaApartamento { get; set; }

    [MaxLength(100)]
    public string? Ciudad { get; set; }

    [MaxLength(100)]
    public string? Departamento { get; set; }

    [MaxLength(20)]
    public string? CodigoPostal { get; set; }

    [MaxLength(50)]
    public string? Pais { get; set; } = "Colombia";

    [MaxLength(200)]
    public string? Nit { get; set; }

    public bool Activo { get; set; } = true;

    public bool GuardarInfo { get; set; } = false;
}

// ─── Orden ───────────────────────────────────────────────────
public class OrdenDto
{
    public int IdOrden { get; set; }
    public int IdCliente { get; set; }
    public string? NombreCliente { get; set; }
    public DateTime FechaOrden { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Total { get; set; }
    public string? Observaciones { get; set; }
    public List<OrdenDetalleDto> Detalles { get; set; } = new();
}

public class OrdenCreateDto
{
    [Required]
    public int IdCliente { get; set; }

    [MaxLength(500)]
    public string? Observaciones { get; set; }

    [Required]
    public List<OrdenDetalleCreateDto> Detalles { get; set; } = new();
}

public class OrdenUpdateEstadoDto
{
    [Required]
    [RegularExpression("PENDIENTE|CONFIRMADA|ENTREGADA|ANULADA")]
    public string Estado { get; set; } = string.Empty;
}

// ─── Catálogo (vista completa SKU + precios) ─────────────────
public class CatalogoPrecioDto
{
    public string NombreLista { get; set; } = string.Empty;
    public decimal PrecioPaquete { get; set; }
    public decimal PrecioPorUnidad { get; set; }
}

public class CatalogoSkuDto
{
    public string CodigoSku { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Producto { get; set; } = string.Empty;
    public string Sabor { get; set; } = string.Empty;
    public decimal GramajeG { get; set; }
    public int UnidadesPorPaquete { get; set; }
    public bool Activo { get; set; }
    public string? UrlImage { get; set; }
    public int? Orden { get; set; }
    public string? BadgeDescripcion { get; set; }
    public List<CatalogoPrecioDto> Precios { get; set; } = new();
}

// ─── OrdenDetalle ────────────────────────────────────────────
public class OrdenDetalleDto
{
    public int IdDetalle { get; set; }
    public string CodigoSku { get; set; } = string.Empty;
    public int CantidadPaquetes { get; set; }
    public decimal PrecioPaqueteDetal { get; set; }
    public decimal PrecioPaquete { get; set; }
    public decimal PrecioPorUnidad { get; set; }
    public bool AplicaMayorista { get; set; }
    public decimal? Subtotal { get; set; }
}

public class OrdenDetalleCreateDto
{
    [Required] [MaxLength(20)]
    public string CodigoSku { get; set; } = string.Empty;

    [Required] [Range(1, int.MaxValue)]
    public int CantidadPaquetes { get; set; }
}

// ─── Pedido Completo (Cliente nuevo + Orden + Detalles) ──────
public class PedidoCreateDto
{
    [Required]
    public ClienteCreateDto Cliente { get; set; } = null!;

    [MaxLength(500)]
    public string? Observaciones { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "La orden debe tener al menos un detalle.")]
    public List<OrdenDetalleCreateDto> Detalles { get; set; } = new();
}

public class PedidoDto
{
    public ClienteDto Cliente { get; set; } = null!;
    public OrdenDto Orden { get; set; } = null!;
}

// ─── WhatsApp ────────────────────────────────────────────────
public class WhatsAppTemplateLanguageDto
{
    [Required]
    public string Code { get; set; } = string.Empty;
}

public class WhatsAppTemplateComponentParameterDto
{
    [Required]
    public string Type { get; set; } = string.Empty;
    public string? Text { get; set; }
}

public class WhatsAppTemplateComponentDto
{
    [Required]
    public string Type { get; set; } = string.Empty;
    public List<WhatsAppTemplateComponentParameterDto>? Parameters { get; set; }
}

public class WhatsAppTemplateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public WhatsAppTemplateLanguageDto Language { get; set; } = new();

    public List<WhatsAppTemplateComponentDto>? Components { get; set; }
}

public class WhatsAppSendMessageDto
{
    [Required]
    public WhatsAppTemplateDto Template { get; set; } = new();
}
