using System.Text;
using EmpanadasDLujo.API.DTOs;
using EmpanadasDLujo.API.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmpanadasDLujo.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration) => _configuration = configuration;

    // Valida las credenciales contra los usuarios configurados (BasicAuth:Users) y,
    // si son correctas, devuelve el token Basic que el portal admin reutiliza en las
    // llamadas a los endpoints protegidos (Authorization: Basic {token}).
    [HttpPost("login")]
    public ActionResult<LoginResponseDto> Login(LoginDto dto)
    {
        var users = _configuration.GetSection("BasicAuth:Users")
            .Get<List<BasicAuthUser>>() ?? new List<BasicAuthUser>();

        var user = users.FirstOrDefault(u =>
            u.Username == dto.Username && u.Password == dto.Password);

        if (user is null)
            return Unauthorized(new { message = "Usuario o contraseña incorrectos." });

        var token = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{user.Username}:{user.Password}"));

        return Ok(new LoginResponseDto
        {
            Token = token,
            Username = user.Username,
            Role = user.Role
        });
    }
}
