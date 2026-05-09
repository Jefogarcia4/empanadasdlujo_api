using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmpanadasDLujo.API.Models;

[Table("Sabor")]
public class Sabor
{
    [Key]
    [Column("id_sabor")]
    public int IdSabor { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    public ICollection<SKU> SKUs { get; set; } = new List<SKU>();
}
