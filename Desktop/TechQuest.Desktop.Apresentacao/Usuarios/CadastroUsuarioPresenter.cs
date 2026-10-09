using TechQuest.Desktop.Aplicacao.Comum;
using TechQuest.Desktop.Aplicacao.Seguranca;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Comum;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Apresentacao.Usuarios;

public interface ICadastroUsuarioView : IViewBase
{
    string Nome { get; }
    string Email { get; }
    string Senha { get; }
    PapelUsuario Papel { get; }

    void DefinirSenha(string senha);

    /// <summary>
    /// Conta criada. A tela mostra (e oferece copiar) as credenciais e fecha.
    /// A senha so existe neste momento: o servidor guarda apenas o hash.
    /// </summary>
    void Concluir(UsuarioAdmin criado, string senhaInicial);
}

/// <summary>
/// Criacao de conta com papel escolhido pelo administrador.
///
/// E O UNICO JEITO DE CRIAR TUTOR OU ADMIN: o cadastro publico do site cria
/// sempre Estudante (RegistrarRequest nao tem campo de papel), para ninguem
/// se cadastrar sozinho como administrador.
///
/// SENHA INICIAL GERADA, NAO DIGITADA: a plataforma nao tem servico de
/// e-mail (limitacao declarada), entao o admin repassa a senha a pessoa.
/// Gerada por RandomNumberGenerator, ela nao segue padrao que o admin
/// repetiria em todas as contas ("Mudar123", "Tutor2026"...). O campo
/// continua editavel para quem preferir definir outra.
/// </summary>
public sealed class CadastroUsuarioPresenter : PresenterBase
{
    private readonly ICadastroUsuarioView _view;
    private readonly UsuariosService _servico;

    public CadastroUsuarioPresenter(ICadastroUsuarioView view, UsuariosService servico, SessaoAdmin sessao)
        : base(view, sessao)
    {
        _view = view;
        _servico = servico;
    }

    public void Iniciar() => GerarSenha();

    public void GerarSenha() => _view.DefinirSenha(GeradorDeSenha.Gerar());

    public async Task SalvarAsync()
    {
        var senha = _view.Senha;

        ResultadoOperacao<UsuarioAdmin>? r = null;
        if (!await ExecutarAsync(async () =>
                r = await _servico.CriarAsync(_view.Nome, _view.Email, senha, _view.Papel)))
            return;

        if (!r!.Sucesso) { _view.MostrarErro(r.Mensagem!); return; }
        _view.Concluir(r.Valor!, senha);
    }
}
