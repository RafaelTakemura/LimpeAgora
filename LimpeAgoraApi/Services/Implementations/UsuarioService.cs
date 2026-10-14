using LimpeAgora.Shared.DTOs.Usuarios;
using LimpeAgoraApi.Data;
using LimpeAgoraApi.Entities;
using LimpeAgoraApi.Exceptions;
using LimpeAgoraApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LimpeAgoraApi.Services.Implementations;

public class UsuarioService : IUsuarioService
{
    private readonly LimpeAgoraDbContext _context;
    private readonly IPasswordService _passwordService;

    public UsuarioService(LimpeAgoraDbContext context, IPasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    public async Task<List<UsuarioResponse>> ListarAsync()
    {
        return await _context.Usuarios
            .AsNoTracking()
            .OrderBy(u => u.NomeCompleto)
            .Select(u => MapearParaResponse(u))
            .ToListAsync();
    }

    public async Task<UsuarioResponse> ObterPorIdAsync(Guid id)
    {
        var usuario = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (usuario == null)
        {
            throw new NotFoundException($"Usuário com ID '{id}' não foi encontrado.");
        }

        return MapearParaResponse(usuario);
    }

    public async Task<UsuarioResponse> CriarAsync(CriarUsuarioRequest request)
    {
        var emailNormalizado = request.Email.Trim().ToLowerInvariant();

        var existeEmail = await _context.Usuarios
            .AnyAsync(u => u.Email.ToLower() == emailNormalizado);

        if (existeEmail)
        {
            throw new BusinessConflictException("Já existe um usuário com este e-mail.");
        }

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            NomeCompleto = request.NomeCompleto.Trim(),
            Email = emailNormalizado,
            Telefone = request.Telefone.Trim(),
            SenhaHash = _passwordService.HashPassword(request.Senha),
            Perfil = request.Perfil,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        return MapearParaResponse(usuario);
    }

    public async Task<UsuarioResponse> AtualizarAsync(Guid id, AtualizarUsuarioRequest request)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
        {
            throw new NotFoundException($"Usuário com ID '{id}' não foi encontrado.");
        }

        usuario.NomeCompleto = request.NomeCompleto.Trim();
        usuario.Telefone = request.Telefone.Trim();
        usuario.Perfil = request.Perfil;
        usuario.Ativo = request.Ativo;

        await _context.SaveChangesAsync();

        return MapearParaResponse(usuario);
    }

    public async Task InativarAsync(Guid id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
        {
            throw new NotFoundException($"Usuário com ID '{id}' não foi encontrado.");
        }

        usuario.Ativo = false;
        await _context.SaveChangesAsync();
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
