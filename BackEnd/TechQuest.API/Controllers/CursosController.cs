using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechQuest.API.Extensoes;
using TechQuest.Application.Dtos;
using TechQuest.Application.Servicos;
using TechQuest.Domain.Enums;

namespace TechQuest.API.Controllers;

[ApiController]
[Route("api/cursos")]
[Authorize]
public class CursosController : ControllerBase
{
    private readonly CursoService _cursos;
    private readonly ProgressoService _progresso;

    public CursosController(CursoService cursos, ProgressoService progresso)
    {
        _cursos = cursos;
        _progresso = progresso;
    }

    /// <summary>Cursos publicados. Substitui mock.json > cursos[].</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CursoResumoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
        => Ok(await _cursos.ListarAsync(ct));

    /// <summary>Curso com aulas e, se o usuario for estudante, seu progresso.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CursoDetalheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(int id, CancellationToken ct)
    {
        var curso = await _cursos.ObterAsync(id, User.IdEstudante(), ct);
        return curso is null
            ? NotFound(new { mensagem = "Curso nao encontrado." })
            : Ok(curso);
    }

    /// <summary>
    /// Matricula o estudante logado. Idempotente: chamar de novo devolve a
    /// matricula existente em vez de duplicar.
    /// </summary>
    [HttpPost("{id:int}/matricula")]
    [Authorize(Roles = nameof(PapelUsuario.Estudante))]
    [ProducesResponseType(typeof(MatriculaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Matricular(int id, CancellationToken ct)
    {
        var idEstudante = User.IdEstudante();
        if (idEstudante is null) return Forbid();

        var matricula = await _progresso.MatricularAsync(idEstudante.Value, id, ct);
        return matricula is null
            ? NotFound(new { mensagem = "Curso nao encontrado ou nao publicado." })
            : Ok(matricula);
    }
}
