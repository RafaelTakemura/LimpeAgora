using LimpeAgoraApp.ViewModels;

namespace LimpeAgoraApp.Views;

public partial class CadastroPage : ContentPage
{
    public CadastroPage(CadastroViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
