using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Medalhas;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.UI.Formularios;

/// <summary>Dialogo de criacao e edicao de medalha.</summary>
public partial class FormMedalha : Form, IMedalhaEditorView
{
    private sealed record OpcaoRaridade(string Valor, string Rotulo)
    {
        public override string ToString() => Rotulo;
    }

    private readonly MedalhaEditorPresenter _presenter;

    public MedalhaAdmin? Salva { get; private set; }

    public FormMedalha(MedalhasService servico, SessaoAdmin sessao, MedalhaAdmin? existente)
    {
        InitializeComponent();
        foreach (var r in Raridades.Todas)
            cmbRaridade.Items.Add(new OpcaoRaridade(r, MedalhasPresenter.NomeDaRaridade(r)));

        _presenter = new MedalhaEditorPresenter(this, servico, sessao, existente);
        _presenter.Iniciar();
    }

    // ---------------- IMedalhaEditorView ----------------

    public string Nome => txtNome.Text;
    public string? Raridade => (cmbRaridade.SelectedItem as OpcaoRaridade)?.Valor;
    public string Descricao => txtDescricao.Text;

    public void Preencher(string titulo, string nome, string raridade, string descricao, string? aviso)
    {
        Text = titulo;
        lblTitulo.Text = titulo;
        txtNome.Text = nome;
        txtDescricao.Text = descricao;
        cmbRaridade.SelectedItem = cmbRaridade.Items.Cast<OpcaoRaridade>().FirstOrDefault(o => o.Valor == raridade);
        lblAviso.Text = aviso ?? "";
    }

    public bool Confirmar(string mensagem, string titulo)
        => MessageBox.Show(this, mensagem, titulo, MessageBoxButtons.YesNo,
               MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;

    public void Concluir(MedalhaAdmin salva)
    {
        Salva = salva;
        DialogResult = DialogResult.OK;
        Close();
    }

    public void DefinirOcupado(bool ocupado)
    {
        if (IsDisposed) return;
        UseWaitCursor = ocupado;
        btnSalvar.Enabled = !ocupado;
        btnCancelar.Enabled = !ocupado;
        btnSalvar.Text = ocupado ? "Salvando..." : "Salvar";
        if (ocupado) lblErro.Text = "";
    }

    public void MostrarErro(string mensagem) => lblErro.Text = mensagem;

    private async void btnSalvar_Click(object? sender, EventArgs e) => await _presenter.SalvarAsync();
}
