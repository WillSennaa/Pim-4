using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Painel;
using TechQuest.Desktop.Apresentacao.Principal;
using TechQuest.Desktop.UI.Estilo;

namespace TechQuest.Desktop.UI.Telas;

/// <summary>
/// View do Painel. Copia textos prontos do presenter para os cartoes e
/// repassa os tres cliques (Atualizar, Tentar novamente, Ir para aprovacoes).
/// </summary>
public partial class TelaPainel : UserControl, IPainelView
{
    private readonly PainelPresenter _presenter;

    public TelaPainel(PainelService servico, INavegador navegador, SessaoAdmin sessao)
    {
        InitializeComponent();
        _presenter = new PainelPresenter(this, servico, navegador, sessao);
    }

    /// <summary>Carrega ao aparecer. Cada visita ao Painel busca dados novos.</summary>
    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await _presenter.CarregarAsync();
    }

    // ---------------- IPainelView ----------------
    // Todos os metodos conferem IsDisposed: se o admin trocar de secao com a
    // requisicao em andamento, a resposta chega para uma tela ja descartada.

    public void Exibir(PainelExibicao d)
    {
        if (IsDisposed) return;

        cartaoUsuarios.Valor = d.TotalUsuarios;
        cartaoAtivos.Valor = d.UsuariosAtivos;
        cartaoAtivos.Detalhe = d.DetalheAtivos;
        cartaoEstudantes.Valor = d.Estudantes;
        cartaoTutores.Valor = d.Tutores;
        cartaoPublicados.Valor = d.CursosPublicados;
        cartaoPendentes.Valor = d.CursosPendentes;
        cartaoMatriculas.Valor = d.Matriculas;
        cartaoCertificados.Valor = d.Certificados;
        cartaoChamados.Valor = d.ChamadosAbertos;

        lblAtalhoTitulo.Text = d.TituloAprovacoes;
        lblAtalhoDetalhe.Text = d.DetalheAprovacoes;

        // Fila com curso esperando ganha o amarelo de alerta do web
        // (--color-warning-light); fila vazia volta ao branco neutro.
        pnlAtalhoAprovacoes.BackColor = d.HaAprovacoesPendentes ? Tema.AlertaClaro : Tema.Superficie;
        lblAtalhoTitulo.ForeColor = d.HaAprovacoesPendentes ? Tema.Alerta : Tema.Texto;
        cartaoPendentes.CorValor = d.HaAprovacoesPendentes ? Tema.Alerta : Tema.Texto;

        lblAtualizado.Text = d.AtualizadoEm;
    }

    public void DefinirOcupado(bool ocupado)
    {
        if (IsDisposed) return;
        UseWaitCursor = ocupado;
        btnAtualizar.Enabled = !ocupado;
        btnTentarNovamente.Enabled = !ocupado;
        btnAtualizar.Text = ocupado ? "Atualizando..." : "Atualizar";
    }

    public void MostrarErro(string mensagem)
    {
        if (IsDisposed) return;
        lblFalha.Text = mensagem;
        pnlFalha.Visible = true;
    }

    public void OcultarErro()
    {
        if (IsDisposed) return;
        pnlFalha.Visible = false;
    }

    // ---------------- eventos ----------------

    private async void btnAtualizar_Click(object? sender, EventArgs e) => await _presenter.CarregarAsync();

    private async void btnTentarNovamente_Click(object? sender, EventArgs e) => await _presenter.CarregarAsync();

    private void btnRevisar_Click(object? sender, EventArgs e) => _presenter.IrParaAprovacoes();
}
