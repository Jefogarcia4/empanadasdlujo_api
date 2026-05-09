using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmpanadasDLujo.API.Models;

[Table("Lista_Precios")]
public class ListaPrecios
{
    [Key]
    [Column("id_lista")]
    public int IdLista { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    public ICollection<PrecioSKU> PreciosSKU { get; set; } = new List<PrecioSKU>();
    public ICollection<Orden> Ordenes { get; set; } = new List<Orden>();
}
