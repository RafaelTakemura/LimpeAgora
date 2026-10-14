using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LimpeAgora.Shared.DTOs.Auth;
using LimpeAgoraApp.Services;

namespace LimpeAgoraApp.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IApiClientService _apiClient;
    private readonly IAuthStorageService _authStorage;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string senha = string.Empty;

    public LoginViewModel(IApiClientService apiClient, IAuthStorageService authStorage)
    {
        _apiClient = apiClient;
        _authStorage = authStorage;
        Titulo = "LimpeAgora — Login";
    }

    [RelayCommand]
    private async Task RealizarLoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
        {
            MensagemErro = "Informe seu e-mail e senha.";
            return;
        }

        IsBusy = true;
        MensagemErro = null;

        try
        {
            var req = new LoginRequest { Email = Email.Trim(), Senha = Senha };
            var response = await _apiClient.PostAsync<LoginRequest, AuthResponse>("api/auth/login", req);

            if (response != null && !string.IsNullOrEmpty(response.Token))
            {
                await _authStorage.SalvarTokenAsync(response.Token);
                await _authStorage.SalvarUsuarioInfoAsync(response.Usuario.Id, response.Usuario.NomeCompleto, response.Usuario.Email);

                // Navega para a tela principal
                await Shell.Current.GoToAsync("//CategoriasPage");
            }
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
    private async Task IrParaCadastroAsync()
    {
        await Shell.Current.GoToAsync("CadastroPage");
    }
}
