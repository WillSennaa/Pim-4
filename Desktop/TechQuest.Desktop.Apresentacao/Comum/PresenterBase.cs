using TechQuest.Desktop.Aplicacao.Erros;
using TechQuest.Desktop.Aplicacao.Sessao;

namespace TechQuest.Desktop.Apresentacao.Comum;

/// <summary>
/// Comportamento comum dos presenters: travar a tela durante a chamada e
/// transformar falhas em mensagem.
///
/// POR QUE HERANCA AQUI: todo presenter precisa exatamente do mesmo
/// try/catch/finally. Sem a classe base, cada tela repetiria o bloco, e
/// bastaria uma esquecer o finally para ficar travada depois de um erro.
///
/// SESSAO EXPIRADA E TRATADA AQUI, UMA VEZ SO: qualquer tela que receba
/// 401 chama SessaoAdmin.Expirar(); a janela principal, que escuta o evento,
/// leva o usuario de volta ao login. Nenhuma tela precisa saber como.
/// </summary>
public abstract class PresenterBase
{
    private readonly IViewBase _view;
    protected SessaoAdmin Sessao { get; }

    protected PresenterBase(IViewBase view, SessaoAdmin sessao)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        Sessao = sessao ?? throw new ArgumentNullException(nameof(sessao));
    }

    /// <summary>
    /// Executa a acao com a tela travada. Devolve false se houve falha, ja
    /// comunicada ao usuario; quem chama so precisa decidir se continua.
    ///
    /// Excecoes FORA da familia TechQuestException nao sao capturadas: sao
    /// defeito de programa, e o tratador global de Program.cs as registra.
    /// Engoli-las aqui esconderia o defeito atras de uma mensagem generica.
    /// </summary>
    protected async Task<bool> ExecutarAsync(Func<Task> acao)
    {
        _view.DefinirOcupado(true);
        try
        {
            await acao();
            return true;
        }
        catch (NaoAutenticadoException)
        {
            Sessao.Expirar();
            return false;
        }
        catch (TechQuestException ex)
        {
            _view.MostrarErro(ex.Message);
            return false;
        }
        finally
        {
            _view.DefinirOcupado(false);
        }
    }
}
