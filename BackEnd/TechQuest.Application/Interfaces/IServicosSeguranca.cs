using TechQuest.Domain.Entidades;
using TechQuest.Domain.Enums;

namespace TechQuest.Application.Interfaces;

public interface IHashSenhaService
{
    string GerarHash(string senha);
    bool Verificar(string senha, string hash);
}

public interface ITokenService
{
    (string Token, DateTime ExpiraEm) Gerar(Usuario usuario, PapelUsuario papel);
}
