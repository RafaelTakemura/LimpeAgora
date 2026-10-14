namespace LimpeAgora.Shared.DTOs.Produtos;

public class ProdutoResponse
{
    public Guid Id { get; set; }
    public Guid CategoriaId { get; set; }
    public string NomeCategoria { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string Apresentacao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int QuantidadeEstoque { get; set; }
    public string? ImagemUrl { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }
}
