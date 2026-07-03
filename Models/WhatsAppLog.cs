using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmpanadasDLujo.API.Models;

[Table("WhatsAppLog")]
public class WhatsAppLog
{
    [Key]
    [Column("id_log")]
    public int IdLog { get; set; }

    // Referencia suave a Orden (sin FK dura para no bloquear el borrado de órdenes PENDIENTE).
    [Column("id_orden")]
    public int? IdOrden { get; set; }

    [MaxLength(20)]
    [Column("tipo")]
    public string? Tipo { get; set; }   // NEGOCIO | CLIENTE

    [MaxLength(20)]
    [Column("destinatario")]
    public string? Destinatario { get; set; }

    [MaxLength(100)]
    [Column("plantilla")]
    public string? Plantilla { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("estado")]
    public string Estado { get; set; } = "FALLIDO";

    [Column("codigo_http")]
    public int? CodigoHttp { get; set; }

    [Column("mensaje_error")]
    public string? MensajeError { get; set; }

    [Column("payload_json")]
    public string? PayloadJson { get; set; }

    [Column("fecha_intento")]
    public DateTime FechaIntento { get; set; } = DateTime.Now;
}
