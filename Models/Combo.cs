using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmpanadasDLujo.API.Models;

[Table("Combo")]
public class Combo
{
    [Key]
    [Column("id_combo")]
    public int IdCombo { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("codigo_combo")]
    public string CodigoCombo { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column("subcategoria")]
    public string? Subcategoria { get; set; }

    [MaxLength(255)]
    [Column("descripcion_corta")]
    public string? DescripcionCorta { get; set; }

    [MaxLength(1000)]
    [Column("descripcion_larga")]
    public string? DescripcionLarga { get; set; }

    [Required]
    [Column("precio_normal", TypeName = "decimal(12,2)")]
    public decimal PrecioNormal { get; set; }

    [Required]
    [Column("precio_combo", TypeName = "decimal(12,2)")]
    public decimal PrecioCombo { get; set; }

    [Column("peso_total_g", TypeName = "decimal(10,2)")]
    public decimal? PesoTotalG { get; set; }

    [Column("unidades_totales")]
    public int? UnidadesTotales { get; set; }

    [Column("activo")]
    public bool Activo { get; set; } = true;

    [Column("orden")]
    public int? Orden { get; set; }

    [MaxLength(255)]
    [Column("url_image")]
    public string? UrlImage { get; set; }

    [MaxLength(100)]
    [Column("badge_descripcion")]
    public string? BadgeDescripcion { get; set; }

    public ICollection<ComboComponente> Componentes { get; set; } = new List<ComboComponente>();
    public ICollection<OrdenDetalle> OrdenDetalles { get; set; } = new List<OrdenDetalle>();
}
