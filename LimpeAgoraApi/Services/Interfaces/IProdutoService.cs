using LimpeAgora.Shared.DTOs.Produtos;

namespace LimpeAgoraApi.Services.Interfaces;

public interface IProdutoService
{
    Task<List<ProdutoResponse>> ListarAsync(Guid? categoriaId = null, bool? apenasAtivos = null);
    Task<ProdutoResponse> ObterPorIdAsync(Guid id);
    Task<ProdutoResponse> CriarAsync(CriarProdutoRequest request);
    Task<ProdutoResponse> AtualizarAsync(Guid id, AtualizarProdutoRequest request);
    Task InativarAsync(Guid id);
    Task<ProdutoResponse> AjustarEstoqueAsync(Guid id, AjustarEstoqueRequest request);
}
