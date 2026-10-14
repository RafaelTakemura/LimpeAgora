using System.ComponentModel.DataAnnotations;

namespace LimpeAgora.Shared.DTOs.Produtos;

public class AtualizarProdutoRequest
{
    [Required(ErrorMessage = "A categoria é obrigatória.")]
    public Guid CategoriaId { get; set; }

    [Required(ErrorMessage = "O nome do produto é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "A descrição não pode exceder 500 caracteres.")]
    public string? Descricao { get; set; }

    [Required(ErrorMessage = "A apresentação é obrigatória (ex: 500 mL, 1 L, 5 kg).")]
    [StringLength(50, ErrorMessage = "A apresentação não pode exceder 50 caracteres.")]
    public string Apresentacao { get; set; } = string.Empty;

    [Range(0.01, 999999.99, ErrorMessage = "O preço deve ser maior que zero.")]
    public decimal Preco { get; set; }

    [Url(ErrorMessage = "A URL da imagem é inválida.")]
    [StringLength(500, ErrorMessage = "A URL da imagem não pode exceder 500 caracteres.")]
    public string? ImagemUrl { get; set; }

    public bool Ativo { get; set; }
}
