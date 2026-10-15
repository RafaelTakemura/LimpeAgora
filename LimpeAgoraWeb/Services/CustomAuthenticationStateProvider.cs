using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace LimpeAgoraWeb.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly AuthSessionService _sessionService;

    public CustomAuthenticationStateProvider(AuthSessionService sessionService)
    {
        _sessionService = sessionService;
        _sessionService.OnChange += () => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_sessionService.IsAuthenticated)
        {
            await _sessionService.RestaurarSessaoAsync();
        }

        if (!_sessionService.IsAuthenticated || _sessionService.Usuario == null)
        {
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            return new AuthenticationState(anonymous);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, _sessionService.Usuario.Id.ToString()),
            new(ClaimTypes.Name, _sessionService.Usuario.NomeCompleto),
            new(ClaimTypes.Email, _sessionService.Usuario.Email),
            new(ClaimTypes.Role, _sessionService.Usuario.Perfil.ToString()),
            new("perfil", _sessionService.Usuario.Perfil.ToString())
        };

        var identity = new ClaimsIdentity(claims, "LimpeAgoraJwtAuth");
        var user = new ClaimsPrincipal(identity);

        return new AuthenticationState(user);
    }
}
