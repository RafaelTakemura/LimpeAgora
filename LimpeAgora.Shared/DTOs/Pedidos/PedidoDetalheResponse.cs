using LimpeAgora.Shared.Enums;

namespace LimpeAgora.Shared.DTOs.Pedidos;

public class PedidoDetalheResponse
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public string EmailCliente { get; set; } = string.Empty;
    public string TelefoneCliente { get; set; } = string.Empty;
    public DateTime DataPedido { get; set; }
    public StatusPedido Status { get; set; }
    public string StatusDescricao => Status.ToString();
    public decimal ValorTotal { get; set; }
    public string? Observacao { get; set; }
    public DateTime AtualizadoEm { get; set; }
    public List<ItemPedidoResponse> Itens { get; set; } = new();
    public List<HistoricoStatusResponse> Historico { get; set; } = new();
}
