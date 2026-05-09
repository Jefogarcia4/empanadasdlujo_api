using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmpanadasDLujo.API.Models;

[Table("Precio_SKU")]
public class PrecioSKU
{
    [Key]
    [Column("id_precio")]
    public int IdPrecio { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("codigo_sku")]
    public string CodigoSku { get; set; } = string.Empty;

    [Column("id_lista")]
    public int IdLista { get; set; }

    [Required]
    [Column("precio_paquete", TypeName = "decimal(12,2)")]
    public decimal PrecioPaquete { get; set; }

    [Required]
    [Column("precio_por_unidad", TypeName = "decimal(12,2)")]
    public decimal PrecioPorUnidad { get; set; }

    [ForeignKey(nameof(CodigoSku))]
    public SKU SKU { get; set; } = null!;

    [ForeignKey(nameof(IdLista))]
    public ListaPrecios ListaPrecios { get; set; } = null!;
}
