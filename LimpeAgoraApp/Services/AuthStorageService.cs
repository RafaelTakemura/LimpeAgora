namespace LimpeAgoraApp.Services;

public interface IAuthStorageService
{
    Task SalvarTokenAsync(string token);
    Task<string?> ObterTokenAsync();
    Task SalvarUsuarioInfoAsync(Guid id, string nome, string email);
    Task<(Guid? Id, string? Nome, string? Email)> ObterUsuarioInfoAsync();
    Task LimparSessaoAsync();
}

public class AuthStorageService : IAuthStorageService
{
    private const string TokenKey = "limpeagora_jwt_token";
    private const string UserIdKey = "limpeagora_user_id";
    private const string UserNameKey = "limpeagora_user_name";
    private const string UserEmailKey = "limpeagora_user_email";

    public async Task SalvarTokenAsync(string token)
    {
        await SecureStorage.Default.SetAsync(TokenKey, token);
    }

    public async Task<string?> ObterTokenAsync()
    {
        return await SecureStorage.Default.GetAsync(TokenKey);
    }

    public async Task SalvarUsuarioInfoAsync(Guid id, string nome, string email)
    {
        await SecureStorage.Default.SetAsync(UserIdKey, id.ToString());
        await SecureStorage.Default.SetAsync(UserNameKey, nome);
        await SecureStorage.Default.SetAsync(UserEmailKey, email);
    }

    public async Task<(Guid? Id, string? Nome, string? Email)> ObterUsuarioInfoAsync()
    {
        var idStr = await SecureStorage.Default.GetAsync(UserIdKey);
        var nome = await SecureStorage.Default.GetAsync(UserNameKey);
        var email = await SecureStorage.Default.GetAsync(UserEmailKey);

        Guid? id = Guid.TryParse(idStr, out var parsed) ? parsed : null;
        return (id, nome, email);
    }

    public Task LimparSessaoAsync()
    {
        SecureStorage.Default.Remove(TokenKey);
        SecureStorage.Default.Remove(UserIdKey);
        SecureStorage.Default.Remove(UserNameKey);
        SecureStorage.Default.Remove(UserEmailKey);
        return Task.CompletedTask;
    }
}
