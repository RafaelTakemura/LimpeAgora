using LimpeAgora.Shared.DTOs.Produtos;
using LimpeAgoraApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LimpeAgoraApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _produtoService;

    public ProdutosController(IProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ProdutoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProdutoResponse>>> Listar(
        [FromQuery] Guid? categoriaId = null,
        [FromQuery] bool? apenasAtivos = null)
    {
        var produtos = await _produtoService.ListarAsync(categoriaId, apenasAtivos);
        return Ok(produtos);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProdutoResponse>> ObterPorId(Guid id)
    {
        var produto = await _produtoService.ObterPorIdAsync(id);
        return Ok(produto);
    }

    [HttpPost]
    [Authorize(Roles = "Funcionario,Administrador")]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProdutoResponse>> Criar([FromBody] CriarProdutoRequest request)
    {
        var response = await _produtoService.CriarAsync(request);
        return CreatedAtAction(nameof(ObterPorId), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Funcionario,Administrador")]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProdutoResponse>> Atualizar(Guid id, [FromBody] AtualizarProdutoRequest request)
    {
        var response = await _produtoService.AtualizarAsync(id, request);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Funcionario,Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inativar(Guid id)
    {
        await _produtoService.InativarAsync(id);
        return NoContent();
    }

    [HttpPatch("{id:guid}/estoque")]
    [Authorize(Roles = "Funcionario,Administrador")]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProdutoResponse>> AjustarEstoque(Guid id, [FromBody] AjustarEstoqueRequest request)
    {
        var response = await _produtoService.AjustarEstoqueAsync(id, request);
        return Ok(response);
    }
}
