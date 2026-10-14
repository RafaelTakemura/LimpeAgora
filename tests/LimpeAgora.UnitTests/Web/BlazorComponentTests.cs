using Bunit;
using LimpeAgoraWeb.Components.Pages;
using LimpeAgoraWeb.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LimpeAgora.UnitTests.Web;

public class BlazorComponentTests : BunitContext
{
    [Fact]
    public void LoginPage_RenderizacaoInicial_DeveConterCamposEmailSenhaEBotaoEntrar()
    {
        // Arrange
        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5025") };
        var sessionService = new AuthSessionService();
        var apiClient = new ApiClient(httpClient, sessionService);
        var authWebService = new AuthWebService(apiClient, sessionService);

        Services.AddSingleton(sessionService);
        Services.AddSingleton(apiClient);
        Services.AddSingleton(authWebService);

        // Act: Renderiza o componente Blazor Login.razor
        var cut = Render<Login>();

        // Assert: Valida a árvore DOM gerada pelo componente
        var emailInput = cut.Find("input[placeholder='nome@exemplo.com']");
        var senhaInput = cut.Find("input[type='password']");
        var submitButton = cut.Find("button[type='submit']");

        Assert.NotNull(emailInput);
        Assert.NotNull(senhaInput);
        Assert.Contains("Entrar no Sistema", submitButton.TextContent);
        Assert.Contains("LimpeAgora", cut.Markup);
        Assert.Contains("Painel Administrativo da Loja", cut.Markup);
        Assert.Contains("Acesso exclusivo para funcionários e administradores", cut.Markup);
    }
}
