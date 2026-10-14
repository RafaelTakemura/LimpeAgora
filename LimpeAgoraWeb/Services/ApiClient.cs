using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LimpeAgoraWeb.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthSessionService _sessionService;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(HttpClient httpClient, AuthSessionService sessionService)
    {
        _httpClient = httpClient;
        _sessionService = sessionService;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    private void AplicarTokenAutenticacao()
    {
        if (!string.IsNullOrEmpty(_sessionService.Token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _sessionService.Token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<T?> GetFromJsonAsync<T>(string endpoint)
    {
        AplicarTokenAutenticacao();
        var response = await _httpClient.GetAsync(endpoint);
        await GarantirSucessoAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
    }

    public async Task<TResponse?> PostAsJsonAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        AplicarTokenAutenticacao();
        var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions);
        await GarantirSucessoAsync(response);
        return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
    }

    public async Task PostAsJsonAsync<TRequest>(string endpoint, TRequest data)
    {
        AplicarTokenAutenticacao();
        var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions);
        await GarantirSucessoAsync(response);
    }

    public async Task<TResponse?> PutAsJsonAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        AplicarTokenAutenticacao();
        var response = await _httpClient.PutAsJsonAsync(endpoint, data, _jsonOptions);
        await GarantirSucessoAsync(response);
        return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
    }

    public async Task<TResponse?> PatchAsJsonAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        AplicarTokenAutenticacao();
        var response = await _httpClient.PatchAsJsonAsync(endpoint, data, _jsonOptions);
        await GarantirSucessoAsync(response);
        return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
    }

    public async Task DeleteAsync(string endpoint)
    {
        AplicarTokenAutenticacao();
        var response = await _httpClient.DeleteAsync(endpoint);
        await GarantirSucessoAsync(response);
    }

    private async Task GarantirSucessoAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        string mensagemErro = $"Erro na comunicação com a API (Código HTTP {(int)response.StatusCode}).";

        try
        {
            var content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(content))
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("mensagem", out var propMensagem))
                {
                    mensagemErro = propMensagem.GetString() ?? mensagemErro;
                }
                else if (doc.RootElement.TryGetProperty("title", out var propTitle))
                {
                    mensagemErro = propTitle.GetString() ?? mensagemErro;
                }
            }
        }
        catch
        {
            // Caso a resposta não seja JSON formatado, usa a mensagem genérica HTTP
        }

        throw new ApplicationException(mensagemErro);
    }
}
