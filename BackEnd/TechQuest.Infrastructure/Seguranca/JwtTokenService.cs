using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;
using TechQuest.Domain.Enums;

namespace TechQuest.Infrastructure.Seguranca;

/// <summary>
/// Emite JWT assinado com HMAC-SHA256.
///
/// POR QUE JWT: o token e autocontido, entao a mesma API atende o site, o app
/// mobile e a aplicacao desktop sem estado de sessao no servidor.
/// ALTERNATIVA REJEITADA: cookie de sessao, que depende de estado no servidor
/// e nao funciona bem fora do navegador.
/// </summary>
public class JwtTokenService : ITokenService
{
    public const string ClaimIdEstudante = "idEstudante";
    public const string ClaimIdTutor = "idTutor";
    public const string ClaimIdAdm = "idAdm";

    private readonly IConfiguration _config;
    public JwtTokenService(IConfiguration config) => _config = config;

    public (string Token, DateTime ExpiraEm) Gerar(Usuario usuario, PapelUsuario papel)
    {
        var chave = _config["Jwt:Chave"]
            ?? throw new InvalidOperationException("Jwt:Chave nao configurada.");
        var emissor = _config["Jwt:Emissor"] ?? "TechQuest.API";
        var audiencia = _config["Jwt:Audiencia"] ?? "TechQuest.Clientes";
        var horas = int.TryParse(_config["Jwt:HorasValidade"], out var h) ? h : 8;

        var expiraEm = DateTime.UtcNow.AddHours(horas);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),  // claim padrao "sub"
            new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Role, papel.ToString())
        };

        if (!string.IsNullOrWhiteSpace(usuario.Email))
            claims.Add(new Claim(ClaimTypes.Email, usuario.Email));

        // Os IDs das especializacoes vao no token para evitar uma consulta
        // extra em todo endpoint que precisa saber QUAL tutor ou QUAL aluno e.
        // Igualmente importante: como vem do token assinado, o cliente nao
        // consegue trocar o numero e agir no lugar de outra pessoa.
        if (usuario.Estudante is not null)
            claims.Add(new Claim(ClaimIdEstudante, usuario.Estudante.IdEstudante.ToString()));

        if (usuario.Tutor is not null)
            claims.Add(new Claim(ClaimIdTutor, usuario.Tutor.IdTutor.ToString()));

        if (usuario.Adm is not null)
            claims.Add(new Claim(ClaimIdAdm, usuario.Adm.IdAdm.ToString()));

        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: emissor,
            audience: audiencia,
            claims: claims,
            expires: expiraEm,
            signingCredentials: credenciais);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }
}
