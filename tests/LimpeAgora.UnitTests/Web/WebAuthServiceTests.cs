using System.Security.Claims;
using LimpeAgora.Shared.DTOs.Usuarios;
using LimpeAgora.Shared.Enums;
using LimpeAgoraWeb.Services;
using Xunit;

namespace LimpeAgora.UnitTests.Web;

public class WebAuthServiceTests
{
    [Fact]
    public async Task CustomAuthenticationStateProvider_QuandoNaoAutenticado_DeveRetornarUsuarioAnonimo()
    {
        // Arrange
        var session = new AuthSessionService();
        var provider = new CustomAuthenticationStateProvider(session);

        // Act
        var state = await provider.GetAuthenticationStateAsync();

        // Assert
        Assert.NotNull(state);
        Assert.NotNull(state.User);
        Assert.False(state.User.Identity?.IsAuthenticated ?? false);
        Assert.False(session.IsAuthenticated);
    }

    [Fact]
    public async Task CustomAuthenticationStateProvider_AposIniciarSessao_DeveRetornarClaimsCorretasEAutenticado()
    {
        // Arrange
        var session = new AuthSessionService();
        var provider = new CustomAuthenticationStateProvider(session);
        var usuario = new UsuarioResponse
        {
            Id = Guid.NewGuid(),
            NomeCompleto = "Administrador Silva",
            Email = "admin@limpeagora.com",
            Telefone = "11988887777",
            Perfil = PerfilUsuario.Administrador,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        // Act
        session.IniciarSessao("token-jwt-falso-123", usuario);
        var state = await provider.GetAuthenticationStateAsync();

        // Assert
        Assert.True(session.IsAuthenticated);
        Assert.True(session.IsAdmin);
        Assert.True(session.IsFuncionario);

        Assert.NotNull(state.User.Identity);
        Assert.True(state.User.Identity.IsAuthenticated);
        Assert.Equal("LimpeAgoraJwtAuth", state.User.Identity.AuthenticationType);

        Assert.Equal(usuario.Id.ToString(), state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal("Administrador Silva", state.User.FindFirst(ClaimTypes.Name)?.Value);
        Assert.Equal("admin@limpeagora.com", state.User.FindFirst(ClaimTypes.Email)?.Value);
        Assert.Equal(PerfilUsuario.Administrador.ToString(), state.User.FindFirst(ClaimTypes.Role)?.Value);
    }

    [Fact]
    public async Task CustomAuthenticationStateProvider_AposEncerrarSessao_DeveVoltarParaAnonimo()
    {
        // Arrange
        var session = new AuthSessionService();
        var provider = new CustomAuthenticationStateProvider(session);
        var usuario = new UsuarioResponse
        {
            Id = Guid.NewGuid(),
            NomeCompleto = "Funcionario Teste",
            Email = "func@limpeagora.com",
            Perfil = PerfilUsuario.Funcionario,
            Ativo = true
        };
        session.IniciarSessao("token-123", usuario);
        Assert.True(session.IsAuthenticated);

        // Act
        session.EncerrarSessao();
        var state = await provider.GetAuthenticationStateAsync();

        // Assert
        Assert.False(session.IsAuthenticated);
        Assert.Null(session.Token);
        Assert.Null(session.Usuario);
        Assert.False(state.User.Identity?.IsAuthenticated ?? false);
    }

    [Theory]
    [InlineData(PerfilUsuario.Administrador, true, true)]
    [InlineData(PerfilUsuario.Funcionario, false, true)]
    [InlineData(PerfilUsuario.Cliente, false, false)]
    public void AuthSessionService_PerfisDeAcesso_DeveAvaliarAdminEFuncionarioCorretamente(
        PerfilUsuario perfil,
        bool esperaIsAdmin,
        bool esperaIsFuncionario)
    {
        // Arrange
        var session = new AuthSessionService();
        var usuario = new UsuarioResponse
        {
            Id = Guid.NewGuid(),
            NomeCompleto = "Usuario Perfil Teste",
            Email = "teste@limpeagora.com",
            Perfil = perfil,
            Ativo = true
        };

        // Act
        session.IniciarSessao("fake-token", usuario);

        // Assert
        Assert.Equal(esperaIsAdmin, session.IsAdmin);
        Assert.Equal(esperaIsFuncionario, session.IsFuncionario);
    }
}
