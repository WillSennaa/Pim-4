using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechQuest.API.Extensoes;
using TechQuest.Application.Dtos;
using TechQuest.Application.Servicos;

namespace TechQuest.API.Controllers;

/// <summary>Duvidas e solicitacoes (tabela Chamado do modelo).</summary>
[ApiController]
[Route("api/chamados")]
[Authorize]
public class ChamadosController : ControllerBase
{
    private readonly ChamadoService _chamados;
    public ChamadosController(ChamadoService chamados) => _chamados = chamados;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ChamadoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var id = User.IdUsuario();
        return id is null ? Unauthorized() : Ok(await _chamados.ListarAsync(id.Value, ct));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ChamadoDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Abrir([FromBody] AbrirChamadoRequest req, CancellationToken ct)
    {
        var id = User.IdUsuario();
        if (id is null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(req.Assunto) || string.IsNullOrWhiteSpace(req.Descricao))
            return BadRequest(new { mensagem = "Assunto e descricao sao obrigatorios." });

        var chamado = await _chamados.AbrirAsync(id.Value, req, ct);
        return CreatedAtAction(nameof(Listar), new { id = chamado.Id }, chamado);
    }

    [HttpPost("{id:int}/fechar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Fechar(int id, CancellationToken ct)
    {
        var idUsuario = User.IdUsuario();
        if (idUsuario is null) return Unauthorized();

        var ok = await _chamados.FecharAsync(id, idUsuario.Value, ct);
        return ok ? NoContent() : NotFound(new { mensagem = "Chamado nao encontrado ou sem permissao." });
    }
}
