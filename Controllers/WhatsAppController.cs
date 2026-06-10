using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EmpanadasDLujo.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmpanadasDLujo.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class WhatsAppController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhatsAppController> _logger;

    public WhatsAppController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<WhatsAppController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    // POST api/whatsapp/send_message
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
        var response = await client.PostAsync($"{apiUrl}/{phoneNumberId}/messages", content);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("[WhatsApp] Error al enviar mensaje. Status: {Status}. Body: {Body}",
                response.StatusCode, responseBody);
            return StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<object>(responseBody));
        }

        _logger.LogInformation("[WhatsApp] Mensaje enviado exitosamente.");
        return Ok(JsonSerializer.Deserialize<object>(responseBody));
    }
}
