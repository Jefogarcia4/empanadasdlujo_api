using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmpanadasDLujo.API.Models;

[Table("Orden_Detalle")]
public class OrdenDetalle
{
    [Key]
    [Column("id_detalle")]
    public int IdDetalle { get; set; }

    [Column("id_orden")]
    public int IdOrden { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("codigo_sku")]
    public string CodigoSku { get; set; } = string.Empty;

    [Column("cantidad_paquetes")]
    public int CantidadPaquetes { get; set; }

    [Required]
    [Column("precio_paquete", TypeName = "decimal(12,2)")]
    public decimal PrecioPaquete { get; set; }

    [Required]
    [Column("precio_por_unidad", TypeName = "decimal(12,2)")]
    public decimal PrecioPorUnidad { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [Column("subtotal", TypeName = "decimal(12,2)")]
    public decimal? Subtotal { get; set; }

    [ForeignKey(nameof(IdOrden))]
    public Orden Orden { get; set; } = null!;

    [ForeignKey(nameof(CodigoSku))]
    public SKU SKU { get; set; } = null!;
}
