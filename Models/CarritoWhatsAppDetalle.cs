using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmpanadasDLujo.API.Models;

// Item de un carrito borrador de WhatsApp. Igual que OrdenDetalle, referencia un SKU O un
// combo (nunca ambos ni ninguno). Sin FK dura a SKU/Combo; la existencia se valida al crear.
[Table("CarritoWhatsAppDetalle")]
public class CarritoWhatsAppDetalle
{
    [Key]
    [Column("id_detalle")]
    public int IdDetalle { get; set; }

    [Column("id_carrito")]
    public int IdCarrito { get; set; }

    [MaxLength(20)]
    [Column("codigo_sku")]
    public string? CodigoSku { get; set; }

    [Column("id_combo")]
    public int? IdCombo { get; set; }

    [Column("cantidad")]
    public int Cantidad { get; set; }

    [ForeignKey(nameof(IdCarrito))]
    public CarritoWhatsApp Carrito { get; set; } = null!;
}
