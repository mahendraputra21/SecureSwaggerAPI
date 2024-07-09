using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace SecureSwaggerAPI;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public BasicAuthenticationHandler(
             IOptionsMonitor<AuthenticationSchemeOptions> options,
             ILoggerFactory logger,
             UrlEncoder encoder,
             ISystemClock clock)
             : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return AuthenticateResult.Fail("Missing Authorization Header");
        }

        var headerValue = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
        if (headerValue.Scheme != "Basic")
        {
            return AuthenticateResult.Fail("Invalid Authorization Scheme");
        }

        var credentialBytes = Convert.FromBase64String(headerValue.Parameter);
        var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
        var username = credentials[0];
        var password = credentials[1];

        if (username != "admin" || password != "password")
        {
            return AuthenticateResult.Fail("Invalid Username or Password");
        }

        var claims = new[] { new Claim(ClaimTypes.Name, username) };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
