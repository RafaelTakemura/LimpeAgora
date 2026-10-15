using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LimpeAgora.Shared.DTOs.Pedidos;
using LimpeAgora.Shared.DTOs.Produtos;
using LimpeAgoraApp.Models;
using LimpeAgoraApp.Services;

namespace LimpeAgoraApp.ViewModels;

[QueryProperty(nameof(CategoriaIdString), "categoriaId")]
[QueryProperty(nameof(NomeCategoria), "nomeCategoria")]
public partial class ProdutosViewModel : BaseViewModel
{
    private readonly IApiClientService _apiClient;
    private readonly ILocalDatabaseService _localDb;
    private readonly ICarrinhoService _carrinho;

    public ObservableCollection<ProdutoLocal> Produtos { get; } = new();
    public ObservableCollection<ItemCarrinho> ItensCarrinho => _carrinho.Itens;

    [ObservableProperty]
    private string? categoriaIdString;

    [ObservableProperty]
    private string? nomeCategoria;

    [ObservableProperty]
    private bool modalCarrinhoAberto;

    [ObservableProperty]
    private string? observacaoPedido;

    [ObservableProperty]
    private decimal valorTotalCarrinho;

    [ObservableProperty]
    private int totalItensCarrinho;

    public ProdutosViewModel(
        IApiClientService apiClient,
        ILocalDatabaseService localDb,
        ICarrinhoService carrinho)
    {
        _apiClient = apiClient;
        _localDb = localDb;
        _carrinho = carrinho;
        Titulo = "Catálogo de Produtos";

        _carrinho.OnCarrinhoAtualizado += AtualizarTotaisCarrinho;
    }

    private void AtualizarTotaisCarrinho()
    {
        ValorTotalCarrinho = _carrinho.ValorTotal;
        TotalItensCarrinho = _carrinho.TotalItens;
        SincronizarQuantidadesProdutos();

        if (_carrinho.TotalItens == 0 && ModalCarrinhoAberto)
        {
            ModalCarrinhoAberto = false;
        }
    }

    private void SincronizarQuantidadesProdutos()
    {
        foreach (var p in Produtos)
        {
            p.QuantidadeNoCarrinho = _carrinho.ObterQuantidadeDoProduto(p.Id);
        }
    }

    [RelayCommand]
    public async Task CarregarProdutosAsync()
    {
        IsBusy = true;
        MensagemErro = null;
        DadosLocaisDesatualizados = false;

        Guid? catId = Guid.TryParse(CategoriaIdString, out var g) ? g : null;

        try
        {
            var endpoint = catId.HasValue
                ? $"api/produtos?apenasAtivos=true&categoriaId={catId.Value}"
                : "api/produtos?apenasAtivos=true";

            List<ProdutoResponse>? remotas = null;
            try
            {
                remotas = await _apiClient.GetAsync<List<ProdutoResponse>>(endpoint);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProdutosViewModel] API indisponível: {ex}");
            }

            if (remotas != null)
            {
                // API respondeu: exibe dados online, independentemente do resultado do cache
                var locais = remotas.Select(p => new ProdutoLocal
                {
                    Id = p.Id,
                    CategoriaId = p.CategoriaId,
                    NomeCategoria = p.NomeCategoria,
                    Nome = p.Nome,
                    Descricao = p.Descricao,
                    Apresentacao = p.Apresentacao,
                    Preco = p.Preco,
                    QuantidadeEstoque = p.QuantidadeEstoque,
                    ImagemUrl = p.ImagemUrl,
                    Ativo = p.Ativo,
                    AtualizadoEmLocal = DateTime.UtcNow
                }).ToList();

                Produtos.Clear();
                foreach (var p in locais)
                {
                    p.QuantidadeNoCarrinho = _carrinho.ObterQuantidadeDoProduto(p.Id);
                    Produtos.Add(p);
                }
                DadosLocaisDesatualizados = false;

                try { await _localDb.SalvarProdutosAsync(locais); }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[ProdutosViewModel] Falha ao gravar cache: {ex}"); }
                return;
            }

            // API realmente inacessível: usa cache SQLite
            var produtosCache = await _localDb.ObterProdutosAsync(catId);
            Produtos.Clear();
            foreach (var p in produtosCache)
            {
                p.QuantidadeNoCarrinho = _carrinho.ObterQuantidadeDoProduto(p.Id);
                Produtos.Add(p);
            }

            DadosLocaisDesatualizados = Produtos.Count > 0;
            if (Produtos.Count == 0)
            {
                MensagemErro = "Não foi possível carregar os produtos da API e não há dados em cache local.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void IncrementarProduto(ProdutoLocal produto)
    {
        if (produto == null) return;
        if (produto.QuantidadeEstoque <= 0)
        {
            Shell.Current.DisplayAlertAsync("Aviso", "Produto esgotado no momento.", "OK");
            return;
        }

        if (produto.QuantidadeNoCarrinho >= produto.QuantidadeEstoque)
        {
            Shell.Current.DisplayAlertAsync("Estoque Máximo", $"Quantidade máxima disponível em estoque: {produto.QuantidadeEstoque} un.", "OK");
            return;
        }

        _carrinho.AdicionarProduto(
            produto.Id,
            produto.Nome,
            produto.Apresentacao,
            produto.Preco,
            quantidade: 1,
            estoqueMax: produto.QuantidadeEstoque);
    }

    [RelayCommand]
    private void DecrementarProduto(ProdutoLocal produto)
    {
        if (produto == null) return;
        if (produto.QuantidadeNoCarrinho <= 0) return;

        _carrinho.SubtrairProduto(produto.Id, 1);
    }

    [RelayCommand]
    private void AdicionarAoCarrinho(ProdutoLocal produto)
    {
        IncrementarProduto(produto);
    }

    [RelayCommand]
    private void IncrementarItemCarrinho(ItemCarrinho item)
    {
        if (item == null) return;
        if (item.Quantidade >= item.QuantidadeEstoqueMax)
        {
            Shell.Current.DisplayAlertAsync("Estoque Máximo", $"Quantidade máxima disponível em estoque: {item.QuantidadeEstoqueMax} un.", "OK");
            return;
        }

        _carrinho.IncrementarItem(item.ProdutoId);
    }

    [RelayCommand]
    private void DecrementarItemCarrinho(ItemCarrinho item)
    {
        if (item == null) return;
        _carrinho.DecrementarItem(item.ProdutoId);
    }

    [RelayCommand]
    private void AbrirModalCarrinho()
    {
        if (!_carrinho.Itens.Any())
        {
            Shell.Current.DisplayAlertAsync("Aviso", "Seu carrinho está vazio.", "OK");
            return;
        }
        ModalCarrinhoAberto = true;
    }

    [RelayCommand]
    private void FecharModalCarrinho()
    {
        ModalCarrinhoAberto = false;
    }

    [RelayCommand]
    private void RemoverDoCarrinho(ItemCarrinho item)
    {
        if (item == null) return;
        _carrinho.RemoverProduto(item.ProdutoId);
    }

    [RelayCommand]
    private async Task ConfirmarPedidoAsync()
    {
        if (!_carrinho.Itens.Any())
        {
            await Shell.Current.DisplayAlertAsync("Aviso", "O carrinho está vazio. Adicione produtos para criar um pedido.", "OK");
            return;
        }

        if (!_apiClient.IsOnline)
        {
            await Shell.Current.DisplayAlertAsync("Sem Conexão", "É necessário estar conectado à internet para criar um pedido.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            var req = new CriarPedidoRequest
            {
                Observacao = ObservacaoPedido,
                Itens = _carrinho.Itens.Select(i => new ItemPedidoInput
                {
                    ProdutoId = i.ProdutoId,
                    Quantidade = i.Quantidade
                }).ToList()
            };

            await _apiClient.PostAsync<CriarPedidoRequest, PedidoDetalheResponse>("api/pedidos", req);

            _carrinho.LimparCarrinho();
            ModalCarrinhoAberto = false;
            ObservacaoPedido = string.Empty;

            await Shell.Current.DisplayAlertAsync("Pedido Realizado!", "Seu pedido foi registrado com sucesso. Você pagará à vista no momento da retirada.", "OK");
            await Shell.Current.GoToAsync("//MeusPedidosPage");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
