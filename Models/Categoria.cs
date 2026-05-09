using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmpanadasDLujo.API.Models;

[Table("Categoria")]
public class Categoria
{
    [Key]
    [Column("id_categoria")]
    public int IdCategoria { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
