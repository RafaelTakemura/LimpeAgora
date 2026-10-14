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
            if (_apiClient.IsOnline)
            {
                var remotas = await _apiClient.GetAsync<List<CategoriaResponse>>("api/categorias?apenasAtivas=true");
                if (remotas != null)
                {
                    var locais = remotas.Select(c => new CategoriaLocal
                    {
                        Id = c.Id,
                        Nome = c.Nome,
                        Descricao = c.Descricao,
                        Ativa = c.Ativa,
                        CriadoEm = c.CriadoEm,
                        AtualizadoEmLocal = DateTime.UtcNow
                    }).ToList();

                    await _localDb.SalvarCategoriasAsync(locais);

                    Categorias.Clear();
                    foreach (var c in locais)
                    {
                        Categorias.Add(c);
                    }
                    return;
                }
            }

            // Fallback para cache SQLite local caso API esteja offline
            DadosLocaisDesatualizados = true;
            var categoriasCache = await _localDb.ObterCategoriasAsync();
            Categorias.Clear();
            foreach (var c in categoriasCache)
            {
                Categorias.Add(c);
            }
        }
        catch (Exception ex)
        {
            // Em caso de erro de rede, tenta recuperar do cache local
            DadosLocaisDesatualizados = true;
            var categoriasCache = await _localDb.ObterCategoriasAsync();
            Categorias.Clear();
            foreach (var c in categoriasCache)
            {
                Categorias.Add(c);
            }

            if (!Categorias.Any())
            {
                MensagemErro = $"Falha ao carregar categorias: {ex.Message}";
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
