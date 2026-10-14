using LimpeAgora.Shared.Enums;

namespace LimpeAgoraApi.Entities;

public class Pedido
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public DateTime DataPedido { get; set; } = DateTime.UtcNow;
    public StatusPedido Status { get; set; } = StatusPedido.Pendente;
    public decimal ValorTotal { get; set; }
    public string? Observacao { get; set; }
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? ExcluidoEm { get; set; }

    // Relacionamentos
    public Usuario Usuario { get; set; } = null!;
    public ICollection<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
    public ICollection<HistoricoStatusPedido> HistoricosStatus { get; set; } = new List<HistoricoStatusPedido>();
}
