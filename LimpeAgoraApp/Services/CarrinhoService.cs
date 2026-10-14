using System.Collections.ObjectModel;
using LimpeAgoraApp.Models;

namespace LimpeAgoraApp.Services;

public interface ICarrinhoService
{
    ObservableCollection<ItemCarrinho> Itens { get; }
    decimal ValorTotal { get; }
    int TotalItens { get; }
    event Action? OnCarrinhoAtualizado;

    void AdicionarProduto(Guid produtoId, string nome, string apresentacao, decimal preco, int quantidade, int estoqueMax);
    void SubtrairProduto(Guid produtoId, int quantidade = 1);
    void IncrementarItem(Guid produtoId);
    void DecrementarItem(Guid produtoId);
    void RemoverProduto(Guid produtoId);
    int ObterQuantidadeDoProduto(Guid produtoId);
    void LimparCarrinho();
}

public class CarrinhoService : ICarrinhoService
{
    public ObservableCollection<ItemCarrinho> Itens { get; } = new();

    public decimal ValorTotal => Itens.Sum(i => i.Subtotal);

    public int TotalItens => Itens.Sum(i => i.Quantidade);

    public event Action? OnCarrinhoAtualizado;

    public void AdicionarProduto(Guid produtoId, string nome, string apresentacao, decimal preco, int quantidade, int estoqueMax)
    {
        var itemExistente = Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (itemExistente != null)
        {
            var novaQtd = itemExistente.Quantidade + quantidade;
            if (novaQtd > estoqueMax)
            {
                novaQtd = estoqueMax;
            }
            itemExistente.Quantidade = novaQtd;
        }
        else
        {
            var qtdInicial = Math.Min(quantidade, estoqueMax);
            if (qtdInicial > 0)
            {
                Itens.Add(new ItemCarrinho
                {
                    ProdutoId = produtoId,
                    NomeProduto = nome,
                    Apresentacao = apresentacao,
                    PrecoUnitario = preco,
                    Quantidade = qtdInicial,
                    QuantidadeEstoqueMax = estoqueMax
                });
            }
        }

        OnCarrinhoAtualizado?.Invoke();
    }

    public void SubtrairProduto(Guid produtoId, int quantidade = 1)
    {
        var itemExistente = Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (itemExistente != null)
        {
            if (itemExistente.Quantidade > quantidade)
            {
                itemExistente.Quantidade -= quantidade;
            }
            else
            {
                Itens.Remove(itemExistente);
            }
            OnCarrinhoAtualizado?.Invoke();
        }
    }

    public void IncrementarItem(Guid produtoId)
    {
        var item = Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (item != null)
        {
            if (item.Quantidade < item.QuantidadeEstoqueMax)
            {
                item.Quantidade++;
                OnCarrinhoAtualizado?.Invoke();
            }
        }
    }

    public void DecrementarItem(Guid produtoId)
    {
        var item = Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (item != null)
        {
            if (item.Quantidade > 1)
            {
                item.Quantidade--;
            }
            else
            {
                Itens.Remove(item);
            }
            OnCarrinhoAtualizado?.Invoke();
        }
    }

    public int ObterQuantidadeDoProduto(Guid produtoId)
    {
        return Itens.FirstOrDefault(i => i.ProdutoId == produtoId)?.Quantidade ?? 0;
    }

    public void RemoverProduto(Guid produtoId)
    {
        var item = Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (item != null)
        {
            Itens.Remove(item);
            OnCarrinhoAtualizado?.Invoke();
        }
    }

    public void LimparCarrinho()
    {
        Itens.Clear();
        OnCarrinhoAtualizado?.Invoke();
    }
}
