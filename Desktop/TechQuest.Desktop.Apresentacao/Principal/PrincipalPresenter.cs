using TechQuest.Desktop.Aplicacao.Estado;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;

namespace TechQuest.Desktop.Apresentacao.Principal;

/// <summary>
/// Navegacao entre secoes, logout e reacao a sessao expirada.
///
/// IDisposable PORQUE ASSINA EVENTOS: a SessaoAdmin e o ContadorDePendencias
/// vivem o programa inteiro, e a janela principal pode ser recriada a cada login. Sem
/// cancelar as assinaturas, eles segurariam uma referencia para cada
/// janela antiga, e um "Expirou" futuro tentaria fechar janelas ja
/// descartadas.
/// </summary>
public sealed class PrincipalPresenter : INavegador, IDisposable
{
    private readonly IPrincipalView _view;
    private readonly SessaoAdmin _sessao;
    private readonly AutenticacaoService _autenticacao;
    private readonly ContadorDePendencias _pendencias;
    private bool _encerrando;

    public SecaoAdmin? SecaoAtual { get; private set; }

    public PrincipalPresenter(
        IPrincipalView view, SessaoAdmin sessao, AutenticacaoService autenticacao,
        ContadorDePendencias pendencias)
    {
        _view = view;
        _sessao = sessao;
        _autenticacao = autenticacao;
        _pendencias = pendencias;
        _sessao.Expirou += AoExpirarSessao;
        _pendencias.Mudou += AoMudarPendencias;
    }

    public void Iniciar()
    {
        var usuario = _sessao.Usuario
            ?? throw new InvalidOperationException("Janela principal aberta sem sessão ativa.");

        _view.MostrarUsuario(usuario.Nome, usuario.Iniciais ?? IniciaisDe(usuario.Nome));
        _view.AtualizarSelo(SecaoAdmin.Aprovacoes, _pendencias.CursosPendentes);
        Navegar(SecaoAdmin.Painel);
    }

    public void Navegar(SecaoAdmin secao)
    {
        if (SecaoAtual == secao) return; // clicar no item ja aberto nao recarrega
        SecaoAtual = secao;
        _view.ExibirSecao(secao, secao.Titulo());
    }

    public void Sair()
    {
        if (!_view.ConfirmarSaida()) return;
        _encerrando = true;
        _autenticacao.Sair();
        _view.Fechar(SaidaPrincipal.VoltarAoLogin);
    }

    private void AoExpirarSessao(object? sender, EventArgs e)
    {
        if (_encerrando) return;
        _encerrando = true;
        _view.AvisarSessaoExpirada();
        _view.Fechar(SaidaPrincipal.VoltarAoLogin);
    }

    private void AoMudarPendencias(object? sender, EventArgs e)
        => _view.AtualizarSelo(SecaoAdmin.Aprovacoes, _pendencias.CursosPendentes);

    /// <summary>Reserva caso a API nao mande iniciais: "Ana Souza" vira "AS".</summary>
    public static string IniciaisDe(string nome)
    {
        var partes = nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return partes.Length switch
        {
            0 => "?",
            1 => partes[0][..1].ToUpperInvariant(),
            _ => (partes[0][..1] + partes[^1][..1]).ToUpperInvariant()
        };
    }

    public void Dispose()
    {
        _sessao.Expirou -= AoExpirarSessao;
        _pendencias.Mudou -= AoMudarPendencias;
    }
}
