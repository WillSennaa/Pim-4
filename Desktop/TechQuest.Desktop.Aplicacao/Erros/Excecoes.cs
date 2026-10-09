namespace TechQuest.Desktop.Aplicacao.Erros;

/// <summary>
/// Raiz das falhas que a aplicacao sabe explicar ao usuario.
///
/// POR QUE FICAM NESTA CAMADA E NAO NA INTEGRACAO: quem LANCA e a Integracao
/// (ao ler o status HTTP), mas quem TRATA sao os servicos e os presenters.
/// Se as excecoes morassem na Integracao, a Apresentacao teria de referenciar
/// a camada de HTTP so para dar nome aos erros. Declaradas aqui, a Integracao
/// as implementa "de baixo para cima", como no back-end, em que Application
/// declara os contratos e Infrastructure os cumpre.
///
/// A mensagem de toda excecao desta familia ja esta pronta para a tela.
/// </summary>
public abstract class TechQuestException : Exception
{
    protected TechQuestException(string mensagem, Exception? interna = null)
        : base(mensagem, interna) { }
}

/// <summary>
/// 401. Credencial recusada no login, ou token vencido/ausente depois dele.
/// Fora do login, significa sessao expirada: a tela volta ao login.
/// </summary>
public sealed class NaoAutenticadoException : TechQuestException
{
    public NaoAutenticadoException(string mensagem) : base(mensagem) { }
}

/// <summary>403. Autenticado, mas sem o papel exigido pelo endpoint.</summary>
public sealed class AcessoNegadoException : TechQuestException
{
    public AcessoNegadoException(string mensagem) : base(mensagem) { }
}

/// <summary>404. O registro nao existe (ou deixou de existir).</summary>
public sealed class RegistroNaoEncontradoException : TechQuestException
{
    public RegistroNaoEncontradoException(string mensagem) : base(mensagem) { }
}

/// <summary>
/// 400 e 409. O servidor recusou por regra de negocio ("curso ja publicado",
/// "e-mail ja cadastrado"). A mensagem vem do proprio servidor, que e quem
/// conhece a regra; o desktop apenas a exibe.
/// </summary>
public sealed class OperacaoRecusadaException : TechQuestException
{
    public OperacaoRecusadaException(string mensagem) : base(mensagem) { }
}

/// <summary>
/// Rede fora, tempo esgotado ou erro 5xx. Diferente das anteriores, aqui
/// TENTAR DE NOVO pode resolver: o Azure SQL serverless pausa sozinho e a
/// primeira requisicao depois da pausa costuma demorar.
/// </summary>
public sealed class ServidorIndisponivelException : TechQuestException
{
    public ServidorIndisponivelException(string mensagem, Exception? interna = null)
        : base(mensagem, interna) { }
}
