using LimpeAgora.Shared.DTOs.Auth;
using LimpeAgora.Shared.DTOs.Usuarios;

namespace LimpeAgoraWeb.Services;

public class AuthWebService
{
    private readonly ApiClient _apiClient;
    private readonly AuthSessionService _sessionService;

    public AuthWebService(ApiClient apiClient, AuthSessionService sessionService)
    {
        _apiClient = apiClient;
        _sessionService = sessionService;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var response = await _apiClient.PostAsJsonAsync<LoginRequest, AuthResponse>("api/auth/login", request);
        if (response != null && !string.IsNullOrEmpty(response.Token) && response.Usuario != null)
        {
            _sessionService.IniciarSessao(response.Token, response.Usuario);
            return response;
        }

        throw new ApplicationException("Falha ao obter dados de autenticação.");
    }

    public void Logout()
    {
        _sessionService.EncerrarSessao();
    }
}
