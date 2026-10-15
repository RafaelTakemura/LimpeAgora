using LimpeAgoraApp.Models;
using SQLite;

namespace LimpeAgoraApp.Services;

public class LocalDatabaseService : ILocalDatabaseService
{
    private SQLiteAsyncConnection? _database;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task InicializarAsync()
    {
        if (_database != null) return;

        await _semaphore.WaitAsync();
        try
        {
            if (_database != null) return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "limpeagora_cache.db3");
            _database = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache);

            await _database.CreateTableAsync<CategoriaLocal>();
            await _database.CreateTableAsync<ProdutoLocal>();
            await _database.CreateTableAsync<PedidoLocal>();
            await _database.CreateTableAsync<ItemPedidoLocal>();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SalvarCategoriasAsync(IEnumerable<CategoriaLocal> categorias)
    {
        await InicializarAsync();
        await _database!.DeleteAllAsync<CategoriaLocal>();
        await _database.InsertAllAsync(categorias);
    }

    public async Task<List<CategoriaLocal>> ObterCategoriasAsync()
    {
        await InicializarAsync();
        return await _database!.Table<CategoriaLocal>()
            .Where(c => c.Ativa)
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    public async Task SalvarProdutosAsync(IEnumerable<ProdutoLocal> produtos, Guid? categoriaId = null)
    {
        await InicializarAsync();

        if (categoriaId.HasValue && categoriaId.Value != Guid.Empty)
        {
            // Carga filtrada: substitui apenas os produtos da categoria, preservando o cache das demais
            var catId = categoriaId.Value;
            await _database!.Table<ProdutoLocal>().Where(p => p.CategoriaId == catId).DeleteAsync();
        }
        else
        {
            await _database!.DeleteAllAsync<ProdutoLocal>();
        }

        foreach (var produto in produtos)
        {
            await _database.InsertOrReplaceAsync(produto);
        }
    }

    public async Task<List<ProdutoLocal>> ObterProdutosAsync(Guid? categoriaId = null)
    {
        await InicializarAsync();
        var query = _database!.Table<ProdutoLocal>().Where(p => p.Ativo);

        if (categoriaId.HasValue && categoriaId.Value != Guid.Empty)
        {
            query = query.Where(p => p.CategoriaId == categoriaId.Value);
        }

        return await query.OrderBy(p => p.Nome).ToListAsync();
    }

    public async Task SalvarPedidosAsync(Guid usuarioId, IEnumerable<PedidoLocal> pedidos)
    {
        await InicializarAsync();
        // Remove pedidos anteriores do usuário para sincronização limpa
        var pedidosAntigos = await _database!.Table<PedidoLocal>()
            .Where(p => p.UsuarioId == usuarioId)
            .ToListAsync();

        foreach (var p in pedidosAntigos)
        {
            await _database.Table<ItemPedidoLocal>().Where(i => i.PedidoId == p.Id).DeleteAsync();
            await _database.DeleteAsync(p);
        }

        await _database.InsertAllAsync(pedidos);
    }

    public async Task<List<PedidoLocal>> ObterPedidosAsync(Guid usuarioId)
    {
        await InicializarAsync();
        return await _database!.Table<PedidoLocal>()
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.DataPedido)
            .ToListAsync();
    }

    public async Task SalvarItensPedidoAsync(Guid pedidoId, IEnumerable<ItemPedidoLocal> itens)
    {
        await InicializarAsync();
        await _database!.Table<ItemPedidoLocal>().Where(i => i.PedidoId == pedidoId).DeleteAsync();
        await _database.InsertAllAsync(itens);
    }

    public async Task<List<ItemPedidoLocal>> ObterItensPedidoAsync(Guid pedidoId)
    {
        await InicializarAsync();
        return await _database!.Table<ItemPedidoLocal>()
            .Where(i => i.PedidoId == pedidoId)
            .ToListAsync();
    }

    public async Task LimparDadosUsuarioAsync(Guid usuarioId)
    {
        await InicializarAsync();
        var pedidos = await _database!.Table<PedidoLocal>().Where(p => p.UsuarioId == usuarioId).ToListAsync();
        foreach (var p in pedidos)
        {
            await _database.Table<ItemPedidoLocal>().Where(i => i.PedidoId == p.Id).DeleteAsync();
            await _database.DeleteAsync(p);
        }
    }
}
