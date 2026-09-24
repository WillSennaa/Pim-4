using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechQuest.Application.Dtos;
using TechQuest.Application.Servicos;

namespace TechQuest.API.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly AutenticacaoService _servico;
    public AuthController(AutenticacaoService servico) => _servico = servico;

    /// <summary>Autentica e devolve o token JWT.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Senha))
            return BadRequest(new { mensagem = "Informe e-mail e senha." });

        var resultado = await _servico.AutenticarAsync(req, ct);

        // Mensagem generica de proposito: nao informa se o e-mail existe.
        return resultado is null
            ? Unauthorized(new { mensagem = "E-mail ou senha invalidos." })
            : Ok(resultado);
    }

    /// <summary>
    /// Cadastro publico (link "Cadastre-se" da tela de login). Cria sempre uma
    /// conta de Estudante e ja devolve o token, para o usuario entrar direto.
    /// </summary>
    [HttpPost("registrar")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarRequest req, CancellationToken ct)
    {
        var resultado = await _servico.RegistrarAsync(req, ct);

        return resultado.Status == StatusOperacao.Ok
            ? Created(string.Empty, resultado.Valor)
            : BadRequest(new { mensagem = resultado.Mensagem });
    }
}
