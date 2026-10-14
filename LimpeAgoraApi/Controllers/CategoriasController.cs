using LimpeAgora.Shared.DTOs.Categorias;
using LimpeAgoraApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LimpeAgoraApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _categoriaService;

    public CategoriasController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<CategoriaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CategoriaResponse>>> Listar([FromQuery] bool? apenasAtivas = null)
    {
        var categorias = await _categoriaService.ListarAsync(apenasAtivas);
        return Ok(categorias);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CategoriaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoriaResponse>> ObterPorId(Guid id)
    {
        var categoria = await _categoriaService.ObterPorIdAsync(id);
        return Ok(categoria);
    }

    [HttpPost]
    [Authorize(Roles = "Funcionario,Administrador")]
    [ProducesResponseType(typeof(CategoriaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoriaResponse>> Criar([FromBody] CategoriaUpsertRequest request)
    {
        var response = await _categoriaService.CriarAsync(request);
        return CreatedAtAction(nameof(ObterPorId), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Funcionario,Administrador")]
    [ProducesResponseType(typeof(CategoriaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoriaResponse>> Atualizar(Guid id, [FromBody] CategoriaUpsertRequest request)
    {
        var response = await _categoriaService.AtualizarAsync(id, request);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Funcionario,Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inativar(Guid id)
    {
        await _categoriaService.InativarAsync(id);
        return NoContent();
    }
}
