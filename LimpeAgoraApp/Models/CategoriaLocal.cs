using SQLite;

namespace LimpeAgoraApp.Models;

[Table("CategoriasCache")]
public class CategoriaLocal
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Indexed]
    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public bool Ativa { get; set; }

    public DateTime CriadoEm { get; set; }

    public DateTime AtualizadoEmLocal { get; set; } = DateTime.UtcNow;
}
