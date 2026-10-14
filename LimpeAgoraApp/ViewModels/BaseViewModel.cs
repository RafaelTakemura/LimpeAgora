using CommunityToolkit.Mvvm.ComponentModel;

namespace LimpeAgoraApp.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? titulo;

    [ObservableProperty]
    private string? mensagemErro;

    [ObservableProperty]
    private bool dadosLocaisDesatualizados;
}
