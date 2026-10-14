namespace LimpeAgoraApi.Entities;

public class Produto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CategoriaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string Apresentacao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int QuantidadeEstoque { get; set; }
    public string? ImagemUrl { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

    // Relacionamentos
    public Categoria Categoria { get; set; } = null!;
    public ICollection<ItemPedido> ItensPedido { get; set; } = new List<ItemPedido>();
}
