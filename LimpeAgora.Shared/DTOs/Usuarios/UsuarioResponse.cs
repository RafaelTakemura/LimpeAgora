using LimpeAgora.Shared.Enums;

namespace LimpeAgora.Shared.DTOs.Usuarios;

public class UsuarioResponse
{
    public Guid Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public PerfilUsuario Perfil { get; set; }
    public string PerfilDescricao => Perfil.ToString();
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}
