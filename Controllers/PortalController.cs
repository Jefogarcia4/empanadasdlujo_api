using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EmpanadasDLujo.API.Data;
using EmpanadasDLujo.API.DTOs;
using EmpanadasDLujo.API.Models;
using EmpanadasDLujo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EmpanadasDLujo.API.Controllers;

// Portal de clientes: login por código OTP enviado a WhatsApp y consulta de sus pedidos.
[ApiController]
[Route("api/[controller]")]
public class PortalController : ControllerBase
{
    // Esquema de autenticación JWT para el cliente (distinto del Basic del admin).
    public const string ClientScheme = "ClientJwt";

    private const int OTP_LONGITUD = 6;
    private const int OTP_VIGENCIA_MIN = 5;
    private const int OTP_MAX_INTENTOS = 5;
    private const int OTP_COOLDOWN_SEG = 60;

    private readonly AppDbContext _context;
    private readonly IWhatsAppSender _whatsApp;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PortalController> _logger;

    public PortalController(
        AppDbContext context,
        IWhatsAppSender whatsApp,
        IConfiguration configuration,
        ILogger<PortalController> logger)
    {
        _context = context;
        _whatsApp = whatsApp;
        _configuration = configuration;
        _logger = logger;
    }

    // POST api/portal/otp/solicitar  → genera y envía un código OTP al WhatsApp del teléfono.
    [AllowAnonymous]
    [HttpPost("otp/solicitar")]
    public async Task<ActionResult<OtpSolicitarRespuestaDto>> SolicitarOtp(OtpSolicitarDto dto)
    {
        var telefono = NormalizarTelefono(dto.Telefono);
        if (telefono.Length < 10)
            return BadRequest(new { message = "Número de teléfono inválido." });

        // Cooldown: evita reenvíos en ráfaga desde el mismo número.
        var ultimo = await _context.OtpCodigos
            .Where(o => o.Telefono == telefono)
            .OrderByDescending(o => o.IdOtp)
            .FirstOrDefaultAsync();

        if (ultimo is not null && (DateTime.Now - ultimo.FechaCreacion).TotalSeconds < OTP_COOLDOWN_SEG)
        {
            var espera = OTP_COOLDOWN_SEG - (int)(DateTime.Now - ultimo.FechaCreacion).TotalSeconds;
            return StatusCode(429, new { message = $"Espera {espera} segundos antes de pedir otro código." });
        }

        // Invalida cualquier código activo previo: solo uno vigente por teléfono.
        var activos = await _context.OtpCodigos
            .Where(o => o.Telefono == telefono && !o.Consumido)
            .ToListAsync();
        foreach (var a in activos) a.Consumido = true;

        var codigo = GenerarCodigo();
        var otp = new OtpCodigo
        {
            Telefono      = telefono,
            CodigoHash    = HashCodigo(telefono, codigo),
            ExpiraEn      = DateTime.Now.AddMinutes(OTP_VIGENCIA_MIN),
            Intentos      = 0,
            Consumido     = false,
            FechaCreacion = DateTime.Now
        };
        _context.OtpCodigos.Add(otp);
        await _context.SaveChangesAsync();

        // Modo dev: permite probar sin que WhatsApp funcione (devuelve el código y no falla el envío).
        var devMode = _configuration.GetValue<bool>("Otp:DevReturnCode");

        // Envía el código por WhatsApp (plantilla con el código como parámetro {{1}} del cuerpo).
        var templateName = _configuration["WhatsApp:OtpTemplateName"] ?? "codigo_verificacion";
        var templateLang = _configuration["WhatsApp:OtpTemplateLanguage"] ?? "es_CO";
        var (ok, _, body) = await _whatsApp.SendTemplateAsync(
            ToWhatsApp(telefono), templateName, templateLang, new[] { codigo }, otpButtonCode: codigo);

        if (!ok)
        {
            _logger.LogError("[Portal] No se pudo enviar el OTP a {Telefono}: {Body}", telefono, body);
            if (!devMode)
                return StatusCode(502, new { message = "No se pudo enviar el código por WhatsApp. Intenta de nuevo." });
        }

        if (devMode)
            _logger.LogWarning("[Portal][DEV] Código OTP para {Telefono}: {Codigo}", telefono, codigo);

        return Ok(new OtpSolicitarRespuestaDto
        {
            Enviado   = ok,
            ExpiraEn  = otp.ExpiraEn,
            Mensaje   = ok ? "Te enviamos un código por WhatsApp." : "Modo prueba: usa el código mostrado.",
            CodigoDev = devMode ? codigo : null
        });
    }

    // POST api/portal/otp/verificar  → valida el código y devuelve el token JWT del cliente.
    [AllowAnonymous]
    [HttpPost("otp/verificar")]
    public async Task<ActionResult<PortalSesionDto>> VerificarOtp(OtpVerificarDto dto)
    {
        var telefono = NormalizarTelefono(dto.Telefono);

        var otp = await _context.OtpCodigos
            .Where(o => o.Telefono == telefono && !o.Consumido && o.ExpiraEn > DateTime.Now)
            .OrderByDescending(o => o.IdOtp)
            .FirstOrDefaultAsync();

        if (otp is null)
            return BadRequest(new { message = "El código expiró o no existe. Solicita uno nuevo." });

        if (otp.Intentos >= OTP_MAX_INTENTOS)
        {
            otp.Consumido = true;
            await _context.SaveChangesAsync();
            return BadRequest(new { message = "Demasiados intentos. Solicita un código nuevo." });
        }

        if (otp.CodigoHash != HashCodigo(telefono, dto.Codigo))
        {
            otp.Intentos++;
            await _context.SaveChangesAsync();
            var restantes = OTP_MAX_INTENTOS - otp.Intentos;
            return BadRequest(new { message = $"Código incorrecto. Te quedan {restantes} intento(s)." });
        }

        otp.Consumido = true;
        await _context.SaveChangesAsync();

        // Nombre para saludar (del cliente más reciente con ese teléfono, si existe).
        var nombre = await _context.Clientes
            .Where(c => c.Telefono == telefono)
            .OrderByDescending(c => c.IdCliente)
            .Select(c => c.Nombre)
            .FirstOrDefaultAsync();

        var (token, expira) = GenerarJwt(telefono, nombre);

        return Ok(new PortalSesionDto
        {
            Token    = token,
            Telefono = telefono,
            Nombre   = nombre,
            ExpiraEn = expira
        });
    }

    // GET api/portal/mis-pedidos  → pedidos del teléfono autenticado (todos sus Cliente).
    [Authorize(AuthenticationSchemes = ClientScheme)]
    [HttpGet("mis-pedidos")]
    public async Task<ActionResult<IEnumerable<OrdenDto>>> MisPedidos()
    {
        var telefono = User.FindFirstValue("telefono");
        if (string.IsNullOrWhiteSpace(telefono))
            return Unauthorized();

        var items = await _context.Ordenes
            .Include(o => o.Cliente)
            .Include(o => o.Detalles).ThenInclude(d => d.Combo)
            .Where(o => o.Cliente.Telefono == telefono)
            .OrderByDescending(o => o.IdOrden)
            .Select(o => new OrdenDto
            {
                IdOrden                = o.IdOrden,
                IdCliente              = o.IdCliente,
                NombreCliente          = o.Cliente.Nombre,
                ApellidosCliente       = o.Cliente.Apellidos,
                TelefonoCliente        = o.Cliente.Telefono,
                EmailCliente           = o.Cliente.Email,
                DireccionCliente       = o.Cliente.Direccion,
                CasaApartamentoCliente = o.Cliente.CasaApartamento,
                CiudadCliente          = o.Cliente.Ciudad,
                DepartamentoCliente    = o.Cliente.Departamento,
                CodigoPostalCliente    = o.Cliente.CodigoPostal,
                PaisCliente            = o.Cliente.Pais,
                FechaOrden    = o.FechaOrden,
                Estado        = o.Estado,
                Subtotal      = o.Subtotal,
                Descuento     = o.Descuento,
                Total         = o.Total,
                Observaciones = o.Observaciones,
                Detalles = o.Detalles.Select(d => new OrdenDetalleDto
                {
                    IdDetalle          = d.IdDetalle,
                    CodigoSku          = d.CodigoSku,
                    IdCombo            = d.IdCombo,
                    CodigoCombo        = d.Combo != null ? d.Combo.CodigoCombo : null,
                    NombreCombo        = d.Combo != null ? d.Combo.Nombre : null,
                    EsCombo            = d.IdCombo != null,
                    CantidadPaquetes   = d.CantidadPaquetes,
                    PrecioPaqueteDetal = d.PrecioPaqueteDetal,
                    PrecioPaquete      = d.PrecioPaquete,
                    PrecioPorUnidad    = d.PrecioPorUnidad,
                    AplicaMayorista    = d.AplicaMayorista,
                    Subtotal           = d.Subtotal
                }).ToList()
            })
            .ToListAsync();

        return Ok(items);
    }

    // ─── Helpers ─────────────────────────────────────────────
    private static string NormalizarTelefono(string raw)
    {
        var digits = new string((raw ?? string.Empty).Where(char.IsDigit).ToArray());
        // Si viene con indicativo 57 + 10 dígitos, lo dejamos en los 10 locales para
        // que coincida con Cliente.Telefono (capturado a 10 dígitos en el checkout).
        if (digits.Length == 12 && digits.StartsWith("57"))
            digits = digits[2..];
        return digits;
    }

    // WhatsApp exige E.164 sin '+': anteponemos 57 a los 10 dígitos locales.
    private static string ToWhatsApp(string telefono) =>
        telefono.Length == 10 ? $"57{telefono}" : telefono;

    private static string GenerarCodigo() =>
        RandomNumberGenerator.GetInt32(0, 1_000_000).ToString($"D{OTP_LONGITUD}");

    private static string HashCodigo(string telefono, string codigo)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{telefono}:{codigo}"));
        return Convert.ToHexString(bytes);
    }

    private (string token, DateTime expira) GenerarJwt(string telefono, string? nombre)
    {
        var secret = _configuration["Jwt:Secret"]!;
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var dias = int.TryParse(_configuration["Jwt:ExpiresDays"], out var d) ? d : 30;
        var expira = DateTime.UtcNow.AddDays(dias);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new("telefono", telefono),
            new(ClaimTypes.Name, nombre ?? telefono),
        };

        var jwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expira,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(jwt), expira);
    }
}
