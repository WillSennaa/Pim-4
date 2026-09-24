using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechQuest.API.Extensoes;
using TechQuest.Application.Dtos;
using TechQuest.Application.Servicos;
using TechQuest.Domain.Enums;

namespace TechQuest.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = nameof(PapelUsuario.Admin))]
public class AdminController : ControllerBase
{
    private readonly AdminService _admin;
    public AdminController(AdminService admin) => _admin = admin;

    [HttpGet("resumo")]
    public async Task<IActionResult> Resumo(CancellationToken ct)
        => Ok(await _admin.ResumoAsync(ct));

    // ---------------- AVALIACAO DE CURSOS ----------------
    /// <summary>Cursos aguardando avaliacao (padrao: Pendente).</summary>
    [HttpGet("cursos")]
    public async Task<IActionResult> Cursos([FromQuery] string? status, CancellationToken ct)
        => Ok(await _admin.ListarSolicitacoesAsync(status, ct));

    [HttpPost("cursos/{idCurso:int}/aprovar")]
    public async Task<IActionResult> Aprovar(int idCurso, CancellationToken ct)
    {
        var idAdm = User.IdAdm();
        if (idAdm is null) return Forbid();
        return Traduzir(await _admin.AprovarAsync(idAdm.Value, idCurso, ct));
    }

    /// <summary>
    /// Rejeita o curso. O motivo vira um chamado para o tutor, porque a tabela
    /// Curso nao tem campo de parecer e Chamado ja e o canal do modelo.
    /// </summary>
    [HttpPost("cursos/{idCurso:int}/rejeitar")]
    public async Task<IActionResult> Rejeitar(
        int idCurso, [FromBody] AvaliarCursoRequest req, CancellationToken ct)
    {
        var idAdm = User.IdAdm();
        if (idAdm is null) return Forbid();
        return Traduzir(await _admin.RejeitarAsync(idAdm.Value, idCurso, req.Motivo, ct));
    }

    // ---------------- USUARIOS ----------------
    [HttpGet("usuarios")]
    public async Task<IActionResult> Usuarios(CancellationToken ct)
        => Ok(await _admin.ListarUsuariosAsync(ct));

    [HttpPost("usuarios")]
    public async Task<IActionResult> CriarUsuario(
        [FromBody] CriarUsuarioRequest req, CancellationToken ct)
        => Traduzir(await _admin.CriarUsuarioAsync(req, ct));

    /// <summary>
    /// Ativa ou desativa um usuario. E este endpoint que dispara a trigger
    /// TRG_Auditoria_StatusUsuario do banco: a linha em Log_Auditoria aparece
    /// sozinha, sem a API inserir nada.
    /// </summary>
    [HttpPatch("usuarios/{idUsuario:int}/status")]
    public async Task<IActionResult> AlterarStatus(
        int idUsuario, [FromBody] AlterarStatusRequest req, CancellationToken ct)
    {
        var idAdm = User.IdAdm();
        if (idAdm is null) return Forbid();

        var r = await _admin.AlterarStatusAsync(idAdm.Value, idUsuario, req.Ativo, ct);
        return r.Status == StatusOperacao.Ok ? NoContent() : Traduzir(r);
    }

    [HttpGet("logs")]
    public async Task<IActionResult> Logs([FromQuery] int limite = 100, CancellationToken ct = default)
        => Ok(await _admin.ListarLogsAsync(limite, ct));

    private IActionResult Traduzir<T>(Resultado<T> r) => r.Status switch
    {
        StatusOperacao.Ok => Ok(r.Valor),
        StatusOperacao.NaoEncontrado => NotFound(new { mensagem = r.Mensagem }),
        StatusOperacao.SemPermissao => StatusCode(StatusCodes.Status403Forbidden,
                                                  new { mensagem = r.Mensagem }),
        _ => BadRequest(new { mensagem = r.Mensagem })
    };
}
