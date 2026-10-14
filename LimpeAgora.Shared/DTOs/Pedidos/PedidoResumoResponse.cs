using LimpeAgora.Shared.Enums;

namespace LimpeAgora.Shared.DTOs.Pedidos;

public class PedidoResumoResponse
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public DateTime DataPedido { get; set; }
    public StatusPedido Status { get; set; }
    public string StatusDescricao => Status.ToString();
    public decimal ValorTotal { get; set; }
    public int TotalItens { get; set; }
    public string? Observacao { get; set; }
}
