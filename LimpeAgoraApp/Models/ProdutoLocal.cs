using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;

namespace LimpeAgoraApp.Models;

[Table("ProdutosCache")]
public partial class ProdutoLocal : ObservableObject
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Indexed]
    public Guid CategoriaId { get; set; }

    public string NomeCategoria { get; set; } = string.Empty;

    [Indexed]
    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public string Apresentacao { get; set; } = string.Empty;

    public decimal Preco { get; set; }

    public int QuantidadeEstoque { get; set; }

    public string? ImagemUrl { get; set; }

    public bool Ativo { get; set; }

    public DateTime AtualizadoEmLocal { get; set; } = DateTime.UtcNow;

    private int _quantidadeNoCarrinho;

    [Ignore]
    public int QuantidadeNoCarrinho
    {
        get => _quantidadeNoCarrinho;
        set => SetProperty(ref _quantidadeNoCarrinho, value);
    }
}
