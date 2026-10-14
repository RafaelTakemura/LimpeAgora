using LimpeAgora.Shared.DTOs.Categorias;
using LimpeAgoraApi.Data;
using LimpeAgoraApi.Entities;
using LimpeAgoraApi.Exceptions;
using LimpeAgoraApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LimpeAgoraApi.Services.Implementations;

public class CategoriaService : ICategoriaService
{
    private readonly LimpeAgoraDbContext _context;

    public CategoriaService(LimpeAgoraDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoriaResponse>> ListarAsync(bool? apenasAtivas = null)
    {
        var query = _context.Categorias.AsNoTracking().AsQueryable();

        if (apenasAtivas.HasValue)
        {
            query = query.Where(c => c.Ativa == apenasAtivas.Value);
        }

        return await query
            .OrderBy(c => c.Nome)
            .Select(c => new CategoriaResponse
            {
                Id = c.Id,
                Nome = c.Nome,
                Descricao = c.Descricao,
                Ativa = c.Ativa,
                CriadoEm = c.CriadoEm
            })
            .ToListAsync();
    }

    public async Task<CategoriaResponse> ObterPorIdAsync(Guid id)
    {
        var categoria = await _context.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria == null)
        {
            throw new NotFoundException($"Categoria com ID '{id}' não foi encontrada.");
        }

        return new CategoriaResponse
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Descricao = categoria.Descricao,
            Ativa = categoria.Ativa,
            CriadoEm = categoria.CriadoEm
        };
    }

    public async Task<CategoriaResponse> CriarAsync(CategoriaUpsertRequest request)
    {
        var nomeFormatado = request.Nome.Trim();

        var existeComMesmoNome = await _context.Categorias
            .AnyAsync(c => c.Nome.ToLower() == nomeFormatado.ToLower());

        if (existeComMesmoNome)
        {
            throw new BusinessConflictException($"Já existe uma categoria cadastrada com o nome '{nomeFormatado}'.");
        }

        var categoria = new Categoria
        {
            Id = Guid.NewGuid(),
            Nome = nomeFormatado,
            Descricao = request.Descricao?.Trim(),
            Ativa = request.Ativa,
            CriadoEm = DateTime.UtcNow
        };

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        return new CategoriaResponse
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Descricao = categoria.Descricao,
            Ativa = categoria.Ativa,
            CriadoEm = categoria.CriadoEm
        };
    }

    public async Task<CategoriaResponse> AtualizarAsync(Guid id, CategoriaUpsertRequest request)
    {
        var categoria = await _context.Categorias.FindAsync(id);

        if (categoria == null)
        {
            throw new NotFoundException($"Categoria com ID '{id}' não foi encontrada.");
        }

        var nomeFormatado = request.Nome.Trim();

        var existeComMesmoNome = await _context.Categorias
            .AnyAsync(c => c.Id != id && c.Nome.ToLower() == nomeFormatado.ToLower());

        if (existeComMesmoNome)
        {
            throw new BusinessConflictException($"Já existe outra categoria com o nome '{nomeFormatado}'.");
        }

        categoria.Nome = nomeFormatado;
        categoria.Descricao = request.Descricao?.Trim();
        categoria.Ativa = request.Ativa;

        await _context.SaveChangesAsync();

        return new CategoriaResponse
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            Descricao = categoria.Descricao,
            Ativa = categoria.Ativa,
            CriadoEm = categoria.CriadoEm
        };
    }

    public async Task InativarAsync(Guid id)
    {
        var categoria = await _context.Categorias.FindAsync(id);

        if (categoria == null)
        {
            throw new NotFoundException($"Categoria com ID '{id}' não foi encontrada.");
        }

        // Exclusão lógica para preservar histórico de produtos e pedidos
        categoria.Ativa = false;
        await _context.SaveChangesAsync();
    }
}
