using LimpeAgoraApp.Models;

namespace LimpeAgoraApp.Services;

public interface ILocalDatabaseService
{
    Task InicializarAsync();
    Task SalvarCategoriasAsync(IEnumerable<CategoriaLocal> categorias);
    Task<List<CategoriaLocal>> ObterCategoriasAsync();
    Task SalvarProdutosAsync(IEnumerable<ProdutoLocal> produtos, Guid? categoriaId = null);
    Task<List<ProdutoLocal>> ObterProdutosAsync(Guid? categoriaId = null);
    Task SalvarPedidosAsync(Guid usuarioId, IEnumerable<PedidoLocal> pedidos);
    Task<List<PedidoLocal>> ObterPedidosAsync(Guid usuarioId);
    Task SalvarItensPedidoAsync(Guid pedidoId, IEnumerable<ItemPedidoLocal> itens);
    Task<List<ItemPedidoLocal>> ObterItensPedidoAsync(Guid pedidoId);
    Task LimparDadosUsuarioAsync(Guid usuarioId);
}
