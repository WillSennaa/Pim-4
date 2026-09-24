using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechQuest.API.Extensoes;
using TechQuest.Application.Dtos;
using TechQuest.Application.Servicos;
using TechQuest.Domain.Enums;

namespace TechQuest.API.Controllers;

/// <summary>
/// Area do tutor. Duas camadas de protecao: o atributo Authorize barra quem
/// nao tem o papel, e o servico verifica se o curso pertence a ESTE tutor.
/// O atributo sozinho nao bastaria — um tutor continuaria alcancando o curso
/// de outro tutor.
/// </summary>
[ApiController]
[Route("api/tutor")]
[Authorize(Roles = nameof(PapelUsuario.Tutor))]
public class TutorController : ControllerBase
{
    private readonly TutorService _tutor;
    public TutorController(TutorService tutor) => _tutor = tutor;

    // ---------------- CURSOS ----------------
    [HttpGet("cursos")]
    public async Task<IActionResult> MeusCursos(CancellationToken ct)
    {
        var id = User.IdTutor();
        return id is null ? Forbid() : Ok(await _tutor.ListarCursosAsync(id.Value, ct));
    }

    [HttpPost("cursos")]
    public async Task<IActionResult> CriarCurso([FromBody] CriarCursoRequest req, CancellationToken ct)
    {
        var id = User.IdTutor();
        if (id is null) return Forbid();
        return Traduzir(await _tutor.CriarCursoAsync(id.Value, req, ct));
    }

    [HttpPut("cursos/{idCurso:int}")]
    public async Task<IActionResult> AtualizarCurso(
        int idCurso, [FromBody] CriarCursoRequest req, CancellationToken ct)
    {
        var id = User.IdTutor();
        if (id is null) return Forbid();
        return Traduzir(await _tutor.AtualizarCursoAsync(id.Value, idCurso, req, ct));
    }

    /// <summary>Envia o curso para avaliacao do administrador.</summary>
    [HttpPost("cursos/{idCurso:int}/submeter")]
    public async Task<IActionResult> Submeter(int idCurso, CancellationToken ct)
    {
        var id = User.IdTutor();
        if (id is null) return Forbid();
        return Traduzir(await _tutor.SubmeterAsync(id.Value, idCurso, ct));
    }

    // ---------------- AULAS ----------------
    [HttpPost("cursos/{idCurso:int}/aulas")]
    public async Task<IActionResult> AdicionarAula(
        int idCurso, [FromBody] MaterialRequest req, CancellationToken ct)
    {
        var id = User.IdTutor();
        if (id is null) return Forbid();
        return Traduzir(await _tutor.AdicionarAulaAsync(id.Value, idCurso, req, ct));
    }

    [HttpDelete("aulas/{idMaterial:int}")]
    public async Task<IActionResult> RemoverAula(int idMaterial, CancellationToken ct)
    {
        var id = User.IdTutor();
        if (id is null) return Forbid();

        var r = await _tutor.RemoverAulaAsync(id.Value, idMaterial, ct);
        return r.Status == StatusOperacao.Ok ? NoContent() : Traduzir(r);
    }

    // ---------------- PROVA ----------------
    [HttpPost("cursos/{idCurso:int}/prova")]
    public async Task<IActionResult> CriarProva(
        int idCurso, [FromBody] CriarProvaRequest req, CancellationToken ct)
    {
        var id = User.IdTutor();
        if (id is null) return Forbid();
        return Traduzir(await _tutor.CriarProvaAsync(id.Value, idCurso, req, ct));
    }

    [HttpPost("provas/{idProva:int}/questoes")]
    public async Task<IActionResult> AdicionarQuestao(
        int idProva, [FromBody] CriarQuestaoRequest req, CancellationToken ct)
    {
        var id = User.IdTutor();
        if (id is null) return Forbid();
        return Traduzir(await _tutor.AdicionarQuestaoAsync(id.Value, idProva, req, ct));
    }

    // ---------------- ALUNOS ----------------
    [HttpGet("alunos")]
    public async Task<IActionResult> Alunos([FromQuery] int? idCurso, CancellationToken ct)
    {
        var id = User.IdTutor();
        return id is null ? Forbid() : Ok(await _tutor.ListarAlunosAsync(id.Value, idCurso, ct));
    }

    /// <summary>Concede manualmente uma medalha sem criterio automatico.</summary>
    [HttpPost("estudantes/{idEstudante:int}/medalhas/{idMedalha:int}")]
    public async Task<IActionResult> ConcederMedalha(
        int idEstudante, int idMedalha, CancellationToken ct)
        => Traduzir(await _tutor.ConcederMedalhaAsync(idEstudante, idMedalha, ct));

    /// <summary>
    /// Converte o resultado de negocio no status HTTP correspondente. Concentrar
    /// a traducao aqui evita repetir if/else em cada acao.
    /// </summary>
    private IActionResult Traduzir<T>(Resultado<T> r) => r.Status switch
    {
        StatusOperacao.Ok => Ok(r.Valor),
        StatusOperacao.NaoEncontrado => NotFound(new { mensagem = r.Mensagem }),
        StatusOperacao.SemPermissao => StatusCode(StatusCodes.Status403Forbidden,
                                                  new { mensagem = r.Mensagem }),
        _ => BadRequest(new { mensagem = r.Mensagem })
    };
}
