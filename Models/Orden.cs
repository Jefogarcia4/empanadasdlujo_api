using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmpanadasDLujo.API.Models;

[Table("Orden")]
public class Orden
{
    [Key]
    [Column("id_orden")]
    public int IdOrden { get; set; }

    [Column("id_cliente")]
    public int IdCliente { get; set; }

    [Column("fecha_orden")]
    public DateTime FechaOrden { get; set; } = DateTime.Now;

    [Required]
    [MaxLength(20)]
    [Column("estado")]
    public string Estado { get; set; } = "PENDIENTE";

    [Column("subtotal", TypeName = "decimal(14,2)")]
    public decimal Subtotal { get; set; } = 0;

    [Column("descuento", TypeName = "decimal(14,2)")]
    public decimal Descuento { get; set; } = 0;

    [Column("total", TypeName = "decimal(14,2)")]
    public decimal Total { get; set; } = 0;

    [MaxLength(500)]
    [Column("observaciones")]
    public string? Observaciones { get; set; }

    [ForeignKey(nameof(IdCliente))]
    public Cliente Cliente { get; set; } = null!;

    public ICollection<OrdenDetalle> Detalles { get; set; } = new List<OrdenDetalle>();
}
