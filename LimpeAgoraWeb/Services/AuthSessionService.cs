using LimpeAgora.Shared.DTOs.Usuarios;
using LimpeAgora.Shared.Enums;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace LimpeAgoraWeb.Services;

public class AuthSessionData
{
    public string Token { get; set; } = string.Empty;
    public UsuarioResponse? Usuario { get; set; }
}

public class AuthSessionService
{
    private const string StorageKey = "limpeagora_auth_session";
    private readonly ProtectedLocalStorage? _protectedLocalStorage;

    public string? Token { get; private set; }
    public UsuarioResponse? Usuario { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(Token) && Usuario != null;
    public bool IsAdmin => Usuario?.Perfil == PerfilUsuario.Administrador;
    public bool IsFuncionario => Usuario?.Perfil == PerfilUsuario.Funcionario || IsAdmin;

    public event Action? OnChange;

    public AuthSessionService(ProtectedLocalStorage? protectedLocalStorage = null)
    {
        _protectedLocalStorage = protectedLocalStorage;
    }

    public void IniciarSessao(string token, UsuarioResponse usuario)
    {
        Token = token;
        Usuario = usuario;
        NotifyStateChanged();
        _ = PersistirSessaoAsync(token, usuario);
    }

    public async Task IniciarSessaoAsync(string token, UsuarioResponse usuario)
    {
        Token = token;
        Usuario = usuario;
        NotifyStateChanged();
        await PersistirSessaoAsync(token, usuario);
    }

    public void EncerrarSessao()
    {
        Token = null;
        Usuario = null;
        NotifyStateChanged();
        _ = LimparSessaoPersistidaAsync();
    }

    public async Task EncerrarSessaoAsync()
    {
        Token = null;
        Usuario = null;
        NotifyStateChanged();
        await LimparSessaoPersistidaAsync();
    }

    public async Task<bool> RestaurarSessaoAsync()
    {
        if (IsAuthenticated)
        {
            return true;
        }

        if (_protectedLocalStorage == null)
        {
            return false;
        }

        try
        {
            var result = await _protectedLocalStorage.GetAsync<AuthSessionData>(StorageKey);
            if (result.Success && result.Value != null && !string.IsNullOrEmpty(result.Value.Token) && result.Value.Usuario != null)
            {
                Token = result.Value.Token;
                Usuario = result.Value.Usuario;
                NotifyStateChanged();
                return true;
            }
        }
        catch
        {
            // Tratamento resiliente caso o JS interop não esteja disponível ou a chave seja inválida
        }

        return false;
    }

    private async Task PersistirSessaoAsync(string token, UsuarioResponse usuario)
    {
        if (_protectedLocalStorage == null) return;

        try
        {
            await _protectedLocalStorage.SetAsync(StorageKey, new AuthSessionData
            {
                Token = token,
                Usuario = usuario
            });
        }
        catch
        {
            // Ignora silenciosamente se o armazenamento seguro falhar temporariamente
        }
    }

    private async Task LimparSessaoPersistidaAsync()
    {
        if (_protectedLocalStorage == null) return;

        try
        {
            await _protectedLocalStorage.DeleteAsync(StorageKey);
        }
        catch
        {
            // Ignora silenciosamente
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
