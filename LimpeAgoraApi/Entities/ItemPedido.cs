namespace LimpeAgoraApi.Entities;

public class ItemPedido
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PedidoId { get; set; }
    public Guid ProdutoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }

    // Propriedade calculada em memória (não persistida)
    public decimal Subtotal => Quantidade * PrecoUnitario;

    // Relacionamentos
    public Pedido Pedido { get; set; } = null!;
    public Produto Produto { get; set; } = null!;
}
