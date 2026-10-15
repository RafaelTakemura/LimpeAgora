using System.Security.Claims;
using LimpeAgora.Shared.DTOs.Pedidos;
using LimpeAgora.Shared.Enums;
using LimpeAgoraApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LimpeAgoraApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PedidosController : ControllerBase
{
    private readonly IPedidoService _pedidoService;

    public PedidosController(IPedidoService pedidoService)
    {
        _pedidoService = pedidoService;
    }

    [HttpPost]
    [Authorize(Roles = "Cliente,Funcionario,Administrador")]
    [ProducesResponseType(typeof(PedidoDetalheResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PedidoDetalheResponse>> CriarPedido([FromBody] CriarPedidoRequest request)
    {
        var usuarioId = ObterUsuarioIdLogado();
        var response = await _pedidoService.CriarPedidoAsync(usuarioId, request);
        return CreatedAtAction(nameof(ObterPorId), new { id = response.Id }, response);
    }

    [HttpGet("meus")]
    [Authorize(Roles = "Cliente,Funcionario,Administrador")]
    [ProducesResponseType(typeof(List<PedidoResumoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<PedidoResumoResponse>>> ListarMeusPedidos()
    {
        var usuarioId = ObterUsuarioIdLogado();
        var pedidos = await _pedidoService.ListarMeusPedidosAsync(usuarioId);
        return Ok(pedidos);
    }

    [HttpGet]
    [Authorize(Roles = "Funcionario,Administrador")]
    [ProducesResponseType(typeof(List<PedidoResumoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<PedidoResumoResponse>>> ListarTodos(
        [FromQuery] StatusPedido? status = null,
        [FromQuery] Guid? usuarioId = null,
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null)
    {
        var pedidos = await _pedidoService.ListarTodosPedidosAsync(status, usuarioId, dataInicio, dataFim);
        return Ok(pedidos);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PedidoDetalheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PedidoDetalheResponse>> ObterPorId(Guid id)
    {
        var usuarioId = ObterUsuarioIdLogado();
        var isFuncionarioOuAdmin = EhFuncionarioOuAdmin();

        var pedido = await _pedidoService.ObterDetalhesPorIdAsync(id, usuarioId, isFuncionarioOuAdmin);
        return Ok(pedido);
    }

    [HttpPost("{id:guid}/cancelamento")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelarPedido(Guid id)
    {
        var usuarioId = ObterUsuarioIdLogado();
        var isFuncionarioOuAdmin = EhFuncionarioOuAdmin();

        await _pedidoService.CancelarPedidoAsync(id, usuarioId, isFuncionarioOuAdmin);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Funcionario,Administrador")]
    [ProducesResponseType(typeof(PedidoDetalheResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PedidoDetalheResponse>> AlterarStatus(
        Guid id,
        [FromBody] AlterarStatusPedidoRequest request)
    {
        var usuarioId = ObterUsuarioIdLogado();
        var response = await _pedidoService.AlterarStatusPedidoAsync(id, request.NovoStatus, usuarioId);
        return Ok(response);
    }

    [HttpGet("{id:guid}/historico")]
    [ProducesResponseType(typeof(List<HistoricoStatusResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<HistoricoStatusResponse>>> ObterHistorico(Guid id)
    {
        var usuarioId = ObterUsuarioIdLogado();
        var isFuncionarioOuAdmin = EhFuncionarioOuAdmin();

        var historico = await _pedidoService.ObterHistoricoAsync(id, usuarioId, isFuncionarioOuAdmin);
        return Ok(historico);
    }

    private Guid ObterUsuarioIdLogado()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(idClaim, out var id))
        {
            return id;
        }

        throw new UnauthorizedAccessException("Usuário não autenticado ou identificador inválido no token.");
    }

    private bool EhFuncionarioOuAdmin()
    {
        return User.IsInRole("Funcionario") || User.IsInRole("Administrador");
    }
}
