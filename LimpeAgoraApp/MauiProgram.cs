using LimpeAgoraApp.Services;
using LimpeAgoraApp.ViewModels;
using LimpeAgoraApp.Views;
using Microsoft.Extensions.Logging;

namespace LimpeAgoraApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Serviços (Infraestrutura e Cache SQLite)
        builder.Services.AddSingleton<ILocalDatabaseService, LocalDatabaseService>();
        builder.Services.AddSingleton<IAuthStorageService, AuthStorageService>();
        builder.Services.AddSingleton<IApiClientService, ApiClientService>();
        builder.Services.AddSingleton<ICarrinhoService, CarrinhoService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<CadastroViewModel>();
        builder.Services.AddTransient<CategoriasViewModel>();
        builder.Services.AddTransient<ProdutosViewModel>();
        builder.Services.AddTransient<MeusPedidosViewModel>();

        // Views
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<CadastroPage>();
        builder.Services.AddTransient<CategoriasPage>();
        builder.Services.AddTransient<ProdutosPage>();
        builder.Services.AddTransient<MeusPedidosPage>();

        return builder.Build();
    }
}
