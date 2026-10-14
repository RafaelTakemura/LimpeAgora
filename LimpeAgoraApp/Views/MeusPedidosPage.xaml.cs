using LimpeAgoraApp.ViewModels;

namespace LimpeAgoraApp.Views;

public partial class MeusPedidosPage : ContentPage
{
    private readonly MeusPedidosViewModel _viewModel;

    public MeusPedidosPage(MeusPedidosViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.CarregarPedidosCommand.ExecuteAsync(null);
    }
}
