using LimpeAgora.Shared.DTOs.Categorias;

namespace LimpeAgoraApi.Services.Interfaces;

public interface ICategoriaService
{
    Task<List<CategoriaResponse>> ListarAsync(bool? apenasAtivas = null);
    Task<CategoriaResponse> ObterPorIdAsync(Guid id);
    Task<CategoriaResponse> CriarAsync(CategoriaUpsertRequest request);
    Task<CategoriaResponse> AtualizarAsync(Guid id, CategoriaUpsertRequest request);
    Task InativarAsync(Guid id);
}
