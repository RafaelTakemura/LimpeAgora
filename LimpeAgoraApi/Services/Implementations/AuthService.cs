using LimpeAgora.Shared.DTOs.Auth;
using LimpeAgora.Shared.DTOs.Usuarios;
using LimpeAgora.Shared.Enums;
using LimpeAgoraApi.Data;
using LimpeAgoraApi.Entities;
using LimpeAgoraApi.Exceptions;
using LimpeAgoraApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LimpeAgoraApi.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly LimpeAgoraDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public AuthService(
        LimpeAgoraDbContext _context,
        IPasswordService passwordService,
        ITokenService tokenService)
    {
        this._context = _context;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> CadastrarClienteAsync(CadastrarClienteRequest request)
    {
        var emailNormalizado = request.Email.Trim().ToLowerInvariant();

        var emailJaExiste = await _context.Usuarios
            .AnyAsync(u => u.Email.ToLower() == emailNormalizado);

        if (emailJaExiste)
        {
            throw new BusinessConflictException("Já existe um usuário cadastrado com este e-mail.");
        }

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            NomeCompleto = request.NomeCompleto.Trim(),
            Email = emailNormalizado,
            Telefone = request.Telefone.Trim(),
            SenhaHash = _passwordService.HashPassword(request.Senha),
            Perfil = PerfilUsuario.Cliente, // Obrigatoriamente Cliente no autocadastro público
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        var (token, expiraEm) = _tokenService.GerarToken(usuario);

        return new AuthResponse
        {
            Token = token,
            ExpiraEm = expiraEm,
            Usuario = MapearParaResponse(usuario)
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var emailNormalizado = request.Email.Trim().ToLowerInvariant();

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email.ToLower() == emailNormalizado);

        if (usuario == null)
        {
            throw new DomainException("Credenciais inválidas.", 401);
        }

        if (!usuario.Ativo)
        {
            throw new DomainException("Usuário inativo. Entre em contato com o suporte ou a administração da loja.", 403);
        }

        var senhaValida = _passwordService.VerifyPassword(request.Senha, usuario.SenhaHash);
        if (!senhaValida)
        {
            throw new DomainException("Credenciais inválidas.", 401);
        }

        var (token, expiraEm) = _tokenService.GerarToken(usuario);

        return new AuthResponse
        {
            Token = token,
            ExpiraEm = expiraEm,
            Usuario = MapearParaResponse(usuario)
        };
    }

    public async Task<UsuarioResponse> ObterUsuarioLogadoAsync(Guid usuarioId)
    {
        var usuario = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == usuarioId);

        if (usuario == null)
        {
            throw new NotFoundException("Usuário não encontrado.");
        }

        return MapearParaResponse(usuario);
    }

    private static UsuarioResponse MapearParaResponse(Usuario usuario)
    {
        return new UsuarioResponse
        {
            Id = usuario.Id,
            NomeCompleto = usuario.NomeCompleto,
            Email = usuario.Email,
            Telefone = usuario.Telefone,
            Perfil = usuario.Perfil,
            Ativo = usuario.Ativo,
            CriadoEm = usuario.CriadoEm
        };
    }
}
