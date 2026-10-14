using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LimpeAgoraApi.Entities;
using LimpeAgoraApi.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace LimpeAgoraApi.Services.Implementations;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario)
    {
        var chaveSecreta = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(chaveSecreta) || chaveSecreta.StartsWith("SUA_CHAVE_JWT"))
        {
            chaveSecreta = "LimpeAgora_Chave_Desenvolvimento_Local_Testes_Padrao_2026_Minimo32Chars!";
        }
        var issuer = _configuration["Jwt:Issuer"] ?? "LimpeAgoraApi";
        var audience = _configuration["Jwt:Audience"] ?? "LimpeAgoraClients";
        var expirationHours = int.TryParse(_configuration["Jwt:ExpirationHours"], out var hours) ? hours : 8;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveSecreta));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiraEm = DateTime.UtcNow.AddHours(expirationHours);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Name, usuario.NomeCompleto),
            new(ClaimTypes.Role, usuario.Perfil.ToString()),
            new("perfil", usuario.Perfil.ToString()),
            new("ativo", usuario.Ativo.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiraEm,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expiraEm);
    }
}
