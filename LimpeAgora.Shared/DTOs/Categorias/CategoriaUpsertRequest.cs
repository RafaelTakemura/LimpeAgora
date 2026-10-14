using System.ComponentModel.DataAnnotations;

namespace LimpeAgora.Shared.DTOs.Categorias;

public class CategoriaUpsertRequest
{
    [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "A descrição não pode exceder 500 caracteres.")]
    public string? Descricao { get; set; }

    public bool Ativa { get; set; } = true;
}
