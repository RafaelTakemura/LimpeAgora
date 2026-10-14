using LimpeAgora.Shared.DTOs.Categorias;

namespace LimpeAgoraWeb.Services;

public class CategoriaWebService
{
    private readonly ApiClient _apiClient;

    public CategoriaWebService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<CategoriaResponse>> ListarAsync(bool? apenasAtivas = null)
    {
        var endpoint = apenasAtivas.HasValue ? $"api/categorias?apenasAtivas={apenasAtivas.Value}" : "api/categorias";
        var resultado = await _apiClient.GetFromJsonAsync<List<CategoriaResponse>>(endpoint);
        return resultado ?? new List<CategoriaResponse>();
    }

    public async Task<CategoriaResponse?> ObterPorIdAsync(Guid id)
    {
        return await _apiClient.GetFromJsonAsync<CategoriaResponse>($"api/categorias/{id}");
    }

    public async Task<CategoriaResponse?> CriarAsync(CategoriaUpsertRequest request)
    {
        return await _apiClient.PostAsJsonAsync<CategoriaUpsertRequest, CategoriaResponse>("api/categorias", request);
    }

    public async Task<CategoriaResponse?> AtualizarAsync(Guid id, CategoriaUpsertRequest request)
    {
        return await _apiClient.PutAsJsonAsync<CategoriaUpsertRequest, CategoriaResponse>($"api/categorias/{id}", request);
    }

    public async Task InativarAsync(Guid id)
    {
        await _apiClient.DeleteAsync($"api/categorias/{id}");
    }
}
