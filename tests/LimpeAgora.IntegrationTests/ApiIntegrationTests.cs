using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LimpeAgora.Shared.DTOs.Auth;
using LimpeAgora.Shared.DTOs.Categorias;
using LimpeAgora.Shared.DTOs.Pedidos;
using LimpeAgora.Shared.DTOs.Produtos;
using LimpeAgora.Shared.DTOs.Usuarios;
using LimpeAgora.Shared.Enums;
using LimpeAgoraApi.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LimpeAgora.IntegrationTests;

public class ApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CadastrarCliente_ComDadosValidos_DeveRetornar201CreatedEToken()
    {
        // Arrange
        var request = new CadastrarClienteRequest
        {
            NomeCompleto = "Cliente Integracao",
            Email = $"cliente_{Guid.NewGuid()}@teste.com",
            Telefone = "11987654321",
            Senha = "senhaForte123",
            ConfirmarSenha = "senhaForte123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var authResult = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authResult);
        Assert.False(string.IsNullOrEmpty(authResult.Token));
        Assert.Equal(PerfilUsuario.Cliente, authResult.Usuario.Perfil);
    }

    [Fact]
    public async Task CadastrarCliente_ComEmailDuplicado_DeveRetornar409Conflict()
    {
        // Arrange
        var email = $"duplicado_{Guid.NewGuid()}@teste.com";
        var request = new CadastrarClienteRequest
        {
            NomeCompleto = "Usuario Original",
            Email = email,
            Telefone = "11911112222",
            Senha = "senhaOriginal123",
            ConfirmarSenha = "senhaOriginal123"
        };

        // Act 1: Primeiro cadastro
        var res1 = await _client.PostAsJsonAsync("api/auth/register", request);
        Assert.Equal(HttpStatusCode.Created, res1.StatusCode);

        // Act 2: Tentar cadastrar com o mesmo e-mail
        var res2 = await _client.PostAsJsonAsync("api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, res2.StatusCode);
    }

    [Fact]
    public async Task RealizarLogin_ComCredenciaisValidas_DeveRetornar200OK()
    {
        // Arrange
        var email = $"login_{Guid.NewGuid()}@teste.com";
        var senha = "senhaLogin123";

        await _client.PostAsJsonAsync("api/auth/register", new CadastrarClienteRequest
        {
            NomeCompleto = "Cliente Login",
            Email = email,
            Telefone = "11955554444",
            Senha = senha,
            ConfirmarSenha = senha
        });

        // Act
        var loginResponse = await _client.PostAsJsonAsync("api/auth/login", new LoginRequest
        {
            Email = email,
            Senha = senha
        });

        // Assert
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrEmpty(auth.Token));
    }

    [Fact]
    public async Task RealizarLogin_ComSenhaIncorreta_DeveRetornar401Unauthorized()
    {
        // Arrange
        var email = $"errosenha_{Guid.NewGuid()}@teste.com";
        await _client.PostAsJsonAsync("api/auth/register", new CadastrarClienteRequest
        {
            NomeCompleto = "Cliente Erro",
            Email = email,
            Telefone = "11944443333",
            Senha = "senhaCorreta123",
            ConfirmarSenha = "senhaCorreta123"
        });

        // Act
        var loginResponse = await _client.PostAsJsonAsync("api/auth/login", new LoginRequest
        {
            Email = email,
            Senha = "senhaErrada999"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    [Fact]
    public async Task FluxoCompleto_CriacaoPedido_DescontoEstoque_TransicaoStatusEHistorico()
    {
        // 1. Criar e autenticar um cliente
        var emailCliente = $"fluxo_cliente_{Guid.NewGuid()}@teste.com";
        var regCliente = await _client.PostAsJsonAsync("api/auth/register", new CadastrarClienteRequest
        {
            NomeCompleto = "Cliente Fluxo",
            Email = emailCliente,
            Telefone = "11922221111",
            Senha = "senhaCliente123",
            ConfirmarSenha = "senhaCliente123"
        });
        var authCliente = await regCliente.Content.ReadFromJsonAsync<AuthResponse>();

        // 2. Criar cliente com token do admin ou funcionário para cadastrar categoria e produto
        // Criamos o admin diretamente no banco ou via simulação de token
        var tokenAdmin = ObterTokenParaPerfil(PerfilUsuario.Administrador);
        var clientAdmin = _factory.CreateClient();
        clientAdmin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAdmin);

        // Criar categoria
        var catRes = await clientAdmin.PostAsJsonAsync("api/categorias", new CategoriaUpsertRequest
        {
            Nome = $"Cat_{Guid.NewGuid().ToString().Substring(0, 6)}",
            Descricao = "Categoria de Teste Integrado"
        });
        Assert.Equal(HttpStatusCode.Created, catRes.StatusCode);
        var categoria = await catRes.Content.ReadFromJsonAsync<CategoriaResponse>();

        // Criar produto com 10 unidades em estoque a R$ 15,00
        var prodRes = await clientAdmin.PostAsJsonAsync("api/produtos", new CriarProdutoRequest
        {
            CategoriaId = categoria!.Id,
            Nome = "Detergente Concentrado Integracao",
            Apresentacao = "1 L",
            Preco = 15.00m,
            QuantidadeEstoque = 10,
            Ativo = true
        });
        Assert.Equal(HttpStatusCode.Created, prodRes.StatusCode);
        var produto = await prodRes.Content.ReadFromJsonAsync<ProdutoResponse>();

        // 3. Cliente cria pedido solicitando 4 unidades
        var clientUsuario = _factory.CreateClient();
        clientUsuario.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authCliente!.Token);

        var pedidoRes = await clientUsuario.PostAsJsonAsync("api/pedidos", new CriarPedidoRequest
        {
            Observacao = "Retirada até às 17h",
            Itens = new List<ItemPedidoInput>
            {
                new() { ProdutoId = produto!.Id, Quantidade = 4 }
            }
        });

        Assert.Equal(HttpStatusCode.Created, pedidoRes.StatusCode);
        var pedido = await pedidoRes.Content.ReadFromJsonAsync<PedidoDetalheResponse>();
        Assert.NotNull(pedido);
        Assert.Equal(StatusPedido.Pendente, pedido.Status);
        Assert.Equal(60.00m, pedido.ValorTotal); // 4 * 15.00

        // 4. Verificar se o estoque foi descontado de 10 para 6 unidades
        var prodAtualizadoRes = await _client.GetAsync($"api/produtos/{produto.Id}");
        var prodAtualizado = await prodAtualizadoRes.Content.ReadFromJsonAsync<ProdutoResponse>();
        Assert.Equal(6, prodAtualizado!.QuantidadeEstoque);

        // 5. Funcionário avança status: Pendente -> EmPreparacao -> ProntoParaRetirada -> Concluido
        var clientFuncionario = _factory.CreateClient();
        clientFuncionario.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAdmin);

        // Transição 1
        var status1 = await clientFuncionario.PatchAsJsonAsync($"api/pedidos/{pedido.Id}/status", new AlterarStatusPedidoRequest
        {
            NovoStatus = StatusPedido.EmPreparacao
        });
        Assert.Equal(HttpStatusCode.OK, status1.StatusCode);

        // Transição 2
        var status2 = await clientFuncionario.PatchAsJsonAsync($"api/pedidos/{pedido.Id}/status", new AlterarStatusPedidoRequest
        {
            NovoStatus = StatusPedido.ProntoParaRetirada
        });
        Assert.Equal(HttpStatusCode.OK, status2.StatusCode);

        // Transição 3 (Conclusão após pagamento presencial)
        var status3 = await clientFuncionario.PatchAsJsonAsync($"api/pedidos/{pedido.Id}/status", new AlterarStatusPedidoRequest
        {
            NovoStatus = StatusPedido.Concluido
        });
        Assert.Equal(HttpStatusCode.OK, status3.StatusCode);

        // 6. Verificar histórico de transições
        var histRes = await clientFuncionario.GetAsync($"api/pedidos/{pedido.Id}/historico");
        Assert.Equal(HttpStatusCode.OK, histRes.StatusCode);
        var historico = await histRes.Content.ReadFromJsonAsync<List<HistoricoStatusResponse>>();
        Assert.NotNull(historico);
        Assert.True(historico.Count >= 3);
    }

    [Fact]
    public async Task CancelarPedidoPendente_DeveDevolverEstoqueAoProduto()
    {
        // 1. Criar produto com 5 unidades
        var tokenAdmin = ObterTokenParaPerfil(PerfilUsuario.Administrador);
        var clientAdmin = _factory.CreateClient();
        clientAdmin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAdmin);

        var catRes = await clientAdmin.PostAsJsonAsync("api/categorias", new CategoriaUpsertRequest
        {
            Nome = $"CatEstorno_{Guid.NewGuid().ToString().Substring(0, 6)}"
        });
        var categoria = await catRes.Content.ReadFromJsonAsync<CategoriaResponse>();

        var prodRes = await clientAdmin.PostAsJsonAsync("api/produtos", new CriarProdutoRequest
        {
            CategoriaId = categoria!.Id,
            Nome = "Desinfetante Pinheiro",
            Apresentacao = "2 L",
            Preco = 8.50m,
            QuantidadeEstoque = 5,
            Ativo = true
        });
        var produto = await prodRes.Content.ReadFromJsonAsync<ProdutoResponse>();

        // 2. Cliente cria pedido de 3 unidades (estoque vai para 2)
        var emailCliente = $"cancela_{Guid.NewGuid()}@teste.com";
        var reg = await _client.PostAsJsonAsync("api/auth/register", new CadastrarClienteRequest
        {
            NomeCompleto = "Cliente Cancelador",
            Email = emailCliente,
            Telefone = "11988887777",
            Senha = "senhaCliente123",
            ConfirmarSenha = "senhaCliente123"
        });
        var authCliente = await reg.Content.ReadFromJsonAsync<AuthResponse>();

        var clientUsuario = _factory.CreateClient();
        clientUsuario.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authCliente!.Token);

        var pedidoRes = await clientUsuario.PostAsJsonAsync("api/pedidos", new CriarPedidoRequest
        {
            Itens = new List<ItemPedidoInput>
            {
                new() { ProdutoId = produto!.Id, Quantidade = 3 }
            }
        });
        var pedido = await pedidoRes.Content.ReadFromJsonAsync<PedidoDetalheResponse>();

        // Checa estoque reduzido
        var ppos = await (await _client.GetAsync($"api/produtos/{produto.Id}")).Content.ReadFromJsonAsync<ProdutoResponse>();
        Assert.Equal(2, ppos!.QuantidadeEstoque);

        // 3. Cliente cancela o pedido pendente
        var cancelRes = await clientUsuario.PostAsJsonAsync($"api/pedidos/{pedido!.Id}/cancelamento", new { });
        Assert.Equal(HttpStatusCode.NoContent, cancelRes.StatusCode);

        // 4. Estoque deve voltar a 5 unidades
        var pestornado = await (await _client.GetAsync($"api/produtos/{produto.Id}")).Content.ReadFromJsonAsync<ProdutoResponse>();
        Assert.Equal(5, pestornado!.QuantidadeEstoque);
    }

    [Fact]
    public async Task ImpedirClienteDeAlterarStatusAdministrativo_DeveRetornar403Forbidden()
    {
        // Arrange
        var reg = await _client.PostAsJsonAsync("api/auth/register", new CadastrarClienteRequest
        {
            NomeCompleto = "Cliente Curioso",
            Email = $"curioso_{Guid.NewGuid()}@teste.com",
            Telefone = "11977776666",
            Senha = "senhaCliente123",
            ConfirmarSenha = "senhaCliente123"
        });
        var auth = await reg.Content.ReadFromJsonAsync<AuthResponse>();

        var clientCliente = _factory.CreateClient();
        clientCliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        // Act: Tentar alterar status sendo apenas Cliente
        var res = await clientCliente.PatchAsJsonAsync($"api/pedidos/{Guid.NewGuid()}/status", new AlterarStatusPedidoRequest
        {
            NovoStatus = StatusPedido.Concluido
        });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    private string ObterTokenParaPerfil(PerfilUsuario perfil)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LimpeAgoraDbContext>();
        var tokenService = scope.ServiceProvider.GetRequiredService<LimpeAgoraApi.Services.Interfaces.ITokenService>();

        var usuario = new LimpeAgoraApi.Entities.Usuario
        {
            Id = Guid.NewGuid(),
            NomeCompleto = perfil == PerfilUsuario.Administrador ? "Administrador Teste" : "Funcionario Teste",
            Email = $"{perfil.ToString().ToLower()}_{Guid.NewGuid():N}@teste.com",
            Telefone = "11999998888",
            SenhaHash = "hash-qualquer",
            Perfil = perfil,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        db.Usuarios.Add(usuario);
        db.SaveChanges();

        var (token, _) = tokenService.GerarToken(usuario);
        return token;
    }
}
