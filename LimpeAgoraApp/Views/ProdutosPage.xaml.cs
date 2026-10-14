using LimpeAgoraApp.ViewModels;

namespace LimpeAgoraApp.Views;

public partial class ProdutosPage : ContentPage
{
    private readonly ProdutosViewModel _viewModel;

    public ProdutosPage(ProdutosViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CarregarProdutosCommand.ExecuteAsync(null);
    }
}
