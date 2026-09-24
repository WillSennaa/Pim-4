using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechQuest.API.Extensoes;
using TechQuest.Application.Dtos;
using TechQuest.Application.Servicos;
using TechQuest.Domain.Enums;

namespace TechQuest.API.Controllers;

[ApiController]
[Route("api/provas")]
[Authorize]
public class ProvasController : ControllerBase
{
    private readonly ProvaService _servico;
    public ProvasController(ProvaService servico) => _servico = servico;

    /// <summary>
    /// Prova para responder. Substitui questoes.json — e, diferente dele,
    /// nao entrega o gabarito ao navegador.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProvaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(int id, CancellationToken ct)
    {
        var prova = await _servico.ObterParaResponderAsync(id, ct);
        return prova is null
            ? NotFound(new { mensagem = "Prova nao encontrada." })
            : Ok(prova);
    }

    /// <summary>
    /// Submete as respostas, corrige no servidor, grava em Desempenho e
    /// devolve nota, aprovacao e gabarito.
    /// </summary>
    [HttpPost("{id:int}/tentativas")]
    [Authorize(Roles = nameof(PapelUsuario.Estudante))]
    [ProducesResponseType(typeof(ResultadoTentativaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Submeter(
        int id, [FromBody] TentativaRequest req, CancellationToken ct)
    {
        var idEstudante = User.IdEstudante();
        if (idEstudante is null)
            return Forbid();

        if (req.Respostas is null || req.Respostas.Count == 0)
            return BadRequest(new { mensagem = "Nenhuma resposta enviada." });

        var resultado = await _servico.CorrigirAsync(id, idEstudante.Value, req, ct);
        return resultado is null
            ? NotFound(new { mensagem = "Prova nao encontrada." })
            : Ok(resultado);
    }
}
