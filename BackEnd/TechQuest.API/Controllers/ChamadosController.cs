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

    /// <summary>
    /// Abre um chamado. O destinatario e decidido pelo servidor a partir do
    /// tipo: duvida vai ao tutor do curso, tecnico vai a fila dos admins.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ChamadoDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Abrir([FromBody] AbrirChamadoRequest req, CancellationToken ct)
    {
        var id = User.IdUsuario();
        if (id is null) return Unauthorized();

        var r = await _chamados.AbrirAsync(id.Value, req, ct);
        return r.Status == StatusOperacao.Ok
            ? CreatedAtAction(nameof(Listar), new { id = r.Valor!.Id }, r.Valor)
            : Traduzir(r);
    }

    /// <summary>Duvidas enderecadas a mim (usado pelo tutor).</summary>
    [HttpGet("recebidos")]
    public async Task<IActionResult> Recebidos(CancellationToken ct)
    {
        var id = User.IdUsuario();
        return id is null ? Unauthorized() : Ok(await _chamados.ListarRecebidosAsync(id.Value, ct));
    }

    /// <summary>Responde ao chamado e o marca como resolvido.</summary>
    [HttpPost("{id:int}/responder")]
    public async Task<IActionResult> Responder(
        int id, [FromBody] ResponderChamadoRequest req, CancellationToken ct)
    {
        var idUsuario = User.IdUsuario();
        if (idUsuario is null) return Unauthorized();

        var r = await _chamados.ResponderAsync(id, idUsuario.Value, req.Descricao, ct);
        return r.Status == StatusOperacao.Ok ? NoContent() : Traduzir(r);
    }

    /// <summary>Muda a situacao: Aberto, Em andamento ou Resolvido.</summary>
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> AlterarStatus(
        int id, [FromBody] AtualizarStatusChamadoRequest req, CancellationToken ct)
    {
        var idUsuario = User.IdUsuario();
        if (idUsuario is null) return Unauthorized();

        var r = await _chamados.AtualizarStatusAsync(id, idUsuario.Value, req.Status, ct);
        return r.Status == StatusOperacao.Ok ? NoContent() : Traduzir(r);
    }

    private IActionResult Traduzir<T>(Resultado<T> r) => r.Status switch
    {
        StatusOperacao.Ok => Ok(r.Valor),
        StatusOperacao.NaoEncontrado => NotFound(new { mensagem = r.Mensagem }),
        StatusOperacao.SemPermissao => StatusCode(StatusCodes.Status403Forbidden,
                                                  new { mensagem = r.Mensagem }),
        _ => BadRequest(new { mensagem = r.Mensagem })
    };

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
