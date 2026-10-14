using LimpeAgora.Shared.Enums;

namespace LimpeAgoraApi.Entities;

public class HistoricoStatusPedido
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PedidoId { get; set; }
    public StatusPedido StatusAnterior { get; set; }
    public StatusPedido StatusNovo { get; set; }
    public Guid UsuarioResponsavelId { get; set; }
    public DateTime AlteradoEm { get; set; } = DateTime.UtcNow;

    // Relacionamentos
    public Pedido Pedido { get; set; } = null!;
    public Usuario UsuarioResponsavel { get; set; } = null!;
}
