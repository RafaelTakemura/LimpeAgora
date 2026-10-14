using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LimpeAgoraApp.Services;

public interface IApiClientService
{
    bool IsOnline { get; }
    Task<T?> GetAsync<T>(string endpoint);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data);
    Task PostAsync<TRequest>(string endpoint, TRequest data);
}

public class ApiClientService : IApiClientService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthStorageService _authStorage;
    private readonly JsonSerializerOptions _jsonOptions;

    public static string BaseUrl =>
        DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5025/"
            : "http://localhost:5025/";

    public bool IsOnline => Connectivity.Current.NetworkAccess != NetworkAccess.None &&
                            Connectivity.Current.NetworkAccess != NetworkAccess.Unknown;

    public ApiClientService(IAuthStorageService authStorage)
    {
        _authStorage = authStorage;
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    private async Task AplicarTokenAsync()
    {
        var token = await _authStorage.ObterTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        if (!IsOnline)
        {
            throw new HttpRequestException("Dispositivo sem conexão à internet.");
        }

        await AplicarTokenAsync();
        var response = await _httpClient.GetAsync(endpoint);
        await TratarErroSeHouverAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        if (!IsOnline)
        {
            throw new HttpRequestException("Dispositivo sem conexão à internet.");
        }

        await AplicarTokenAsync();
        var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions);
        await TratarErroSeHouverAsync(response);
        return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
    }

    public async Task PostAsync<TRequest>(string endpoint, TRequest data)
    {
        if (!IsOnline)
        {
            throw new HttpRequestException("Dispositivo sem conexão à internet.");
        }

        await AplicarTokenAsync();
        var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions);
        await TratarErroSeHouverAsync(response);
    }

    private static async Task TratarErroSeHouverAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        string erroMsg = $"Erro na comunicação com a API (HTTP {(int)response.StatusCode})";
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(content))
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("mensagem", out var prop))
                {
                    erroMsg = prop.GetString() ?? erroMsg;
                }
            }
        }
        catch { }

        throw new ApplicationException(erroMsg);
    }
}
