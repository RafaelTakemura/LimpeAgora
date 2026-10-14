using LimpeAgora.Shared.DTOs.Pedidos;
using LimpeAgora.Shared.Enums;
using LimpeAgoraApi.Data;
using LimpeAgoraApi.Entities;
using LimpeAgoraApi.Exceptions;
using LimpeAgoraApi.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace LimpeAgora.UnitTests;

public class PedidoServiceTests
{
    private LimpeAgoraDbContext CriarContextoEmMemoria()
    {
        var options = new DbContextOptionsBuilder<LimpeAgoraDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new LimpeAgoraDbContext(options);
    }

    [Fact]
    public async Task CriarPedido_SemItens_DeveLancarDomainException()
    {
        // Arrange
        using var context = CriarContextoEmMemoria();
        var service = new PedidoService(context);
        var request = new CriarPedidoRequest { Itens = new List<ItemPedidoInput>() };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            service.CriarPedidoAsync(Guid.NewGuid(), request));

        Assert.Contains("pelo menos um item", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task CriarPedido_ComQuantidadeZeroOuNegativa_DeveLancarDomainException(int quantidadeInvalida)
    {
        // Arrange
        using var context = CriarContextoEmMemoria();
        var service = new PedidoService(context);
        var request = new CriarPedidoRequest
        {
            Itens = new List<ItemPedidoInput>
            {
                new() { ProdutoId = Guid.NewGuid(), Quantidade = quantidadeInvalida }
            }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            service.CriarPedidoAsync(Guid.NewGuid(), request));

        Assert.Contains("inteiros positivos", ex.Message);
    }

    [Fact]
    public async Task CriarPedido_ComEstoqueSuficiente_DeveCriarEDescontarEstoqueECalcularTotal()
    {
        // Arrange
        using var context = CriarContextoEmMemoria();
        var cliente = new Usuario
        {
            Id = Guid.NewGuid(),
            NomeCompleto = "Cliente Teste",
            Email = "cliente@teste.com",
            Telefone = "11999999999",
            SenhaHash = "hash",
            Perfil = PerfilUsuario.Cliente,
            Ativo = true
        };
        var categoria = new Categoria { Id = Guid.NewGuid(), Nome = "Limpeza Geral", Ativa = true };
        var produto = new Produto
        {
            Id = Guid.NewGuid(),
            CategoriaId = categoria.Id,
            Nome = "Detergente Neutro",
            Apresentacao = "500 mL",
            Preco = 4.50m,
            QuantidadeEstoque = 10,
            Ativo = true
        };

        context.Usuarios.Add(cliente);
        context.Categorias.Add(categoria);
        context.Produtos.Add(produto);
        await context.SaveChangesAsync();

        var service = new PedidoService(context);
        var request = new CriarPedidoRequest
        {
            Itens = new List<ItemPedidoInput>
            {
                new() { ProdutoId = produto.Id, Quantidade = 3 }
            },
            Observacao = "Retirar à tarde"
        };

        // Act
        var pedido = await service.CriarPedidoAsync(cliente.Id, request);

        // Assert
        Assert.NotNull(pedido);
        Assert.Equal(StatusPedido.Pendente, pedido.Status);
        Assert.Equal(13.50m, pedido.ValorTotal); // 3 * 4.50
        Assert.Single(pedido.Itens);
        Assert.Equal(3, pedido.Itens.First().Quantidade);
        Assert.Equal(4.50m, pedido.Itens.First().PrecoUnitario);

        // Verifica desconto de estoque no banco
        var produtoAtualizado = await context.Produtos.FindAsync(produto.Id);
        Assert.Equal(7, produtoAtualizado!.QuantidadeEstoque); // 10 - 3
    }

    [Fact]
    public async Task CriarPedido_ComEstoqueInsuficiente_DeveRejeitarENaoModificarEstoque()
    {
        // Arrange
        using var context = CriarContextoEmMemoria();
        var cliente = new Usuario
        {
            Id = Guid.NewGuid(),
            NomeCompleto = "Cliente Teste",
            Email = "cliente@teste.com",
            Telefone = "11999999999",
            SenhaHash = "hash",
            Perfil = PerfilUsuario.Cliente,
            Ativo = true
        };
        var produto = new Produto
        {
            Id = Guid.NewGuid(),
            Nome = "Desinfetante Lavanda",
            Apresentacao = "1 L",
            Preco = 9.90m,
            QuantidadeEstoque = 2,
            Ativo = true
        };

        context.Usuarios.Add(cliente);
        context.Produtos.Add(produto);
        await context.SaveChangesAsync();

        var service = new PedidoService(context);
        var request = new CriarPedidoRequest
        {
            Itens = new List<ItemPedidoInput>
            {
                new() { ProdutoId = produto.Id, Quantidade = 5 } // Solicitando mais que 2
            }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessConflictException>(() =>
            service.CriarPedidoAsync(cliente.Id, request));

        Assert.Contains("Estoque insuficiente", ex.Message);

        var produtoNoBanco = await context.Produtos.FindAsync(produto.Id);
        Assert.Equal(2, produtoNoBanco!.QuantidadeEstoque); // Permanece intacto
    }

    [Fact]
    public async Task CriarPedido_ComProdutoInativo_DeveRejeitarPedido()
    {
        // Arrange
        using var context = CriarContextoEmMemoria();
        var cliente = new Usuario
        {
            Id = Guid.NewGuid(),
            NomeCompleto = "Cliente Teste",
            Email = "cliente@teste.com",
            Telefone = "11999999999",
            SenhaHash = "hash",
            Perfil = PerfilUsuario.Cliente,
            Ativo = true
        };
        var produtoInativo = new Produto
        {
            Id = Guid.NewGuid(),
            Nome = "Sabão Descontinuado",
            Apresentacao = "1 kg",
            Preco = 12.00m,
            QuantidadeEstoque = 50,
            Ativo = false // Inativo
        };

        context.Usuarios.Add(cliente);
        context.Produtos.Add(produtoInativo);
        await context.SaveChangesAsync();

        var service = new PedidoService(context);
        var request = new CriarPedidoRequest
        {
            Itens = new List<ItemPedidoInput>
            {
                new() { ProdutoId = produtoInativo.Id, Quantidade = 1 }
            }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessConflictException>(() =>
            service.CriarPedidoAsync(cliente.Id, request));

        Assert.Contains("inativo", ex.Message);
    }

    [Fact]
    public async Task CancelarPedido_PendentePeloTitular_DeveDevolverEstoqueRegistrarHistorico()
    {
        // Arrange
        using var context = CriarContextoEmMemoria();
        var clienteId = Guid.NewGuid();
        var cliente = new Usuario
        {
            Id = clienteId,
            NomeCompleto = "Cliente Titular",
            Email = "titular@teste.com",
            Telefone = "11999999999",
            SenhaHash = "hash",
            Perfil = PerfilUsuario.Cliente,
            Ativo = true
        };
        var produto = new Produto
        {
            Id = Guid.NewGuid(),
            Nome = "Água Sanitária",
            Apresentacao = "1 L",
            Preco = 6.00m,
            QuantidadeEstoque = 8,
            Ativo = true
        };

        context.Usuarios.Add(cliente);
        context.Produtos.Add(produto);
        await context.SaveChangesAsync();

        var service = new PedidoService(context);
        var pedido = await service.CriarPedidoAsync(clienteId, new CriarPedidoRequest
        {
            Itens = new List<ItemPedidoInput> { new() { ProdutoId = produto.Id, Quantidade = 4 } }
        });

        // Estoque após compra: 8 - 4 = 4
        var produtoPosCompra = await context.Produtos.FindAsync(produto.Id);
        Assert.Equal(4, produtoPosCompra!.QuantidadeEstoque);

        // Act
        await service.CancelarPedidoAsync(pedido.Id, clienteId, isFuncionarioOuAdmin: false);

        // Assert
        var pedidoCancelado = await context.Pedidos.FindAsync(pedido.Id);
        Assert.Equal(StatusPedido.Cancelado, pedidoCancelado!.Status);

        // Estoque deve ter sido estornado: 4 + 4 = 8
        var produtoEstornado = await context.Produtos.FindAsync(produto.Id);
        Assert.Equal(8, produtoEstornado!.QuantidadeEstoque);

        // Histórico deve registrar o cancelamento
        var historico = await context.HistoricosStatusPedido.Where(h => h.PedidoId == pedido.Id).ToListAsync();
        Assert.Contains(historico, h => h.StatusAnterior == StatusPedido.Pendente && h.StatusNovo == StatusPedido.Cancelado);
    }

    [Fact]
    public async Task CancelarPedido_EmPreparacao_DeveLancarBusinessConflictException()
    {
        // Arrange
        using var context = CriarContextoEmMemoria();
        var clienteId = Guid.NewGuid();
        var pedido = new Pedido
        {
            Id = Guid.NewGuid(),
            UsuarioId = clienteId,
            Status = StatusPedido.EmPreparacao, // Não é mais Pendente
            ValorTotal = 25.00m
        };
        context.Pedidos.Add(pedido);
        await context.SaveChangesAsync();

        var service = new PedidoService(context);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessConflictException>(() =>
            service.CancelarPedidoAsync(pedido.Id, clienteId, isFuncionarioOuAdmin: false));

        Assert.Contains("não podem ser cancelados", ex.Message);
    }

    [Fact]
    public async Task CancelarPedido_PorOutroCliente_DeveLancarDomainException()
    {
        // Arrange
        using var context = CriarContextoEmMemoria();
        var titularId = Guid.NewGuid();
        var outroClienteId = Guid.NewGuid();

        var pedido = new Pedido
        {
            Id = Guid.NewGuid(),
            UsuarioId = titularId,
            Status = StatusPedido.Pendente,
            ValorTotal = 50.00m
        };
        context.Pedidos.Add(pedido);
        await context.SaveChangesAsync();

        var service = new PedidoService(context);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            service.CancelarPedidoAsync(pedido.Id, outroClienteId, isFuncionarioOuAdmin: false));

        Assert.Contains("seus próprios pedidos", ex.Message);
    }

    [Fact]
    public async Task AlterarStatus_FluxoPermitido_PendenteParaEmPreparacaoParaProntoParaConcluido()
    {
        // Arrange
        using var context = CriarContextoEmMemoria();
        var funcionarioId = Guid.NewGuid();
        var funcionario = new Usuario
        {
            Id = funcionarioId,
            NomeCompleto = "Funcionario Balcao",
            Email = "func@loja.com",
            Telefone = "11988888888",
            SenhaHash = "hash",
            Perfil = PerfilUsuario.Funcionario,
            Ativo = true
        };
        var cliente = new Usuario
        {
            Id = Guid.NewGuid(),
            NomeCompleto = "Cliente Final",
            Email = "cliente@loja.com",
            Telefone = "11977777777",
            SenhaHash = "hash",
            Perfil = PerfilUsuario.Cliente,
            Ativo = true
        };
        var pedido = new Pedido
        {
            Id = Guid.NewGuid(),
            UsuarioId = cliente.Id,
            Status = StatusPedido.Pendente,
            ValorTotal = 100.00m
        };

        context.Usuarios.AddRange(funcionario, cliente);
        context.Pedidos.Add(pedido);
        await context.SaveChangesAsync();

        var service = new PedidoService(context);

        // Act & Assert 1: Pendente -> EmPreparacao
        var res1 = await service.AlterarStatusPedidoAsync(pedido.Id, StatusPedido.EmPreparacao, funcionarioId);
        Assert.Equal(StatusPedido.EmPreparacao, res1.Status);

        // Act & Assert 2: EmPreparacao -> ProntoParaRetirada
        var res2 = await service.AlterarStatusPedidoAsync(pedido.Id, StatusPedido.ProntoParaRetirada, funcionarioId);
        Assert.Equal(StatusPedido.ProntoParaRetirada, res2.Status);

        // Act & Assert 3: ProntoParaRetirada -> Concluido
        var res3 = await service.AlterarStatusPedidoAsync(pedido.Id, StatusPedido.Concluido, funcionarioId);
        Assert.Equal(StatusPedido.Concluido, res3.Status);
    }

    [Fact]
    public async Task AlterarStatus_SaltoOuRetrocesso_DeveLancarBusinessConflictException()
    {
        // Arrange
        using var context = CriarContextoEmMemoria();
        var funcionarioId = Guid.NewGuid();
        var pedido = new Pedido
        {
            Id = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            Status = StatusPedido.Pendente,
            ValorTotal = 100.00m
        };
        context.Pedidos.Add(pedido);
        await context.SaveChangesAsync();

        var service = new PedidoService(context);

        // Act & Assert: Tentar pular direto de Pendente para Concluido
        var ex = await Assert.ThrowsAsync<BusinessConflictException>(() =>
            service.AlterarStatusPedidoAsync(pedido.Id, StatusPedido.Concluido, funcionarioId));

        Assert.Contains("Transição inválida", ex.Message);
    }

    [Fact]
    public async Task AlterarStatus_QuandoConcluido_NaoPodeMaisSerModificado()
    {
        // Arrange
        using var context = CriarContextoEmMemoria();
        var funcionarioId = Guid.NewGuid();
        var pedido = new Pedido
        {
            Id = Guid.NewGuid(),
            UsuarioId = Guid.NewGuid(),
            Status = StatusPedido.Concluido, // Estado final
            ValorTotal = 80.00m
        };
        context.Pedidos.Add(pedido);
        await context.SaveChangesAsync();

        var service = new PedidoService(context);

        // Act & Assert: Tentar voltar de Concluido para Pendente
        var ex = await Assert.ThrowsAsync<BusinessConflictException>(() =>
            service.AlterarStatusPedidoAsync(pedido.Id, StatusPedido.Pendente, funcionarioId));

        Assert.Contains("estado final", ex.Message);
    }
}
