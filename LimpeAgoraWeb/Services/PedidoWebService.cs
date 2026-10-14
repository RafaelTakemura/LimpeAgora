using LimpeAgora.Shared.DTOs.Pedidos;
using LimpeAgora.Shared.Enums;

namespace LimpeAgoraWeb.Services;

public class PedidoWebService
{
    private readonly ApiClient _apiClient;

    public PedidoWebService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<PedidoResumoResponse>> ListarTodosAsync(
        StatusPedido? status = null,
        Guid? usuarioId = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null)
    {
        var queryParams = new List<string>();
        if (status.HasValue)
            queryParams.Add($"status={status.Value}");
        if (usuarioId.HasValue && usuarioId.Value != Guid.Empty)
            queryParams.Add($"usuarioId={usuarioId.Value}");
        if (dataInicio.HasValue)
            queryParams.Add($"dataInicio={dataInicio.Value:O}");
        if (dataFim.HasValue)
            queryParams.Add($"dataFim={dataFim.Value:O}");

        var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
        var resultado = await _apiClient.GetFromJsonAsync<List<PedidoResumoResponse>>($"api/pedidos{queryString}");
        return resultado ?? new List<PedidoResumoResponse>();
    }

    public async Task<PedidoDetalheResponse?> ObterPorIdAsync(Guid id)
    {
        return await _apiClient.GetFromJsonAsync<PedidoDetalheResponse>($"api/pedidos/{id}");
    }

    public async Task<PedidoDetalheResponse?> AlterarStatusAsync(Guid id, StatusPedido novoStatus)
    {
        var request = new AlterarStatusPedidoRequest { NovoStatus = novoStatus };
        return await _apiClient.PatchAsJsonAsync<AlterarStatusPedidoRequest, PedidoDetalheResponse>($"api/pedidos/{id}/status", request);
    }

    public async Task CancelarAsync(Guid id)
    {
        await _apiClient.PostAsJsonAsync($"api/pedidos/{id}/cancelamento", new { });
    }

    public async Task<List<HistoricoStatusResponse>> ObterHistoricoAsync(Guid id)
    {
        var resultado = await _apiClient.GetFromJsonAsync<List<HistoricoStatusResponse>>($"api/pedidos/{id}/historico");
        return resultado ?? new List<HistoricoStatusResponse>();
    }

    public async Task<Dictionary<StatusPedido, int>> ObterContagemPorStatusAsync()
    {
        var pedidos = await ListarTodosAsync();
        return pedidos
            .GroupBy(p => p.Status)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}
