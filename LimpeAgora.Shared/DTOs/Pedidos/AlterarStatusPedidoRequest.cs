using System.ComponentModel.DataAnnotations;
using LimpeAgora.Shared.Enums;

namespace LimpeAgora.Shared.DTOs.Pedidos;

public class AlterarStatusPedidoRequest
{
    [Required(ErrorMessage = "O novo status é obrigatório.")]
    public StatusPedido NovoStatus { get; set; }
}
