using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmpanadasDLujo.API.Models;

[Table("Producto")]
public class Producto
{
    [Key]
    [Column("id_producto")]
    public int IdProducto { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("id_categoria")]
    public int IdCategoria { get; set; }

    [ForeignKey(nameof(IdCategoria))]
    public Categoria Categoria { get; set; } = null!;

    public ICollection<SKU> SKUs { get; set; } = new List<SKU>();
}
