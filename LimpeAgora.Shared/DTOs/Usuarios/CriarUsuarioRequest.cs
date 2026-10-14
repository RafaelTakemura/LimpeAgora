using System.ComponentModel.DataAnnotations;
using LimpeAgora.Shared.Enums;

namespace LimpeAgora.Shared.DTOs.Usuarios;

public class CriarUsuarioRequest
{
    [Required(ErrorMessage = "O nome completo é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
    [StringLength(150, ErrorMessage = "O e-mail não pode exceder 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O telefone é obrigatório.")]
    [Phone(ErrorMessage = "Formato de telefone inválido.")]
    [StringLength(20, ErrorMessage = "O telefone não pode exceder 20 caracteres.")]
    public string Telefone { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MinLength(6, ErrorMessage = "A senha deve conter no mínimo 6 caracteres.")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "O perfil do usuário é obrigatório.")]
    public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Cliente;
}
