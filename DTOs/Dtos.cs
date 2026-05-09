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
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? Nit { get; set; }
    public bool Activo { get; set; }
}

public class ClienteCreateDto
{
    [Required] [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(200)]
    public string? Direccion { get; set; }

    [MaxLength(200)]
    public string? Nit { get; set; }

    public bool Activo { get; set; } = true;
}

// ─── Orden ───────────────────────────────────────────────────
public class OrdenDto
{
    public int IdOrden { get; set; }
    public int IdCliente { get; set; }
    public string? NombreCliente { get; set; }
    public int? IdLista { get; set; }
    public string? NombreLista { get; set; }
    public DateTime FechaOrden { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string? Observaciones { get; set; }
    public List<OrdenDetalleDto> Detalles { get; set; } = new();
}

public class OrdenCreateDto
{
    [Required]
    public int IdCliente { get; set; }

    public int? IdLista { get; set; }

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
    public List<CatalogoPrecioDto> Precios { get; set; } = new();
}

// ─── OrdenDetalle ────────────────────────────────────────────
public class OrdenDetalleDto
{
    public int IdDetalle { get; set; }
    public string CodigoSku { get; set; } = string.Empty;
    public int CantidadPaquetes { get; set; }
    public decimal PrecioPaquete { get; set; }
    public decimal PrecioPorUnidad { get; set; }
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

    [Required]
    public int? IdLista { get; set; }

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
