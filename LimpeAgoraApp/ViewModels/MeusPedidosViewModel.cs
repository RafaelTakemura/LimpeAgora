using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LimpeAgora.Shared.DTOs.Pedidos;
using LimpeAgora.Shared.Enums;
using LimpeAgoraApp.Models;
using LimpeAgoraApp.Services;

namespace LimpeAgoraApp.ViewModels;

public partial class MeusPedidosViewModel : BaseViewModel
{
    private readonly IApiClientService _apiClient;
    private readonly ILocalDatabaseService _localDb;
    private readonly IAuthStorageService _authStorage;

    public ObservableCollection<PedidoLocal> Pedidos { get; } = new();

    [ObservableProperty]
    private PedidoDetalheResponse? pedidoDetalheSelecionado;

    [ObservableProperty]
    private bool modalDetalhesAberto;

    public MeusPedidosViewModel(
        IApiClientService apiClient,
        ILocalDatabaseService localDb,
        IAuthStorageService authStorage)
    {
        _apiClient = apiClient;
        _localDb = localDb;
        _authStorage = authStorage;
        Titulo = "Meus Pedidos";
    }

    [RelayCommand]
    public async Task CarregarPedidosAsync()
    {
        IsBusy = true;
        MensagemErro = null;
        DadosLocaisDesatualizados = false;

        var (userId, _, _) = await _authStorage.ObterUsuarioInfoAsync();
        if (!userId.HasValue)
        {
            await Shell.Current.GoToAsync("//LoginPage");
            return;
        }

        try
        {
            var remotas = await _apiClient.GetAsync<List<PedidoResumoResponse>>("api/pedidos/meus");
            if (remotas != null)
            {
                var locais = remotas.Select(p => new PedidoLocal
                {
                    Id = p.Id,
                    UsuarioId = p.UsuarioId,
                    NomeCliente = p.NomeCliente,
                    DataPedido = p.DataPedido,
                    Status = p.Status,
                    StatusDescricao = p.StatusDescricao,
                    ValorTotal = p.ValorTotal,
                    TotalItens = p.TotalItens,
                    Observacao = p.Observacao,
                    SincronizadoEm = DateTime.UtcNow
                }).ToList();

                await _localDb.SalvarPedidosAsync(userId.Value, locais);

                Pedidos.Clear();
                foreach (var p in locais)
                {
                    Pedidos.Add(p);
                }

                DadosLocaisDesatualizados = false;
                return;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MeusPedidosViewModel] Erro ao carregar pedidos da API: {ex.Message}");
        }

        // Se a API não respondeu ou falhou, recorre ao cache local SQLite
        var pedidosCache = await _localDb.ObterPedidosAsync(userId.Value);
        Pedidos.Clear();
        foreach (var p in pedidosCache)
        {
            Pedidos.Add(p);
        }

        DadosLocaisDesatualizados = Pedidos.Count > 0;
        if (Pedidos.Count == 0)
        {
            MensagemErro = "Nenhum pedido encontrado.";
        }
    }

    [RelayCommand]
    private async Task VerDetalhesAsync(PedidoLocal pedido)
    {
        if (pedido == null) return;

        IsBusy = true;
        try
        {
            try
            {
                var detalhe = await _apiClient.GetAsync<PedidoDetalheResponse>($"api/pedidos/{pedido.Id}");
                if (detalhe != null)
                {
                    PedidoDetalheSelecionado = detalhe;
                    ModalDetalhesAberto = true;

                    // Atualiza cache dos itens no SQLite
                    var itensLocais = detalhe.Itens.Select(i => new ItemPedidoLocal
                    {
                        Id = i.Id,
                        PedidoId = detalhe.Id,
                        ProdutoId = i.ProdutoId,
                        NomeProduto = i.NomeProduto,
                        Quantidade = i.Quantidade,
                        PrecoUnitario = i.PrecoUnitario
                    });
                    await _localDb.SalvarItensPedidoAsync(detalhe.Id, itensLocais);
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MeusPedidosViewModel] Erro ao carregar detalhes online: {ex.Message}");
            }

            // Fallback SQLite para itens
            var itensCache = await _localDb.ObterItensPedidoAsync(pedido.Id);
            PedidoDetalheSelecionado = new PedidoDetalheResponse
            {
                Id = pedido.Id,
                UsuarioId = pedido.UsuarioId,
                NomeCliente = pedido.NomeCliente,
                DataPedido = pedido.DataPedido,
                Status = pedido.Status,
                ValorTotal = pedido.ValorTotal,
                Observacao = pedido.Observacao,
                Itens = itensCache.Select(i => new ItemPedidoResponse
                {
                    Id = i.Id,
                    ProdutoId = i.ProdutoId,
                    NomeProduto = i.NomeProduto,
                    Quantidade = i.Quantidade,
                    PrecoUnitario = i.PrecoUnitario
                }).ToList()
            };
            ModalDetalhesAberto = true;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Não foi possível carregar os detalhes: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void FecharModalDetalhes()
    {
        ModalDetalhesAberto = false;
        PedidoDetalheSelecionado = null;
    }

    [RelayCommand]
    private async Task CancelarPedidoAsync(PedidoLocal pedido)
    {
        if (pedido == null) return;

        if (pedido.Status != StatusPedido.Pendente)
        {
            await Shell.Current.DisplayAlertAsync("Aviso", "Apenas pedidos pendentes podem ser cancelados.", "OK");
            return;
        }

        if (!_apiClient.IsOnline)
        {
            await Shell.Current.DisplayAlertAsync("Sem Conexão", "É necessário estar conectado à internet para cancelar um pedido.", "OK");
            return;
        }

        bool confirmar = await Shell.Current.DisplayAlertAsync(
            "Confirmar Cancelamento",
            "Deseja realmente cancelar este pedido? As quantidades serão devolvidas ao estoque.",
            "Sim, Cancelar",
            "Não");

        if (!confirmar) return;

        IsBusy = true;
        try
        {
            await _apiClient.PostAsync($"api/pedidos/{pedido.Id}/cancelamento", new { });
            await Shell.Current.DisplayAlertAsync("Sucesso", "Pedido cancelado com sucesso.", "OK");
            FecharModalDetalhes();
            await CarregarPedidosAsync();
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

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var (userId, _, _) = await _authStorage.ObterUsuarioInfoAsync();
        if (userId.HasValue)
        {
            await _localDb.LimparDadosUsuarioAsync(userId.Value);
        }

        await _authStorage.LimparSessaoAsync();
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
