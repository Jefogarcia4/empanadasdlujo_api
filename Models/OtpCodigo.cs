using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmpanadasDLujo.API.Models;

// Reto OTP para el portal de clientes: se guarda el hash del código (nunca en texto plano),
// con expiración, contador de intentos y bandera de un solo uso.
[Table("OtpCodigo")]
public class OtpCodigo
{
    [Key]
    [Column("id_otp")]
    public int IdOtp { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("telefono")]
    public string Telefono { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("codigo_hash")]
    public string CodigoHash { get; set; } = string.Empty;

    [Column("expira_en")]
    public DateTime ExpiraEn { get; set; }

    [Column("intentos")]
    public int Intentos { get; set; } = 0;

    [Column("consumido")]
    public bool Consumido { get; set; } = false;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
