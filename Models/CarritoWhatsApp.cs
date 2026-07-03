using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmpanadasDLujo.API.Models;

// Carrito borrador generado desde el flujo de WhatsApp. Guarda la info del cliente y los
// items SIN crear Cliente ni Orden: la conversión a pedido real ocurre en el navegador del
// comprador (para que dispare el pixel de Meta) cuando abre el link /carrito/{token}.
[Table("CarritoWhatsApp")]
public class CarritoWhatsApp
{
    [Key]
    [Column("id_carrito")]
    public int IdCarrito { get; set; }

    // Identificador opaco del link (no se expone el int). Default NEWID() en base de datos.
    [Column("token")]
    public Guid Token { get; set; }

    // ── Datos del cliente (copiados de Cliente; sin crear el registro todavía) ──
    [MaxLength(100)]
    [Column("nombre")]
    public string? Nombre { get; set; }

    [MaxLength(100)]
    [Column("apellidos")]
    public string? Apellidos { get; set; }

    [MaxLength(20)]
    [Column("telefono")]
    public string? Telefono { get; set; }

    [MaxLength(100)]
    [Column("email")]
    public string? Email { get; set; }

    [MaxLength(200)]
    [Column("direccion")]
    public string? Direccion { get; set; }

    [MaxLength(100)]
    [Column("casa_apartamento")]
    public string? CasaApartamento { get; set; }

    [MaxLength(100)]
    [Column("ciudad")]
    public string? Ciudad { get; set; }

    [MaxLength(100)]
    [Column("departamento")]
    public string? Departamento { get; set; }

    [MaxLength(20)]
    [Column("codigo_postal")]
    public string? CodigoPostal { get; set; }

    [MaxLength(50)]
    [Column("pais")]
    public string? Pais { get; set; } = "Colombia";

    [MaxLength(500)]
    [Column("observaciones")]
    public string? Observaciones { get; set; }

    // ── Estado / conversión ──
    [Required]
    [MaxLength(20)]
    [Column("estado")]
    public string Estado { get; set; } = "ACTIVO";   // ACTIVO | CONVERTIDO

    // Referencia suave a la Orden creada al convertir (sin FK dura).
    [Column("id_orden")]
    public int? IdOrden { get; set; }

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Column("fecha_conversion")]
    public DateTime? FechaConversion { get; set; }

    public ICollection<CarritoWhatsAppDetalle> Detalles { get; set; } = new List<CarritoWhatsAppDetalle>();
}
