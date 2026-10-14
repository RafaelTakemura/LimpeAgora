using LimpeAgora.Shared.DTOs.Usuarios;
using LimpeAgoraApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LimpeAgoraApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<UsuarioResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UsuarioResponse>>> Listar()
    {
        var usuarios = await _usuarioService.ListarAsync();
        return Ok(usuarios);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioResponse>> ObterPorId(Guid id)
    {
        var usuario = await _usuarioService.ObterPorIdAsync(id);
        return Ok(usuario);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioResponse>> Criar([FromBody] CriarUsuarioRequest request)
    {
        var response = await _usuarioService.CriarAsync(request);
        return CreatedAtAction(nameof(ObterPorId), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioResponse>> Atualizar(Guid id, [FromBody] AtualizarUsuarioRequest request)
    {
        var response = await _usuarioService.AtualizarAsync(id, request);
        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inativar(Guid id)
    {
        await _usuarioService.InativarAsync(id);
        return NoContent();
    }
}
