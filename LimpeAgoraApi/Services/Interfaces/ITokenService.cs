using LimpeAgoraApi.Entities;

namespace LimpeAgoraApi.Services.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario);
}
