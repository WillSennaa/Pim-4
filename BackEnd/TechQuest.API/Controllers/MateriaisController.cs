using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechQuest.API.Extensoes;
using TechQuest.Application.Dtos;
using TechQuest.Application.Servicos;
using TechQuest.Domain.Enums;

namespace TechQuest.API.Controllers;

[ApiController]
[Route("api/materiais")]
[Authorize(Roles = nameof(PapelUsuario.Estudante))]
public class MateriaisController : ControllerBase
{
    private readonly ProgressoService _progresso;
    public MateriaisController(ProgressoService progresso) => _progresso = progresso;

    /// <summary>
    /// Registra o avanco na aula. E aqui que o XP sai do lugar: a resposta ja
    /// devolve XP, nivel, percentual do curso e medalhas recem-conquistadas,
    /// para a tela atualizar tudo sem uma segunda chamada.
    ///
    /// PUT e nao POST porque a operacao e idempotente: enviar duas vezes o
    /// mesmo progresso deixa o sistema no mesmo estado.
    /// </summary>
    [HttpPut("{id:int}/progresso")]
    [ProducesResponseType(typeof(ProgressoAtualizadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(
        int id, [FromBody] AtualizarProgressoRequest req, CancellationToken ct)
    {
        var idEstudante = User.IdEstudante();
        if (idEstudante is null) return Forbid();

        var resultado = await _progresso.AtualizarAsync(idEstudante.Value, id, req, ct);
        return resultado is null
            ? NotFound(new { mensagem = "Aula nao encontrada." })
            : Ok(resultado);
    }
}
