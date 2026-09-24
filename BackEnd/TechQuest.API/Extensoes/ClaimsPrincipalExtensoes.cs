using System.Security.Claims;
using TechQuest.Infrastructure.Seguranca;

namespace TechQuest.API.Extensoes;

/// <summary>
/// Le a identidade do usuario a partir do token, nunca de parametro enviado
/// pelo cliente. Se o ID viesse na URL ou no corpo, qualquer um poderia agir
/// no lugar de outro trocando um numero.
/// </summary>
public static class ClaimsPrincipalExtensoes
{
    public static int? IdUsuario(this ClaimsPrincipal user)
        => Ler(user, ClaimTypes.NameIdentifier);

    public static int? IdEstudante(this ClaimsPrincipal user)
        => Ler(user, JwtTokenService.ClaimIdEstudante);

    public static int? IdTutor(this ClaimsPrincipal user)
        => Ler(user, JwtTokenService.ClaimIdTutor);

    public static int? IdAdm(this ClaimsPrincipal user)
        => Ler(user, JwtTokenService.ClaimIdAdm);

    private static int? Ler(ClaimsPrincipal user, string tipo)
        => int.TryParse(user.FindFirstValue(tipo), out var id) ? id : null;
}
