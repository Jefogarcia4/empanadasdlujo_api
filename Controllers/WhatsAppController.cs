using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EmpanadasDLujo.API.Data;
using EmpanadasDLujo.API.DTOs;
using EmpanadasDLujo.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmpanadasDLujo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WhatsAppController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhatsAppController> _logger;
    private readonly AppDbContext _context;

    public WhatsAppController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<WhatsAppController> logger,
        AppDbContext context)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _context = context;
    }

    // POST api/whatsapp/send_message  (público: lo invoca el checkout)
    [AllowAnonymous]
    [HttpPost("send_message")]
    public async Task<IActionResult> SendMessage([FromBody] WhatsAppSendMessageDto dto)
    {
        var token = _configuration["WhatsApp:Token"];
        var phoneNumberId = _configuration["WhatsApp:PhoneNumberId"];
        var defaultRecipient = _configuration["WhatsApp:RecipientNumber"];
        var apiUrl = _configuration["WhatsApp:ApiUrl"] ?? "https://graph.facebook.com/v18.0";

        // Permite cambiar el destinatario desde la petición (p. ej. confirmación al comprador).
        var recipient = string.IsNullOrWhiteSpace(dto.To) ? defaultRecipient : dto.To.Trim();

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(phoneNumberId) || string.IsNullOrEmpty(recipient))
        {
            _logger.LogError("[WhatsApp] Faltan variables de configuración (Token, PhoneNumberId, RecipientNumber).");
            await RegistrarAsync(dto, recipient, "FALLIDO", null, "Configuración de WhatsApp incompleta en el servidor.", null);
            return StatusCode(500, new { error = "Configuración de WhatsApp incompleta en el servidor." });
        }

        var payload = new
        {
            messaging_product = "whatsapp",
            to = recipient,
            type = "template",
            template = dto.Template
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        _logger.LogInformation("[WhatsApp] Enviando mensaje a {Recipient} con plantilla {Template}",
            recipient, dto.Template.Name);

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        string responseBody;
        try
        {
            response = await client.PostAsync($"{apiUrl}/{phoneNumberId}/messages", content);
            responseBody = await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            // Fallo de red / excepción al llamar a Meta.
            _logger.LogError(ex, "[WhatsApp] Excepción al enviar mensaje a {Recipient}.", recipient);
            await RegistrarAsync(dto, recipient, "FALLIDO", null, ex.Message, json);
            return StatusCode(502, new { error = "No se pudo contactar el servicio de WhatsApp." });
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("[WhatsApp] Error al enviar mensaje. Status: {Status}. Body: {Body}",
                response.StatusCode, responseBody);
            await RegistrarAsync(dto, recipient, "FALLIDO", (int)response.StatusCode, responseBody, json);
            return StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<object>(responseBody));
        }

        _logger.LogInformation("[WhatsApp] Mensaje enviado exitosamente.");
        await RegistrarAsync(dto, recipient, "EXITOSO", (int)response.StatusCode, responseBody, json);
        return Ok(JsonSerializer.Deserialize<object>(responseBody));
    }

    // GET api/whatsapp/logs?idOrden=&estado=&take=  (solo admin) — envíos registrados, recientes primero.
    [Authorize]
    [HttpGet("logs")]
    public async Task<ActionResult<IEnumerable<WhatsAppLogDto>>> GetLogs(
        [FromQuery] int? idOrden = null,
        [FromQuery] string? estado = null,
        [FromQuery] int take = 200)
    {
        var query = _context.WhatsAppLogs.AsQueryable();
        if (idOrden.HasValue)
            query = query.Where(l => l.IdOrden == idOrden.Value);
        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(l => l.Estado == estado);

        take = Math.Clamp(take, 1, 1000);

        var items = await query
            .OrderByDescending(l => l.IdLog)
            .Take(take)
            .Select(l => new WhatsAppLogDto
            {
                IdLog        = l.IdLog,
                IdOrden      = l.IdOrden,
                Tipo         = l.Tipo,
                Destinatario = l.Destinatario,
                Plantilla    = l.Plantilla,
                Estado       = l.Estado,
                CodigoHttp   = l.CodigoHttp,
                MensajeError = l.MensajeError,
                FechaIntento = l.FechaIntento
            })
            .ToListAsync();

        return Ok(items);
    }

    // Registra el resultado de un envío (EXITOSO | FALLIDO). Aislado en su propio try/catch:
    // si el log falla, se registra con ILogger pero NUNCA rompe la respuesta al cliente.
    // En 'detalle' se guarda la respuesta de WhatsApp (o el error).
    private async Task RegistrarAsync(
        WhatsAppSendMessageDto dto, string? destinatario, string estado,
        int? codigoHttp, string? detalle, string? payloadJson)
    {
        try
        {
            _context.WhatsAppLogs.Add(new WhatsAppLog
            {
                IdOrden      = dto.IdOrden,
                Tipo         = dto.Tipo,
                Destinatario = destinatario,
                Plantilla    = dto.Template?.Name,
                Estado       = estado,
                CodigoHttp   = codigoHttp,
                MensajeError = detalle,
                PayloadJson  = payloadJson,
                FechaIntento = DateTime.Now
            });
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[WhatsApp] No se pudo registrar el envío en WhatsAppLog.");
        }
    }
}
