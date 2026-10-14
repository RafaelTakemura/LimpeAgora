using LimpeAgora.Shared.DTOs.Pedidos;
using LimpeAgora.Shared.Enums;

namespace LimpeAgoraApi.Services.Interfaces;

public interface IPedidoService
{
    Task<PedidoDetalheResponse> CriarPedidoAsync(Guid usuarioId, CriarPedidoRequest request);
    Task<List<PedidoResumoResponse>> ListarMeusPedidosAsync(Guid usuarioId);
    Task<List<PedidoResumoResponse>> ListarTodosPedidosAsync(StatusPedido? status = null, Guid? usuarioId = null, DateTime? dataInicio = null, DateTime? dataFim = null);
    Task<PedidoDetalheResponse> ObterDetalhesPorIdAsync(Guid pedidoId, Guid usuarioRequisitanteId, bool isFuncionarioOuAdmin);
    Task CancelarPedidoAsync(Guid pedidoId, Guid usuarioRequisitanteId, bool isFuncionarioOuAdmin);
    Task<PedidoDetalheResponse> AlterarStatusPedidoAsync(Guid pedidoId, StatusPedido novoStatus, Guid usuarioResponsavelId);
    Task<List<HistoricoStatusResponse>> ObterHistoricoAsync(Guid pedidoId, Guid usuarioRequisitanteId, bool isFuncionarioOuAdmin);
}
