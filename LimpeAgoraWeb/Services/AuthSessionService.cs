using LimpeAgora.Shared.DTOs.Usuarios;
using LimpeAgora.Shared.Enums;

namespace LimpeAgoraWeb.Services;

public class AuthSessionService
{
    public string? Token { get; private set; }
    public UsuarioResponse? Usuario { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(Token) && Usuario != null;
    public bool IsAdmin => Usuario?.Perfil == PerfilUsuario.Administrador;
    public bool IsFuncionario => Usuario?.Perfil == PerfilUsuario.Funcionario || IsAdmin;

    public event Action? OnChange;

    public void IniciarSessao(string token, UsuarioResponse usuario)
    {
        Token = token;
        Usuario = usuario;
        NotifyStateChanged();
    }

    public void EncerrarSessao()
    {
        Token = null;
        Usuario = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
