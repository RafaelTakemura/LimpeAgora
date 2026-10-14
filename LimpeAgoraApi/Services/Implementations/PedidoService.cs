using LimpeAgora.Shared.DTOs.Pedidos;
using LimpeAgora.Shared.Enums;
using LimpeAgoraApi.Data;
using LimpeAgoraApi.Entities;
using LimpeAgoraApi.Exceptions;
using LimpeAgoraApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LimpeAgoraApi.Services.Implementations;

public class PedidoService : IPedidoService
{
    private readonly LimpeAgoraDbContext _context;

    public PedidoService(LimpeAgoraDbContext context)
    {
        _context = context;
    }

    public async Task<PedidoDetalheResponse> CriarPedidoAsync(Guid usuarioId, CriarPedidoRequest request)
    {
        if (request.Itens == null || !request.Itens.Any())
        {
            throw new DomainException("O pedido deve conter pelo menos um item.");
        }

        // Valida se todas as quantidades são inteiros positivos
        if (request.Itens.Any(i => i.Quantidade <= 0))
        {
            throw new DomainException("Todas as quantidades solicitadas devem ser números inteiros positivos (mínimo 1).");
        }

        // Verifica existência e status do cliente
        var cliente = await _context.Usuarios.FindAsync(usuarioId);
        if (cliente == null)
        {
            throw new NotFoundException("Usuário cliente não encontrado.");
        }

        if (!cliente.Ativo)
        {
            throw new DomainException("Usuários inativos não podem realizar pedidos.", 403);
        }

        // Agrupa itens repetidos pelo ProdutoId somando as quantidades
        var itensAgrupados = request.Itens
            .GroupBy(i => i.ProdutoId)
            .Select(g => new { ProdutoId = g.Key, Quantidade = g.Sum(x => x.Quantidade) })
            .ToList();

        var produtoIds = itensAgrupados.Select(i => i.ProdutoId).ToList();

        // Inicia transação atômica para garantia de concorrência e integridade do estoque
        using var transaction = await _context.Database.BeginTransactionAsync();

        var produtos = await _context.Produtos
            .Where(p => produtoIds.Contains(p.Id))
            .ToListAsync();

        if (produtos.Count != produtoIds.Count)
        {
            throw new NotFoundException("Um ou mais produtos informados no pedido não foram encontrados.");
        }

        var pedido = new Pedido
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            DataPedido = DateTime.UtcNow,
            Status = StatusPedido.Pendente,
            Observacao = request.Observacao?.Trim(),
            AtualizadoEm = DateTime.UtcNow
        };

        decimal valorTotal = 0;

        foreach (var itemAgrupado in itensAgrupados)
        {
            var produto = produtos.First(p => p.Id == itemAgrupado.ProdutoId);

            if (!produto.Ativo)
            {
                throw new BusinessConflictException($"O produto '{produto.Nome}' está inativo e não pode ser comprado.");
            }

            if (produto.QuantidadeEstoque < itemAgrupado.Quantidade)
            {
                throw new BusinessConflictException($"Estoque insuficiente para o produto '{produto.Nome}'. Disponível: {produto.QuantidadeEstoque}, Solicitado: {itemAgrupado.Quantidade}.");
            }

            // Desconta a quantidade do estoque atomicamente
            produto.QuantidadeEstoque -= itemAgrupado.Quantidade;
            produto.AtualizadoEm = DateTime.UtcNow;

            var precoUnitario = produto.Preco;
            valorTotal += precoUnitario * itemAgrupado.Quantidade;

            // Congela dados históricos do item no momento da compra
            pedido.Itens.Add(new ItemPedido
            {
                Id = Guid.NewGuid(),
                PedidoId = pedido.Id,
                ProdutoId = produto.Id,
                NomeProduto = produto.Nome,
                Quantidade = itemAgrupado.Quantidade,
                PrecoUnitario = precoUnitario
            });
        }

        pedido.ValorTotal = valorTotal;

        // Registra o primeiro status no histórico
        pedido.HistoricosStatus.Add(new HistoricoStatusPedido
        {
            Id = Guid.NewGuid(),
            PedidoId = pedido.Id,
            StatusAnterior = StatusPedido.Pendente,
            StatusNovo = StatusPedido.Pendente,
            UsuarioResponsavelId = usuarioId,
            AlteradoEm = DateTime.UtcNow
        });

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return await ObterDetalhesPorIdAsync(pedido.Id, usuarioId, isFuncionarioOuAdmin: true);
    }

    public async Task<List<PedidoResumoResponse>> ListarMeusPedidosAsync(Guid usuarioId)
    {
        return await _context.Pedidos
            .AsNoTracking()
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.DataPedido)
            .Select(p => new PedidoResumoResponse
            {
                Id = p.Id,
                UsuarioId = p.UsuarioId,
                NomeCliente = p.Usuario.NomeCompleto,
                DataPedido = p.DataPedido,
                Status = p.Status,
                ValorTotal = p.ValorTotal,
                TotalItens = p.Itens.Sum(i => i.Quantidade),
                Observacao = p.Observacao
            })
            .ToListAsync();
    }

    public async Task<List<PedidoResumoResponse>> ListarTodosPedidosAsync(
        StatusPedido? status = null,
        Guid? usuarioId = null,
        DateTime? dataInicio = null,
        DateTime? dataFim = null)
    {
        var query = _context.Pedidos
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (usuarioId.HasValue && usuarioId.Value != Guid.Empty)
        {
            query = query.Where(p => p.UsuarioId == usuarioId.Value);
        }

        if (dataInicio.HasValue)
        {
            query = query.Where(p => p.DataPedido >= dataInicio.Value);
        }

        if (dataFim.HasValue)
        {
            query = query.Where(p => p.DataPedido <= dataFim.Value);
        }

        return await query
            .OrderByDescending(p => p.DataPedido)
            .Select(p => new PedidoResumoResponse
            {
                Id = p.Id,
                UsuarioId = p.UsuarioId,
                NomeCliente = p.Usuario.NomeCompleto,
                DataPedido = p.DataPedido,
                Status = p.Status,
                ValorTotal = p.ValorTotal,
                TotalItens = p.Itens.Sum(i => i.Quantidade),
                Observacao = p.Observacao
            })
            .ToListAsync();
    }

    public async Task<PedidoDetalheResponse> ObterDetalhesPorIdAsync(
        Guid pedidoId,
        Guid usuarioRequisitanteId,
        bool isFuncionarioOuAdmin)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Usuario)
            .Include(p => p.Itens)
            .Include(p => p.HistoricosStatus)
                .ThenInclude(h => h.UsuarioResponsavel)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == pedidoId);

        if (pedido == null)
        {
            throw new NotFoundException($"Pedido com ID '{pedidoId}' não foi encontrado.");
        }

        // Cliente só pode acessar seu próprio pedido
        if (!isFuncionarioOuAdmin && pedido.UsuarioId != usuarioRequisitanteId)
        {
            throw new DomainException("Você não possui permissão para visualizar este pedido.", 403);
        }

        return new PedidoDetalheResponse
        {
            Id = pedido.Id,
            UsuarioId = pedido.UsuarioId,
            NomeCliente = pedido.Usuario.NomeCompleto,
            EmailCliente = pedido.Usuario.Email,
            TelefoneCliente = pedido.Usuario.Telefone,
            DataPedido = pedido.DataPedido,
            Status = pedido.Status,
            ValorTotal = pedido.ValorTotal,
            Observacao = pedido.Observacao,
            AtualizadoEm = pedido.AtualizadoEm,
            Itens = pedido.Itens.Select(i => new ItemPedidoResponse
            {
                Id = i.Id,
                ProdutoId = i.ProdutoId,
                NomeProduto = i.NomeProduto,
                Quantidade = i.Quantidade,
                PrecoUnitario = i.PrecoUnitario
            }).ToList(),
            Historico = pedido.HistoricosStatus
                .OrderBy(h => h.AlteradoEm)
                .Select(h => new HistoricoStatusResponse
                {
                    Id = h.Id,
                    StatusAnterior = h.StatusAnterior,
                    StatusNovo = h.StatusNovo,
                    UsuarioResponsavelId = h.UsuarioResponsavelId,
                    NomeUsuarioResponsavel = h.UsuarioResponsavel.NomeCompleto,
                    AlteradoEm = h.AlteradoEm
                }).ToList()
        };
    }

    public async Task CancelarPedidoAsync(
        Guid pedidoId,
        Guid usuarioRequisitanteId,
        bool isFuncionarioOuAdmin)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == pedidoId);

        if (pedido == null)
        {
            throw new NotFoundException($"Pedido com ID '{pedidoId}' não foi encontrado.");
        }

        // Validação de titularidade
        if (!isFuncionarioOuAdmin && pedido.UsuarioId != usuarioRequisitanteId)
        {
            throw new DomainException("Você só pode cancelar seus próprios pedidos.", 403);
        }

        // Regra estrita: apenas pedidos com status Pendente podem ser cancelados
        if (pedido.Status != StatusPedido.Pendente)
        {
            throw new BusinessConflictException($"Pedidos com status '{pedido.Status}' não podem ser cancelados. Apenas pedidos Pendentes admitem cancelamento.");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        // Devolução das quantidades de cada item ao estoque
        var produtoIds = pedido.Itens.Select(i => i.ProdutoId).Distinct().ToList();
        var produtos = await _context.Produtos
            .Where(p => produtoIds.Contains(p.Id))
            .ToListAsync();

        foreach (var item in pedido.Itens)
        {
            var produto = produtos.FirstOrDefault(p => p.Id == item.ProdutoId);
            if (produto != null)
            {
                produto.QuantidadeEstoque += item.Quantidade;
                produto.AtualizadoEm = DateTime.UtcNow;
            }
        }

        var statusAnterior = pedido.Status;
        pedido.Status = StatusPedido.Cancelado;
        pedido.AtualizadoEm = DateTime.UtcNow;

        _context.HistoricosStatusPedido.Add(new HistoricoStatusPedido
        {
            Id = Guid.NewGuid(),
            PedidoId = pedido.Id,
            StatusAnterior = statusAnterior,
            StatusNovo = StatusPedido.Cancelado,
            UsuarioResponsavelId = usuarioRequisitanteId,
            AlteradoEm = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<PedidoDetalheResponse> AlterarStatusPedidoAsync(
        Guid pedidoId,
        StatusPedido novoStatus,
        Guid usuarioResponsavelId)
    {
        var pedido = await _context.Pedidos.FindAsync(pedidoId);
        if (pedido == null)
        {
            throw new NotFoundException($"Pedido com ID '{pedidoId}' não foi encontrado.");
        }

        // Validação da máquina de estados do pedido
        ValidarTransicaoStatus(pedido.Status, novoStatus);

        var statusAnterior = pedido.Status;
        pedido.Status = novoStatus;
        pedido.AtualizadoEm = DateTime.UtcNow;

        _context.HistoricosStatusPedido.Add(new HistoricoStatusPedido
        {
            Id = Guid.NewGuid(),
            PedidoId = pedido.Id,
            StatusAnterior = statusAnterior,
            StatusNovo = novoStatus,
            UsuarioResponsavelId = usuarioResponsavelId,
            AlteradoEm = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return await ObterDetalhesPorIdAsync(pedidoId, usuarioResponsavelId, isFuncionarioOuAdmin: true);
    }

    public async Task<List<HistoricoStatusResponse>> ObterHistoricoAsync(
        Guid pedidoId,
        Guid usuarioRequisitanteId,
        bool isFuncionarioOuAdmin)
    {
        var pedido = await _context.Pedidos.FindAsync(pedidoId);
        if (pedido == null)
        {
            throw new NotFoundException($"Pedido com ID '{pedidoId}' não foi encontrado.");
        }

        if (!isFuncionarioOuAdmin && pedido.UsuarioId != usuarioRequisitanteId)
        {
            throw new DomainException("Você não tem permissão para visualizar o histórico deste pedido.", 403);
        }

        return await _context.HistoricosStatusPedido
            .Include(h => h.UsuarioResponsavel)
            .AsNoTracking()
            .Where(h => h.PedidoId == pedidoId)
            .OrderBy(h => h.AlteradoEm)
            .Select(h => new HistoricoStatusResponse
            {
                Id = h.Id,
                StatusAnterior = h.StatusAnterior,
                StatusNovo = h.StatusNovo,
                UsuarioResponsavelId = h.UsuarioResponsavelId,
                NomeUsuarioResponsavel = h.UsuarioResponsavel.NomeCompleto,
                AlteradoEm = h.AlteradoEm
            })
            .ToListAsync();
    }

    private static void ValidarTransicaoStatus(StatusPedido statusAtual, StatusPedido novoStatus)
    {
        if (statusAtual == StatusPedido.Concluido || statusAtual == StatusPedido.Cancelado)
        {
            throw new BusinessConflictException($"O pedido já se encontra em estado final ({statusAtual}) e não pode ser alterado.");
        }

        if (statusAtual == novoStatus)
        {
            throw new BusinessConflictException($"O pedido já se encontra com o status '{novoStatus}'.");
        }

        // Fluxo principal: Pendente -> EmPreparacao -> ProntoParaRetirada -> Concluido
        var transicaoValida = (statusAtual, novoStatus) switch
        {
            (StatusPedido.Pendente, StatusPedido.EmPreparacao) => true,
            (StatusPedido.EmPreparacao, StatusPedido.ProntoParaRetirada) => true,
            (StatusPedido.ProntoParaRetirada, StatusPedido.Concluido) => true,
            _ => false
        };

        if (!transicaoValida)
        {
            throw new BusinessConflictException($"Transição inválida de '{statusAtual}' para '{novoStatus}'. Saltos de etapa ou retrocessos não são permitidos.");
        }
    }
}
