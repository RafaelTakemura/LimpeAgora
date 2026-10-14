using LimpeAgora.Shared.DTOs.Produtos;
using LimpeAgoraApi.Data;
using LimpeAgoraApi.Entities;
using LimpeAgoraApi.Exceptions;
using LimpeAgoraApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LimpeAgoraApi.Services.Implementations;

public class ProdutoService : IProdutoService
{
    private readonly LimpeAgoraDbContext _context;

    public ProdutoService(LimpeAgoraDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProdutoResponse>> ListarAsync(Guid? categoriaId = null, bool? apenasAtivos = null)
    {
        var query = _context.Produtos
            .Include(p => p.Categoria)
            .AsNoTracking()
            .AsQueryable();

        if (categoriaId.HasValue && categoriaId.Value != Guid.Empty)
        {
            query = query.Where(p => p.CategoriaId == categoriaId.Value);
        }

        if (apenasAtivos.HasValue)
        {
            query = query.Where(p => p.Ativo == apenasAtivos.Value);
        }

        return await query
            .OrderBy(p => p.Nome)
            .Select(p => MapearParaResponse(p))
            .ToListAsync();
    }

    public async Task<ProdutoResponse> ObterPorIdAsync(Guid id)
    {
        var produto = await _context.Produtos
            .Include(p => p.Categoria)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (produto == null)
        {
            throw new NotFoundException($"Produto com ID '{id}' não foi encontrado.");
        }

        return MapearParaResponse(produto);
    }

    public async Task<ProdutoResponse> CriarAsync(CriarProdutoRequest request)
    {
        var categoria = await _context.Categorias.FindAsync(request.CategoriaId);
        if (categoria == null)
        {
            throw new NotFoundException($"A categoria informada não existe.");
        }

        if (!categoria.Ativa)
        {
            throw new BusinessConflictException("Não é permitido associar um produto a uma categoria inativa.");
        }

        if (request.Preco <= 0)
        {
            throw new DomainException("O preço do produto deve ser maior que zero.");
        }

        if (request.QuantidadeEstoque < 0)
        {
            throw new DomainException("A quantidade inicial em estoque não pode ser negativa.");
        }

        var produto = new Produto
        {
            Id = Guid.NewGuid(),
            CategoriaId = request.CategoriaId,
            Nome = request.Nome.Trim(),
            Descricao = request.Descricao?.Trim(),
            Apresentacao = request.Apresentacao.Trim(),
            Preco = request.Preco,
            QuantidadeEstoque = request.QuantidadeEstoque,
            ImagemUrl = request.ImagemUrl?.Trim(),
            Ativo = request.Ativo,
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow
        };

        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();

        produto.Categoria = categoria;
        return MapearParaResponse(produto);
    }

    public async Task<ProdutoResponse> AtualizarAsync(Guid id, AtualizarProdutoRequest request)
    {
        var produto = await _context.Produtos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (produto == null)
        {
            throw new NotFoundException($"Produto com ID '{id}' não foi encontrado.");
        }

        var categoria = await _context.Categorias.FindAsync(request.CategoriaId);
        if (categoria == null)
        {
            throw new NotFoundException($"A categoria informada não existe.");
        }

        if (request.Preco <= 0)
        {
            throw new DomainException("O preço do produto deve ser maior que zero.");
        }

        produto.CategoriaId = request.CategoriaId;
        produto.Nome = request.Nome.Trim();
        produto.Descricao = request.Descricao?.Trim();
        produto.Apresentacao = request.Apresentacao.Trim();
        produto.Preco = request.Preco;
        produto.ImagemUrl = request.ImagemUrl?.Trim();
        produto.Ativo = request.Ativo;
        produto.AtualizadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        produto.Categoria = categoria;
        return MapearParaResponse(produto);
    }

    public async Task InativarAsync(Guid id)
    {
        var produto = await _context.Produtos.FindAsync(id);
        if (produto == null)
        {
            throw new NotFoundException($"Produto com ID '{id}' não foi encontrado.");
        }

        produto.Ativo = false;
        produto.AtualizadoEm = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<ProdutoResponse> AjustarEstoqueAsync(Guid id, AjustarEstoqueRequest request)
    {
        if (request.NovaQuantidade < 0)
        {
            throw new DomainException("A quantidade em estoque não pode ser negativa.");
        }

        var produto = await _context.Produtos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (produto == null)
        {
            throw new NotFoundException($"Produto com ID '{id}' não foi encontrado.");
        }

        produto.QuantidadeEstoque = request.NovaQuantidade;
        produto.AtualizadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapearParaResponse(produto);
    }

    private static ProdutoResponse MapearParaResponse(Produto produto)
    {
        return new ProdutoResponse
        {
            Id = produto.Id,
            CategoriaId = produto.CategoriaId,
            NomeCategoria = produto.Categoria?.Nome ?? string.Empty,
            Nome = produto.Nome,
            Descricao = produto.Descricao,
            Apresentacao = produto.Apresentacao,
            Preco = produto.Preco,
            QuantidadeEstoque = produto.QuantidadeEstoque,
            ImagemUrl = produto.ImagemUrl,
            Ativo = produto.Ativo,
            CriadoEm = produto.CriadoEm,
            AtualizadoEm = produto.AtualizadoEm
        };
    }
}
