using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace aggregate_api.Core.Authentication;

public class FakeJwtHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public FakeJwtHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "EvaluatorUser"),
            new Claim("appid", "fake-app-id"),
            new Claim("app_displayname", "EvaluatorApp"),
            new Claim(ClaimTypes.Role, "FakeRole")
        };

        var identity = new ClaimsIdentity(claims, "FakeJwt");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "FakeJwt");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
