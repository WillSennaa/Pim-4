using TechQuest.Desktop.Aplicacao.Comum;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Comum;

namespace TechQuest.Desktop.Apresentacao.Login;

/// <summary>
/// Logica da tela de login. Le os campos pela interface, pede ao servico,
/// e diz a tela o que mostrar. Nao sabe que existe TextBox nem MessageBox.
/// </summary>
public sealed class LoginPresenter : PresenterBase
{
    private readonly ILoginView _view;
    private readonly AutenticacaoService _autenticacao;

    public LoginPresenter(ILoginView view, AutenticacaoService autenticacao, SessaoAdmin sessao)
        : base(view, sessao)
    {
        _view = view;
        _autenticacao = autenticacao;
    }

    public async Task EntrarAsync()
    {
        ResultadoOperacao? resultado = null;

        var concluiu = await ExecutarAsync(async () =>
            resultado = await _autenticacao.EntrarAsync(_view.Email, _view.Senha));

        if (!concluiu || resultado is null) return; // falha de rede: ja exibida

        if (!resultado.Sucesso)
        {
            _view.LimparSenha();
            _view.MostrarErro(resultado.Mensagem ?? "Não foi possível entrar.");
            return;
        }

        _view.ConcluirLogin();
    }
}
