using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmpanadasDLujo.API.Models;

[Table("Cliente")]
public class Cliente
{
    [Key]
    [Column("id_cliente")]
    public int IdCliente { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("telefono")]
    public string? Telefono { get; set; }

    [MaxLength(100)]
    [Column("email")]
    public string? Email { get; set; }

    [MaxLength(200)]
    [Column("direccion")]
    public string? Direccion { get; set; }

    [MaxLength(200)]
    [Column("nit")]
    public string? Nit { get; set; }

    [Column("activo")]
    public bool Activo { get; set; } = true;

    public ICollection<Orden> Ordenes { get; set; } = new List<Orden>();
}
