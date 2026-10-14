using System.ComponentModel.DataAnnotations;
using LimpeAgora.Shared.Enums;

namespace LimpeAgora.Shared.DTOs.Usuarios;

public class AtualizarUsuarioRequest
{
    [Required(ErrorMessage = "O nome completo é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório.")]
    [Phone(ErrorMessage = "Formato de telefone inválido.")]
    [StringLength(20, ErrorMessage = "O telefone não pode exceder 20 caracteres.")]
    public string Telefone { get; set; } = string.Empty;

    [Required(ErrorMessage = "O perfil é obrigatório.")]
    public PerfilUsuario Perfil { get; set; }

    public bool Ativo { get; set; }
}
