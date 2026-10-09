using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Aplicacao.Interfaces;

/// <summary>
/// Acesso ao endpoint de autenticacao da API.
///
/// O QUE E UM GATEWAY: objeto que encapsula o acesso a um sistema externo
/// (Fowler, Patterns of Enterprise Application Architecture). Cumpre no
/// desktop o papel que o repositorio cumpre no back-end: o servico pede
/// "autentique este e-mail" sem saber que existe HTTP, JSON ou URL.
///
/// POR QUE A INTERFACE MORA NA APLICACAO (inversao de dependencia): a
/// Aplicacao define o que precisa e a Integracao fornece. Assim os servicos
/// podem ser exercitados com um duble em memoria, sem rede.
///
/// ALTERNATIVA REJEITADA: chamar o nome de "Repository". Repositorio sugere
/// colecao de entidades persistidas; aqui o que existe do outro lado e uma
/// API com regras proprias, e o nome Gateway deixa isso claro.
/// </summary>
public interface IAutenticacaoGateway
{
    /// <exception cref="Erros.NaoAutenticadoException">E-mail ou senha recusados.</exception>
    Task<RespostaLogin> EntrarAsync(CredenciaisLogin credenciais, CancellationToken ct = default);
}
