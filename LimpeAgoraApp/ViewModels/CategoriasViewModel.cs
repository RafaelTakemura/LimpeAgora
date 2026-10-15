using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LimpeAgora.Shared.DTOs.Categorias;
using LimpeAgoraApp.Models;
using LimpeAgoraApp.Services;

namespace LimpeAgoraApp.ViewModels;

public partial class CategoriasViewModel : BaseViewModel
{
    private readonly IApiClientService _apiClient;
    private readonly ILocalDatabaseService _localDb;

    public ObservableCollection<CategoriaLocal> Categorias { get; } = new();

    public CategoriasViewModel(IApiClientService apiClient, ILocalDatabaseService localDb)
    {
        _apiClient = apiClient;
        _localDb = localDb;
        Titulo = "Categorias de Produtos";
    }

    [RelayCommand]
    public async Task CarregarCategoriasAsync()
    {
        IsBusy = true;
        MensagemErro = null;
        DadosLocaisDesatualizados = false;

        try
        {
            List<CategoriaResponse>? remotas = null;
            try
            {
                remotas = await _apiClient.GetAsync<List<CategoriaResponse>>("api/categorias?apenasAtivas=true");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CategoriasViewModel] API indisponível: {ex}");
            }

            if (remotas != null)
            {
                // API respondeu: exibe dados online, independentemente do resultado do cache
                var locais = remotas.Select(c => new CategoriaLocal
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Descricao = c.Descricao,
                    Ativa = c.Ativa,
                    CriadoEm = c.CriadoEm,
                    AtualizadoEmLocal = DateTime.UtcNow
                }).ToList();

                Categorias.Clear();
                foreach (var c in locais)
                {
                    Categorias.Add(c);
                }
                DadosLocaisDesatualizados = false;

                try { await _localDb.SalvarCategoriasAsync(locais); }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[CategoriasViewModel] Falha ao gravar cache: {ex}"); }
                return;
            }

            // API realmente inacessível: usa cache SQLite
            var categoriasCache = await _localDb.ObterCategoriasAsync();
            Categorias.Clear();
            foreach (var c in categoriasCache)
            {
                Categorias.Add(c);
            }

            DadosLocaisDesatualizados = Categorias.Count > 0;
            if (Categorias.Count == 0)
            {
                MensagemErro = "Não foi possível carregar as categorias da API e não há dados em cache local.";
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelecionarCategoriaAsync(CategoriaLocal categoria)
    {
        if (categoria == null) return;
        await Shell.Current.GoToAsync($"//ProdutosPage?categoriaId={categoria.Id}&nomeCategoria={Uri.EscapeDataString(categoria.Nome)}");
    }
}
