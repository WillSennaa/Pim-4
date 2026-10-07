using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechQuest.API.Extensoes;
using TechQuest.Application.Dtos;
using TechQuest.Application.Servicos;

namespace TechQuest.API.Controllers;

/// <summary>
/// Dados do proprio usuario. Nenhum endpoint recebe ID de usuario por
/// parametro: tudo sai do token, entao ninguem consulta o perfil alheio
/// trocando um numero na URL.
/// </summary>
[ApiController]
[Route("api/perfil")]
[Authorize]
public class PerfilController : ControllerBase
{
    private readonly EstudanteService _estudantes;
    private readonly ConquistaService _conquistas;
    private readonly RelatorioService _relatorios;
    private readonly ContaService _conta;

    public PerfilController(
        EstudanteService estudantes, ConquistaService conquistas,
        RelatorioService relatorios, ContaService conta)
    {
        _estudantes = estudantes;
        _conquistas = conquistas;
        _relatorios = relatorios;
        _conta = conta;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PerfilDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Obter(CancellationToken ct)
    {
        var idUsuario = User.IdUsuario();
        if (idUsuario is null) return Unauthorized();

        var perfil = await _estudantes.ObterPerfilAsync(idUsuario.Value, User.IdEstudante(), ct);
        return perfil is null ? NotFound() : Ok(perfil);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Atualizar(
        [FromBody] AtualizarPerfilRequest req, CancellationToken ct)
    {
        var idUsuario = User.IdUsuario();
        if (idUsuario is null) return Unauthorized();

        var ok = await _estudantes.AtualizarPerfilAsync(idUsuario.Value, req, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpGet("historico")]
    [ProducesResponseType(typeof(IReadOnlyList<ItemHistoricoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Historico(CancellationToken ct)
    {
        var id = User.IdEstudante();
        return id is null ? Forbid() : Ok(await _estudantes.ObterHistoricoAsync(id.Value, ct));
    }

    /// <summary>Todas as medalhas, marcando as ja conquistadas.</summary>
    [HttpGet("conquistas")]
    [ProducesResponseType(typeof(IReadOnlyList<MedalhaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Conquistas(CancellationToken ct)
    {
        var id = User.IdEstudante();
        return id is null ? Forbid() : Ok(await _conquistas.ListarAsync(id.Value, ct));
    }

    [HttpGet("certificados")]
    [ProducesResponseType(typeof(IReadOnlyList<CertificadoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Certificados(CancellationToken ct)
    {
        var id = User.IdEstudante();
        return id is null ? Forbid() : Ok(await _estudantes.ListarCertificadosAsync(id.Value, ct));
    }

    /// <summary>Relatorio de desempenho gerado pela procedure do banco.</summary>
    [HttpGet("desempenho")]
    [ProducesResponseType(typeof(IReadOnlyList<LinhaRelatorioDesempenhoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Desempenho(CancellationToken ct)
    {
        var id = User.IdEstudante();
        return id is null ? Forbid() : Ok(await _relatorios.DesempenhoAsync(id.Value, ct));
    }

    // ---------------- conta do proprio usuario ----------------
    // Todas exigem a senha atual, mesmo com o usuario ja autenticado: um
    // token roubado ou uma sessao esquecida aberta nao podem tomar a conta.

    /// <summary>Troca a propria senha.</summary>
    [HttpPut("senha")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TrocarSenha(
        [FromBody] TrocarSenhaRequest req, CancellationToken ct)
    {
        var id = User.IdUsuario();
        if (id is null) return Unauthorized();

        var r = await _conta.TrocarSenhaAsync(id.Value, req, ct);
        return r.Status == StatusOperacao.Ok ? NoContent() : Traduzir(r);
    }

    /// <summary>
    /// Troca o proprio e-mail. O token continua valido, porque ele carrega o
    /// ID do usuario e nao o e-mail -- nao e preciso entrar de novo.
    /// </summary>
    [HttpPut("email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TrocarEmail(
        [FromBody] TrocarEmailRequest req, CancellationToken ct)
    {
        var id = User.IdUsuario();
        if (id is null) return Unauthorized();

        var r = await _conta.TrocarEmailAsync(id.Value, req, ct);
        return r.Status == StatusOperacao.Ok ? Ok(new { email = r.Valor }) : Traduzir(r);
    }

    /// <summary>
    /// Encerra a propria conta. Desativa e preserva o registro academico, em
    /// vez de apagar: certificados emitidos continuam validaveis por terceiros.
    /// </summary>
    [HttpPost("desativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DesativarConta(
        [FromBody] DesativarContaRequest req, CancellationToken ct)
    {
        var id = User.IdUsuario();
        if (id is null) return Unauthorized();

        var r = await _conta.DesativarContaAsync(id.Value, req, ct);
        return r.Status == StatusOperacao.Ok ? NoContent() : Traduzir(r);
    }

    /// <summary>Converte o resultado de negocio no status HTTP correspondente.</summary>
    private IActionResult Traduzir<T>(Resultado<T> r) => r.Status switch
    {
        StatusOperacao.Ok => Ok(r.Valor),
        StatusOperacao.NaoEncontrado => NotFound(new { mensagem = r.Mensagem }),
        StatusOperacao.SemPermissao => StatusCode(StatusCodes.Status403Forbidden,
                                                  new { mensagem = r.Mensagem }),
        _ => BadRequest(new { mensagem = r.Mensagem })
    };
}
