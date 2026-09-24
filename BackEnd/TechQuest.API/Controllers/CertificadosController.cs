using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechQuest.Application.Dtos;
using TechQuest.Application.Servicos;

namespace TechQuest.API.Controllers;

[ApiController]
[Route("api/certificados")]
public class CertificadosController : ControllerBase
{
    private readonly EstudanteService _estudantes;
    public CertificadosController(EstudanteService estudantes) => _estudantes = estudantes;

    /// <summary>
    /// Valida um certificado pelo codigo impresso nele.
    ///
    /// Deliberadamente publico: quem valida e um empregador, que nao tem conta
    /// na plataforma. O codigo e um GUID gerado pelo banco, entao nao da para
    /// adivinhar, e a resposta so expoe nome, curso e datas — nada sensivel.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{codigo:guid}")]
    [ProducesResponseType(typeof(CertificadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Validar(Guid codigo, CancellationToken ct)
    {
        var certificado = await _estudantes.ValidarAsync(codigo, ct);
        return certificado is null
            ? NotFound(new { mensagem = "Certificado nao encontrado.", valido = false })
            : Ok(certificado);
    }
}
