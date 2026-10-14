using CommunityToolkit.Mvvm.ComponentModel;

namespace LimpeAgoraApp.Models;

public partial class ItemCarrinho : ObservableObject
{
    public Guid ProdutoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public string Apresentacao { get; set; } = string.Empty;
    public decimal PrecoUnitario { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Subtotal))]
    private int _quantidade;

    public int QuantidadeEstoqueMax { get; set; }

    public decimal Subtotal => Quantidade * PrecoUnitario;
}
