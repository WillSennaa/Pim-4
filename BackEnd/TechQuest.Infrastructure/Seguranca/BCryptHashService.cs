using TechQuest.Application.Interfaces;

namespace TechQuest.Infrastructure.Seguranca;

/// <summary>
/// BCrypt com fator de trabalho 11.
///
/// POR QUE: o hash e deliberadamente lento e embute salt aleatorio por senha,
/// o que inviabiliza rainbow tables e encarece ataque de forca bruta.
/// ALTERNATIVAS REJEITADAS: SHA-256, projetado para ser rapido (ruim para
/// senhas); MD5, ja quebrado; senha em texto puro, indefensavel.
/// </summary>
public class BCryptHashService : IHashSenhaService
{
    private const int FatorTrabalho = 11;

    public string GerarHash(string senha) =>
        BCrypt.Net.BCrypt.HashPassword(senha, FatorTrabalho);

    public bool Verificar(string senha, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(senha, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Hash em formato invalido (ex.: senha legada em texto puro).
            return false;
        }
    }
}
