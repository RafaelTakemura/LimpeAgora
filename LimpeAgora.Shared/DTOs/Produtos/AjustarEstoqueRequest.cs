using System.ComponentModel.DataAnnotations;

namespace LimpeAgora.Shared.DTOs.Produtos;

public class AjustarEstoqueRequest
{
    [Range(0, int.MaxValue, ErrorMessage = "A nova quantidade de estoque não pode ser negativa.")]
    public int NovaQuantidade { get; set; }

    [StringLength(250, ErrorMessage = "A justificativa não pode exceder 250 caracteres.")]
    public string? Justificativa { get; set; }
}
