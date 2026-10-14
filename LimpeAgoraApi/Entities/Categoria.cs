namespace LimpeAgoraApi.Entities;

public class Categoria
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativa { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    // Relacionamento com produtos
    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
}
