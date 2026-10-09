using TechQuest.Desktop.Aplicacao.Estado;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Principal;
using TechQuest.Desktop.UI.Estilo;
using TechQuest.Desktop.UI.Navegacao;

namespace TechQuest.Desktop.UI.Formularios;

/// <summary>
/// Janela principal: menu a esquerda, titulo no topo, secao no centro.
///
/// UMA JANELA, VARIAS SECOES (UserControl trocado no painel central), e nao
/// um Form por tela. Reproduz a navegacao do web, em que a barra lateral fica
/// fixa e so o conteudo muda; o admin nunca tem oito janelas abertas para
/// administrar.
///
/// ALTERNATIVA REJEITADA: MDI (janelas filhas dentro da principal). E o
/// modelo classico de WinForms, mas deixa o usuario com janelas sobrepostas
/// para organizar, e hoje parece datado diante do web que ele ja conhece.
/// </summary>
public partial class FormPrincipal : Form, IPrincipalView
{
    private readonly PrincipalPresenter _presenter;
    private readonly FabricaDeTelas _fabrica;
    private readonly Dictionary<SecaoAdmin, Button> _itensMenu = new();

    /// <summary>Lido por Program.cs para decidir entre voltar ao login ou encerrar.</summary>
    public SaidaPrincipal Saida { get; private set; } = SaidaPrincipal.Encerrar;

    public FormPrincipal(
        AutenticacaoService autenticacao, SessaoAdmin sessao,
        ContadorDePendencias pendencias, FabricaDeTelas fabrica)
    {
        InitializeComponent();
        _fabrica = fabrica;
        CriarMenu();
        _presenter = new PrincipalPresenter(this, sessao, autenticacao, pendencias);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        _presenter.Iniciar();
    }

    /// <summary>
    /// Os itens do menu saem do enum SecaoAdmin, e nao do designer. Uma secao
    /// nova no enum aparece no menu sem ninguem lembrar de arrastar um botao,
    /// e o texto do item e o titulo da secao vem da mesma fonte.
    /// </summary>
    private void CriarMenu()
    {
        foreach (var secao in Enum.GetValues<SecaoAdmin>())
        {
            var item = new Button
            {
                Text = secao.Titulo(),
                Tag = secao,
                Size = new Size(206, 40),
                Margin = new Padding(0, 2, 0, 2),
                Padding = new Padding(12, 0, 0, 0),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Tema.MenuTexto,
                BackColor = Tema.MenuFundo,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            item.FlatAppearance.BorderSize = 0;
            item.FlatAppearance.MouseOverBackColor = Tema.MenuHover;
            item.Click += (_, _) => _presenter.Navegar(secao);

            _itensMenu[secao] = item;
            flpMenu.Controls.Add(item);
        }
    }

    // ---------------- IPrincipalView ----------------

    public void MostrarUsuario(string nome, string iniciais)
    {
        lblUsuario.Text = nome;
        lblIniciais.Text = iniciais;
    }

    public void ExibirSecao(SecaoAdmin secao, string titulo)
    {
        lblTituloSecao.Text = titulo;

        foreach (var (s, botao) in _itensMenu)
        {
            var ativo = s == secao;
            botao.BackColor = ativo ? Tema.Primaria : Tema.MenuFundo;
            botao.ForeColor = ativo ? Color.White : Tema.MenuTexto;
            botao.FlatAppearance.MouseOverBackColor = ativo ? Tema.Primaria : Tema.MenuHover;
        }

        // A tela anterior e DESCARTADA, nao escondida: assim cada visita
        // recarrega da API e o admin nunca ve dado velho (um curso que outro
        // admin acabou de aprovar, por exemplo).
        var anterior = pnlConteudo.Controls.Cast<Control>().ToList();
        pnlConteudo.Controls.Clear();
        foreach (var c in anterior) c.Dispose();

        var tela = _fabrica.Criar(secao, _presenter);
        tela.Dock = DockStyle.Fill;
        pnlConteudo.Controls.Add(tela);
    }

    public void AtualizarSelo(SecaoAdmin secao, int? quantidade)
    {
        if (IsDisposed || !_itensMenu.TryGetValue(secao, out var item)) return;
        item.Text = quantidade is > 0 ? $"{secao.Titulo()}   ({quantidade})" : secao.Titulo();
    }

    public bool ConfirmarSaida()
        => MessageBox.Show(this, "Deseja sair da sua conta?", "Sair",
               MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    public void AvisarSessaoExpirada()
        => MessageBox.Show(this, "Sua sessão expirou. Entre novamente para continuar.",
               "Sessão expirada", MessageBoxButtons.OK, MessageBoxIcon.Information);

    /// <summary>
    /// Pode ser chamado de dentro de uma janela modal (a revisao de curso,
    /// quando a sessao expira durante uma aprovacao). Por isso fecha antes as
    /// janelas filhas e adia o proprio fechamento com BeginInvoke: o Close so
    /// roda depois que o laco de mensagens da modal terminar, sem fechar a
    /// janela dona debaixo de uma filha ainda aberta.
    /// </summary>
    public void Fechar(SaidaPrincipal saida)
    {
        Saida = saida;
        foreach (var filha in OwnedForms) filha.Close();
        BeginInvoke(new Action(Close));
    }

    // ---------------- eventos ----------------

    private void btnSair_Click(object? sender, EventArgs e) => _presenter.Sair();
}
