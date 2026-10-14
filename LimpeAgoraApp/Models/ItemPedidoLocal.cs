using SQLite;

namespace LimpeAgoraApp.Models;

[Table("ItensPedidoCache")]
public class ItemPedidoLocal
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Indexed]
    public Guid PedidoId { get; set; }

    public Guid ProdutoId { get; set; }

    public string NomeProduto { get; set; } = string.Empty;

    public int Quantidade { get; set; }

    public decimal PrecoUnitario { get; set; }

    public decimal Subtotal => Quantidade * PrecoUnitario;
}
