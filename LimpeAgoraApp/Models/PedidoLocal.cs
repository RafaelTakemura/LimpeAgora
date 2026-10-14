using LimpeAgora.Shared.Enums;
using SQLite;

namespace LimpeAgoraApp.Models;

[Table("PedidosCache")]
public class PedidoLocal
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Indexed]
    public Guid UsuarioId { get; set; }

    public string NomeCliente { get; set; } = string.Empty;

    public DateTime DataPedido { get; set; }

    public StatusPedido Status { get; set; }

    public string StatusDescricao { get; set; } = string.Empty;

    public decimal ValorTotal { get; set; }

    public int TotalItens { get; set; }

    public string? Observacao { get; set; }

    public DateTime SincronizadoEm { get; set; } = DateTime.UtcNow;
}
