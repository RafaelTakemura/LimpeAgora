using LimpeAgora.Shared.DTOs.Usuarios;

namespace LimpeAgoraWeb.Services;

public class UsuarioWebService
{
    private readonly ApiClient _apiClient;

    public UsuarioWebService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<UsuarioResponse>> ListarAsync()
    {
        var resultado = await _apiClient.GetFromJsonAsync<List<UsuarioResponse>>("api/usuarios");
        return resultado ?? new List<UsuarioResponse>();
    }

    public async Task<UsuarioResponse?> ObterPorIdAsync(Guid id)
    {
        return await _apiClient.GetFromJsonAsync<UsuarioResponse>($"api/usuarios/{id}");
    }

    public async Task<UsuarioResponse?> CriarAsync(CriarUsuarioRequest request)
    {
        return await _apiClient.PostAsJsonAsync<CriarUsuarioRequest, UsuarioResponse>("api/usuarios", request);
    }

    public async Task<UsuarioResponse?> AtualizarAsync(Guid id, AtualizarUsuarioRequest request)
    {
        return await _apiClient.PutAsJsonAsync<AtualizarUsuarioRequest, UsuarioResponse>($"api/usuarios/{id}", request);
    }

    public async Task InativarAsync(Guid id)
    {
        await _apiClient.DeleteAsync($"api/usuarios/{id}");
    }
}
