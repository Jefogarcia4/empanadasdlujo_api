using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmpanadasDLujo.API.Models;

[Table("Combo_Componente")]
public class ComboComponente
{
    [Key]
    [Column("id_componente")]
    public int IdComponente { get; set; }

    [Column("id_combo")]
    public int IdCombo { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("codigo_sku")]
    public string CodigoSku { get; set; } = string.Empty;

    [Required]
    [Column("cantidad_paquetes")]
    public int CantidadPaquetes { get; set; }

    [ForeignKey(nameof(IdCombo))]
    public Combo Combo { get; set; } = null!;

    [ForeignKey(nameof(CodigoSku))]
    public SKU SKU { get; set; } = null!;
}
