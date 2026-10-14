using LimpeAgora.Shared.DTOs.Auth;
using LimpeAgora.Shared.DTOs.Usuarios;

namespace LimpeAgoraApi.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> CadastrarClienteAsync(CadastrarClienteRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<UsuarioResponse> ObterUsuarioLogadoAsync(Guid usuarioId);
}
