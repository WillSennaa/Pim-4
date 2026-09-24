namespace TechQuest.Application.Interfaces;

/// <summary>
/// Unit of Work: os repositorios apenas registram as mudancas; quem decide
/// QUANDO gravar e o servico de negocio.
///
/// POR QUE: concluir um curso grava Historico e Certificado. As duas gravacoes
/// precisam acontecer juntas ou nenhuma — sem isso um certificado poderia ser
/// emitido para um historico que nao foi salvo.
/// </summary>
public interface IUnidadeDeTrabalho
{
    Task SalvarAsync(CancellationToken ct = default);

    /// <summary>Executa a acao dentro de uma transacao unica.</summary>
    Task ExecutarEmTransacaoAsync(Func<Task> acao, CancellationToken ct = default);
}
