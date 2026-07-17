using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EmpanadasDLujo.API.Services;

// Envío de plantillas de WhatsApp (Meta Graph API). Reutilizable desde cualquier
// controlador; centraliza el token/URL de configuración y el armado del payload.
public interface IWhatsAppSender
{
    Task<(bool ok, int? statusCode, string body)> SendTemplateAsync(
        string to, string templateName, string languageCode, IEnumerable<string> bodyParams,
        string? otpButtonCode = null);
}

public class WhatsAppSender : IWhatsAppSender
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WhatsAppSender> _logger;

    public WhatsAppSender(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<WhatsAppSender> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(bool ok, int? statusCode, string body)> SendTemplateAsync(
        string to, string templateName, string languageCode, IEnumerable<string> bodyParams,
        string? otpButtonCode = null)
    {
        var token = _configuration["WhatsApp:Token"];
        var phoneNumberId = _configuration["WhatsApp:PhoneNumberId"];
        var apiUrl = _configuration["WhatsApp:ApiUrl"] ?? "https://graph.facebook.com/v18.0";

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(phoneNumberId) || string.IsNullOrWhiteSpace(to))
        {
            _logger.LogError("[WhatsApp] Configuración incompleta o destinatario vacío.");
            return (false, null, "Configuración de WhatsApp incompleta o destinatario vacío.");
        }

        var components = new List<object>
        {
            new
            {
                type = "body",
                parameters = bodyParams.Select(p => new { type = "text", text = p }).ToArray()
            }
        };

        // Plantillas de tipo Authentication: el botón "Copiar código" (URL) exige el código
        // como parámetro del botón en el índice 0, además del cuerpo.
        if (!string.IsNullOrWhiteSpace(otpButtonCode))
        {
            components.Add(new
            {
                type = "button",
                sub_type = "url",
                index = "0",
                parameters = new[] { new { type = "text", text = otpButtonCode } }
            });
        }

        var payload = new
        {
            messaging_product = "whatsapp",
            to,
            type = "template",
            template = new
            {
                name = templateName,
                language = new { code = languageCode },
                components = components.ToArray()
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync($"{apiUrl}/{phoneNumberId}/messages", content);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                _logger.LogError("[WhatsApp] Error enviando plantilla {Template}. Status {Status}. Body {Body}",
                    templateName, response.StatusCode, body);
            return (response.IsSuccessStatusCode, (int)response.StatusCode, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[WhatsApp] Excepción enviando plantilla {Template} a {To}.", templateName, to);
            return (false, null, ex.Message);
        }
    }
}
