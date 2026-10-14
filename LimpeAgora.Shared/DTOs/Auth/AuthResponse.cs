using LimpeAgora.Shared.DTOs.Usuarios;

namespace LimpeAgora.Shared.DTOs.Auth;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
    public UsuarioResponse Usuario { get; set; } = null!;
}
