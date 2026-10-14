using System.ComponentModel.DataAnnotations;

namespace LimpeAgora.Shared.DTOs.Pedidos;

public class CriarPedidoRequest
{
    [Required(ErrorMessage = "O pedido deve conter pelo menos um item.")]
    [MinLength(1, ErrorMessage = "O pedido deve conter pelo menos um item.")]
    public List<ItemPedidoInput> Itens { get; set; } = new();

    [StringLength(500, ErrorMessage = "A observação não pode exceder 500 caracteres.")]
    public string? Observacao { get; set; }
}
