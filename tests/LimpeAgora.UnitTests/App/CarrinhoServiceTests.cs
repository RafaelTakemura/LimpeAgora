using LimpeAgoraApp.Models;
using LimpeAgoraApp.Services;
using Xunit;

namespace LimpeAgora.UnitTests.App;

public class CarrinhoServiceTests
{
    [Fact]
    public void AdicionarProduto_NovoItem_DeveAdicionarComQuantidadeESubtotalCorretos()
    {
        // Arrange
        var service = new CarrinhoService();
        var produtoId = Guid.NewGuid();

        // Act
        service.AdicionarProduto(produtoId, "Detergente Neutro", "500 ml", 5.50m, 2, estoqueMax: 10);

        // Assert
        Assert.Single(service.Itens);
        var item = service.Itens.First();
        Assert.Equal(produtoId, item.ProdutoId);
        Assert.Equal(2, item.Quantidade);
        Assert.Equal(5.50m, item.PrecoUnitario);
        Assert.Equal(11.00m, item.Subtotal);
        Assert.Equal(11.00m, service.ValorTotal);
        Assert.Equal(2, service.TotalItens);
    }

    [Fact]
    public void AdicionarProduto_MesmoItem_DeveIncrementarQuantidadeAteEstoqueMaximo()
    {
        // Arrange
        var service = new CarrinhoService();
        var produtoId = Guid.NewGuid();

        // Act
        service.AdicionarProduto(produtoId, "Sabão Líquido", "3 L", 25.00m, 3, estoqueMax: 5);
        service.AdicionarProduto(produtoId, "Sabão Líquido", "3 L", 25.00m, 4, estoqueMax: 5); // Tenta somar +4 (total seria 7, mas máx é 5)

        // Assert
        Assert.Single(service.Itens);
        var item = service.Itens.First();
        Assert.Equal(5, item.Quantidade); // Limitado ao estoqueMax
        Assert.Equal(125.00m, service.ValorTotal);
    }

    [Fact]
    public void IncrementarItem_AbaixoDoEstoque_DeveAumentarQuantidadeEmUmaUnidade()
    {
        // Arrange
        var service = new CarrinhoService();
        var produtoId = Guid.NewGuid();
        service.AdicionarProduto(produtoId, "Água Sanitária", "2 L", 8.00m, 1, estoqueMax: 5);

        // Act
        service.IncrementarItem(produtoId);

        // Assert
        var item = service.Itens.First();
        Assert.Equal(2, item.Quantidade);
        Assert.Equal(16.00m, service.ValorTotal);
        Assert.Equal(2, service.TotalItens);
    }

    [Fact]
    public void IncrementarItem_NoEstoqueMaximo_NaoDeveUltrapassarEstoque()
    {
        // Arrange
        var service = new CarrinhoService();
        var produtoId = Guid.NewGuid();
        service.AdicionarProduto(produtoId, "Desengordurante", "500 ml", 12.00m, 3, estoqueMax: 3);

        // Act
        service.IncrementarItem(produtoId); // Já está no máximo (3)

        // Assert
        var item = service.Itens.First();
        Assert.Equal(3, item.Quantidade);
    }

    [Fact]
    public void DecrementarItem_MaiorQueUm_DeveDiminuirQuantidadeEmUmaUnidade()
    {
        // Arrange
        var service = new CarrinhoService();
        var produtoId = Guid.NewGuid();
        service.AdicionarProduto(produtoId, "Álcool 70%", "1 L", 7.50m, 3, estoqueMax: 10);

        // Act
        service.DecrementarItem(produtoId);

        // Assert
        var item = service.Itens.First();
        Assert.Equal(2, item.Quantidade);
        Assert.Equal(15.00m, service.ValorTotal);
    }

    [Fact]
    public void DecrementarItem_QuandoIgualAUm_DeveRemoverDoCarrinho()
    {
        // Arrange
        var service = new CarrinhoService();
        var produtoId = Guid.NewGuid();
        service.AdicionarProduto(produtoId, "Lustra Móveis", "200 ml", 9.00m, 1, estoqueMax: 10);

        // Act
        service.DecrementarItem(produtoId);

        // Assert
        Assert.Empty(service.Itens);
        Assert.Equal(0, service.TotalItens);
        Assert.Equal(0m, service.ValorTotal);
    }

    [Fact]
    public void SubtrairProduto_QuantidadeParcial_DeveAtualizarQuantidade()
    {
        // Arrange
        var service = new CarrinhoService();
        var produtoId = Guid.NewGuid();
        service.AdicionarProduto(produtoId, "Esponja Multiuso", "Pacote 3un", 6.00m, 4, estoqueMax: 10);

        // Act
        service.SubtrairProduto(produtoId, quantidade: 2);

        // Assert
        var item = service.Itens.First();
        Assert.Equal(2, item.Quantidade);
        Assert.Equal(12.00m, service.ValorTotal);
    }

    [Fact]
    public void RemoverProduto_ItemExistente_DeveRemoverTotalmente()
    {
        // Arrange
        var service = new CarrinhoService();
        var prod1 = Guid.NewGuid();
        var prod2 = Guid.NewGuid();
        service.AdicionarProduto(prod1, "Item 1", "1 L", 10.00m, 2, 5);
        service.AdicionarProduto(prod2, "Item 2", "2 L", 20.00m, 1, 5);

        // Act
        service.RemoverProduto(prod1);

        // Assert
        Assert.Single(service.Itens);
        Assert.Equal(prod2, service.Itens.First().ProdutoId);
        Assert.Equal(20.00m, service.ValorTotal);
    }

    [Fact]
    public void LimparCarrinho_DeveEsvaziarColecaoERedefinirTotais()
    {
        // Arrange
        var service = new CarrinhoService();
        service.AdicionarProduto(Guid.NewGuid(), "Item A", "1 L", 10.00m, 2, 5);
        service.AdicionarProduto(Guid.NewGuid(), "Item B", "2 L", 15.00m, 1, 5);
        Assert.Equal(2, service.Itens.Count);

        // Act
        service.LimparCarrinho();

        // Assert
        Assert.Empty(service.Itens);
        Assert.Equal(0, service.TotalItens);
        Assert.Equal(0m, service.ValorTotal);
    }

    [Fact]
    public void ItemCarrinho_AlteracaoQuantidade_DeveRecalcularSubtotalDinamicamente()
    {
        // Arrange
        var item = new ItemCarrinho
        {
            ProdutoId = Guid.NewGuid(),
            NomeProduto = "Pinho Sol",
            Apresentacao = "500 ml",
            PrecoUnitario = 10.50m,
            Quantidade = 2,
            QuantidadeEstoqueMax = 10
        };

        Assert.Equal(21.00m, item.Subtotal);

        // Act: Modificar quantidade observável
        item.Quantidade = 5;

        // Assert
        Assert.Equal(52.50m, item.Subtotal);
    }
}
