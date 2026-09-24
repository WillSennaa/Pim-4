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

    public PerfilController(
        EstudanteService estudantes, ConquistaService conquistas, RelatorioService relatorios)
    {
        _estudantes = estudantes;
        _conquistas = conquistas;
        _relatorios = relatorios;
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
}
