using LimpeAgora.Shared.DTOs.Produtos;

namespace LimpeAgoraWeb.Services;

public class ProdutoWebService
{
    private readonly ApiClient _apiClient;

    public ProdutoWebService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<ProdutoResponse>> ListarAsync(Guid? categoriaId = null, bool? apenasAtivos = null)
    {
        var queryParams = new List<string>();
        if (categoriaId.HasValue && categoriaId.Value != Guid.Empty)
            queryParams.Add($"categoriaId={categoriaId.Value}");
        if (apenasAtivos.HasValue)
            queryParams.Add($"apenasAtivos={apenasAtivos.Value}");

        var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
        var resultado = await _apiClient.GetFromJsonAsync<List<ProdutoResponse>>($"api/produtos{queryString}");
        return resultado ?? new List<ProdutoResponse>();
    }

    public async Task<ProdutoResponse?> ObterPorIdAsync(Guid id)
    {
        return await _apiClient.GetFromJsonAsync<ProdutoResponse>($"api/produtos/{id}");
    }

    public async Task<ProdutoResponse?> CriarAsync(CriarProdutoRequest request)
    {
        return await _apiClient.PostAsJsonAsync<CriarProdutoRequest, ProdutoResponse>("api/produtos", request);
    }

    public async Task<ProdutoResponse?> AtualizarAsync(Guid id, AtualizarProdutoRequest request)
    {
        return await _apiClient.PutAsJsonAsync<AtualizarProdutoRequest, ProdutoResponse>($"api/produtos/{id}", request);
    }

    public async Task InativarAsync(Guid id)
    {
        await _apiClient.DeleteAsync($"api/produtos/{id}");
    }

    public async Task<ProdutoResponse?> AjustarEstoqueAsync(Guid id, AjustarEstoqueRequest request)
    {
        return await _apiClient.PatchAsJsonAsync<AjustarEstoqueRequest, ProdutoResponse>($"api/produtos/{id}/estoque", request);
    }
}
