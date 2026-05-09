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

    [Column("id_lista")]
    public int? IdLista { get; set; }

    [Column("fecha_orden")]
    public DateTime FechaOrden { get; set; } = DateTime.Now;

    [Required]
    [MaxLength(20)]
    [Column("estado")]
    public string Estado { get; set; } = "PENDIENTE";

    [Column("total", TypeName = "decimal(14,2)")]
    public decimal Total { get; set; } = 0;

    [MaxLength(500)]
    [Column("observaciones")]
    public string? Observaciones { get; set; }

    [ForeignKey(nameof(IdCliente))]
    public Cliente Cliente { get; set; } = null!;

    [ForeignKey(nameof(IdLista))]
    public ListaPrecios ListaPrecios { get; set; } = null!;

    public ICollection<OrdenDetalle> Detalles { get; set; } = new List<OrdenDetalle>();
}
