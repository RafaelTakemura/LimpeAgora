using LimpeAgora.Shared.DTOs.Usuarios;

namespace LimpeAgoraApi.Services.Interfaces;

public interface IUsuarioService
{
    Task<List<UsuarioResponse>> ListarAsync();
    Task<UsuarioResponse> ObterPorIdAsync(Guid id);
    Task<UsuarioResponse> CriarAsync(CriarUsuarioRequest request);
    Task<UsuarioResponse> AtualizarAsync(Guid id, AtualizarUsuarioRequest request);
    Task InativarAsync(Guid id);
}
