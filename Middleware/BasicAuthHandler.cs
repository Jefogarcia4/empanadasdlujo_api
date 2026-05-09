using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace EmpanadasDLujo.API.Middleware;

public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;

    public BasicAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header"));

        try
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]!);
            if (!string.Equals(authHeader.Scheme, "Basic", StringComparison.OrdinalIgnoreCase))
                return Task.FromResult(AuthenticateResult.Fail("Invalid authentication scheme"));

            var credentialBytes = Convert.FromBase64String(authHeader.Parameter ?? string.Empty);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);

            if (credentials.Length != 2)
                return Task.FromResult(AuthenticateResult.Fail("Invalid credentials format"));

            var username = credentials[0];
            var password = credentials[1];

            var users = _configuration.GetSection("BasicAuth:Users")
                .Get<List<BasicAuthUser>>() ?? new List<BasicAuthUser>();

            var validUser = users.FirstOrDefault(u =>
                u.Username == username && u.Password == password);

            if (validUser is null)
                return Task.FromResult(AuthenticateResult.Fail("Invalid username or password"));

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, validUser.Role ?? "User")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization header"));
        }
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers["WWW-Authenticate"] = "Basic realm=\"EmpanadasDLujo API\"";
        await base.HandleChallengeAsync(properties);
    }
}

public class BasicAuthUser
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
}
