using System.ComponentModel.DataAnnotations;

namespace LimpeAgora.Shared.DTOs.Pedidos;

public class ItemPedidoInput
{
    [Required(ErrorMessage = "O identificador do produto é obrigatório.")]
    public Guid ProdutoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser um número inteiro positivo (mínimo 1).")]
    public int Quantidade { get; set; }
}
