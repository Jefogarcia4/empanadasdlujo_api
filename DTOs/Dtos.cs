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

    // Solo se pueblan en el listado del admin (GetAll): no son columnas de la tabla.
    public int TotalPedidos { get; set; }
    public DateTime? UltimoPedido { get; set; }
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

// ─── Portal de clientes (login por OTP vía WhatsApp) ─────────
public class OtpSolicitarDto
{
    [Required]
    [MaxLength(20)]
    public string Telefono { get; set; } = string.Empty;
}

public class OtpSolicitarRespuestaDto
{
    public bool Enviado { get; set; }
    public DateTime ExpiraEn { get; set; }
    public string Mensaje { get; set; } = string.Empty;

    // Solo se llena en modo dev (Otp:DevReturnCode = true) para probar sin WhatsApp. Nunca en producción.
    public string? CodigoDev { get; set; }
}

public class OtpVerificarDto
{
    [Required]
    [MaxLength(20)]
    public string Telefono { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^[0-9]{6}$", ErrorMessage = "El código debe tener 6 dígitos.")]
    public string Codigo { get; set; } = string.Empty;
}

public class PortalSesionDto
{
    public string Token { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Nombre { get; set; }
    public DateTime ExpiraEn { get; set; }
}

// ─── Auth ────────────────────────────────────────────────────
public class LoginDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

// ─── Orden ───────────────────────────────────────────────────
public class OrdenDto
{
    public int IdOrden { get; set; }
    public int IdCliente { get; set; }
    public string? NombreCliente { get; set; }
    public string? ApellidosCliente { get; set; }
    public string? TelefonoCliente { get; set; }
    public string? EmailCliente { get; set; }
    public string? DireccionCliente { get; set; }
    public string? CasaApartamentoCliente { get; set; }
    public string? CiudadCliente { get; set; }
    public string? DepartamentoCliente { get; set; }
    public string? CodigoPostalCliente { get; set; }
    public string? PaisCliente { get; set; }
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
    public int? Margen { get; set; }
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
    public string? CodigoSku { get; set; }
    public int? IdCombo { get; set; }
    public string? CodigoCombo { get; set; }
    public string? NombreCombo { get; set; }
    public bool EsCombo { get; set; }
    public int CantidadPaquetes { get; set; }
    public decimal PrecioPaqueteDetal { get; set; }
    public decimal PrecioPaquete { get; set; }
    public decimal PrecioPorUnidad { get; set; }
    public bool AplicaMayorista { get; set; }
    public decimal? Subtotal { get; set; }
}

// Un detalle debe traer codigoSku O idCombo, nunca ambos (validado en el controlador).
public class OrdenDetalleCreateDto
{
    [MaxLength(20)]
    public string? CodigoSku { get; set; }

    public int? IdCombo { get; set; }

    [Required] [Range(1, int.MaxValue)]
    public int CantidadPaquetes { get; set; }
}

// ─── Combo ───────────────────────────────────────────────────
public class ComboComponenteDto
{
    public string CodigoSku { get; set; } = string.Empty;
    public string? Producto { get; set; }
    public string? Sabor { get; set; }
    public decimal GramajeG { get; set; }
    public int UnidadesPorPaquete { get; set; }
    public int CantidadPaquetes { get; set; }
}

public class ComboDto
{
    public int IdCombo { get; set; }
    public string CodigoCombo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Subcategoria { get; set; }
    public string? DescripcionCorta { get; set; }
    public string? DescripcionLarga { get; set; }
    public decimal PrecioNormal { get; set; }
    public decimal PrecioCombo { get; set; }
    public decimal Ahorro { get; set; }
    public decimal? PesoTotalG { get; set; }
    public int? UnidadesTotales { get; set; }
    public bool Activo { get; set; }
    public int? Orden { get; set; }
    public string? UrlImage { get; set; }
    public string? BadgeDescripcion { get; set; }
    public List<ComboComponenteDto> Componentes { get; set; } = new();
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

// ─── Carrito por WhatsApp (contrato simplificado: teléfono + nombre + items) ──
// Un item trae codigoSku O idCombo, nunca ambos ni ninguno (validado en el controlador).
public class PedidoWhatsAppItemDto
{
    [MaxLength(20)]
    public string? CodigoSku { get; set; }

    public int? IdCombo { get; set; }

    [Required] [Range(1, int.MaxValue)]
    public int Cantidad { get; set; }
}

public class PedidoWhatsAppCreateDto
{
    [Required] [MaxLength(20)]
    public string Telefono { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Nombre { get; set; }

    [MaxLength(100)]
    public string? Apellidos { get; set; }

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

    [MaxLength(500)]
    public string? Observaciones { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "El pedido debe tener al menos un producto.")]
    public List<PedidoWhatsAppItemDto> Items { get; set; } = new();
}

// Respuesta al crear el carrito borrador: token + link para abrir la web precargada.
public class CarritoCreatedDto
{
    public Guid Token { get; set; }
    public string Url { get; set; } = string.Empty;
}

// Vista del carrito borrador para precargar la página de checkout.
public class CarritoItemDto
{
    public string? CodigoSku { get; set; }
    public int? IdCombo { get; set; }
    public int Cantidad { get; set; }
}

public class CarritoDto
{
    public Guid Token { get; set; }
    public string Estado { get; set; } = string.Empty;   // ACTIVO | CONVERTIDO
    public int? IdOrden { get; set; }
    public string? Nombre { get; set; }
    public string? Apellidos { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? CasaApartamento { get; set; }
    public string? Ciudad { get; set; }
    public string? Departamento { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Pais { get; set; }
    public string? Observaciones { get; set; }
    public List<CarritoItemDto> Items { get; set; } = new();
}

public class CarritoConvertirDto
{
    [Required]
    public int IdOrden { get; set; }
}

// ─── Catálogo web (SKU + precios que se muestran en la web) ───────────────────
public class CatalogoWebDto
{
    public string CodigoSku { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Producto { get; set; } = string.Empty;
    public string Sabor { get; set; } = string.Empty;
    public decimal GramajeG { get; set; }
    public int UnidadesPorPaquete { get; set; }
    public decimal PrecioPaquete { get; set; }
    public decimal PrecioPorUnidad { get; set; }
    public decimal? PrecioPaqueteMayorista { get; set; }
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
    // Destinatario opcional. Si viene vacío se usa el número por defecto del servidor.
    public string? To { get; set; }

    // Atribución para el log de fallos (opcional): a qué orden pertenece y de qué tipo de mensaje.
    public int? IdOrden { get; set; }
    public string? Tipo { get; set; }   // NEGOCIO | CLIENTE

    [Required]
    public WhatsAppTemplateDto Template { get; set; } = new();
}

public class WhatsAppLogDto
{
    public int IdLog { get; set; }
    public int? IdOrden { get; set; }
    public string? Tipo { get; set; }
    public string? Destinatario { get; set; }
    public string? Plantilla { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int? CodigoHttp { get; set; }
    public string? MensajeError { get; set; }
    public DateTime FechaIntento { get; set; }
}
