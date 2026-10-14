using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LimpeAgora.Shared.DTOs.Auth;
using LimpeAgoraApp.Services;

namespace LimpeAgoraApp.ViewModels;

public partial class CadastroViewModel : BaseViewModel
{
    private readonly IApiClientService _apiClient;

    [ObservableProperty]
    private string nomeCompleto = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string telefone = string.Empty;

    [ObservableProperty]
    private string senha = string.Empty;

    [ObservableProperty]
    private string confirmarSenha = string.Empty;

    public CadastroViewModel(IApiClientService apiClient)
    {
        _apiClient = apiClient;
        Titulo = "Cadastro de Cliente";
    }

    [RelayCommand]
    private async Task CadastrarAsync()
    {
        if (string.IsNullOrWhiteSpace(NomeCompleto) ||
            string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Telefone) ||
            string.IsNullOrWhiteSpace(Senha))
        {
            MensagemErro = "Todos os campos são obrigatórios.";
            return;
        }

        if (Senha != ConfirmarSenha)
        {
            MensagemErro = "As senhas não coincidem.";
            return;
        }

        if (Senha.Length < 6)
        {
            MensagemErro = "A senha deve conter no mínimo 6 caracteres.";
            return;
        }

        IsBusy = true;
        MensagemErro = null;

        try
        {
            var req = new CadastrarClienteRequest
            {
                NomeCompleto = NomeCompleto.Trim(),
                Email = Email.Trim(),
                Telefone = Telefone.Trim(),
                Senha = Senha,
                ConfirmarSenha = ConfirmarSenha
            };

            await _apiClient.PostAsync<CadastrarClienteRequest, AuthResponse>("api/auth/register", req);

            await Shell.Current.DisplayAlertAsync("Sucesso", "Conta criada com sucesso! Faça seu login para continuar.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            MensagemErro = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task VoltarAoLoginAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
