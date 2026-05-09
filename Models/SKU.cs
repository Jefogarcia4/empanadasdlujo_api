using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmpanadasDLujo.API.Models;

[Table("SKU")]
public class SKU
{
    [Key]
    [MaxLength(20)]
    [Column("codigo_sku")]
    public string CodigoSku { get; set; } = string.Empty;

    [Column("id_producto")]
    public int IdProducto { get; set; }

    [Column("id_sabor")]
    public int IdSabor { get; set; }

    [Required]
    [Column("gramaje_g", TypeName = "decimal(10,2)")]
    public decimal GramajeG { get; set; }

    [Required]
    [Column("unidades_por_paquete")]
    public int UnidadesPorPaquete { get; set; }

    [Column("activo")]
    public bool Activo { get; set; } = true;

    [ForeignKey(nameof(IdProducto))]
    public Producto Producto { get; set; } = null!;

    [ForeignKey(nameof(IdSabor))]
    public Sabor Sabor { get; set; } = null!;

    public ICollection<PrecioSKU> Precios { get; set; } = new List<PrecioSKU>();
    public ICollection<OrdenDetalle> OrdenDetalles { get; set; } = new List<OrdenDetalle>();
}
