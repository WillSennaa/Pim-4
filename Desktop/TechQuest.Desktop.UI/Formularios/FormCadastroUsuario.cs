using System.Runtime.InteropServices;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Usuarios;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.UI.Formularios;

/// <summary>Dialogo de criacao de conta com papel escolhido pelo administrador.</summary>
public partial class FormCadastroUsuario : Form, ICadastroUsuarioView
{
    private sealed record OpcaoPapel(PapelUsuario Papel, string Rotulo)
    {
        public override string ToString() => Rotulo;
    }

    private readonly CadastroUsuarioPresenter _presenter;

    /// <summary>Conta criada; lida por quem abriu o dialogo depois do OK.</summary>
    public UsuarioAdmin? Criado { get; private set; }

    public FormCadastroUsuario(UsuariosService servico, SessaoAdmin sessao)
    {
        InitializeComponent();

        // Tutor primeiro, como no web (criar-tutor.html): e o caso mais comum.
        // Estudante se cadastra sozinho pelo site; criar aqui e excecao.
        cmbPapel.Items.AddRange(new object[]
        {
            new OpcaoPapel(PapelUsuario.Tutor, "Tutor"),
            new OpcaoPapel(PapelUsuario.Admin, "Administrador"),
            new OpcaoPapel(PapelUsuario.Estudante, "Estudante")
        });
        cmbPapel.SelectedIndex = 0;

        _presenter = new CadastroUsuarioPresenter(this, servico, sessao);
        _presenter.Iniciar();
    }

    // ---------------- ICadastroUsuarioView ----------------

    public string Nome => txtNome.Text;
    public string Email => txtEmail.Text;
    public string Senha => txtSenha.Text;
    public PapelUsuario Papel => ((OpcaoPapel)cmbPapel.SelectedItem!).Papel;

    public void DefinirSenha(string senha) => txtSenha.Text = senha;

    public void Concluir(UsuarioAdmin criado, string senhaInicial)
    {
        Criado = criado;
        var papel = UsuariosPresenter.NomeDoPapel(criado.Papel).ToLowerInvariant();

        // As credenciais vao para a area de transferencia: o admin cola no
        // canal que preferir, sem redigitar (e sem errar) a senha gerada.
        var copiou = true;
        try
        {
            Clipboard.SetText($"E-mail: {criado.Email}\r\nSenha: {senhaInicial}");
        }
        catch (ExternalException)
        {
            copiou = false; // area de transferencia ocupada por outro programa
        }

        MessageBox.Show(this,
            $"Conta de {criado.Nome} criada com o perfil {papel}.\n\n" +
            $"E-mail: {criado.Email}\nSenha inicial: {senhaInicial}\n\n" +
            (copiou ? "As credenciais foram copiadas para a área de transferência. " : "") +
            "Esta senha não será exibida de novo: o servidor guarda apenas o hash.",
            "Conta criada", MessageBoxButtons.OK, MessageBoxIcon.Information);

        DialogResult = DialogResult.OK;
        Close();
    }

    public void DefinirOcupado(bool ocupado)
    {
        if (IsDisposed) return;
        UseWaitCursor = ocupado;
        btnCriar.Enabled = !ocupado;
        btnCancelar.Enabled = !ocupado;
        btnGerar.Enabled = !ocupado;
        btnCriar.Text = ocupado ? "Criando..." : "Criar conta";
        if (ocupado) lblErro.Text = "";
    }

    public void MostrarErro(string mensagem) => lblErro.Text = mensagem;

    // ---------------- eventos ----------------

    private async void btnCriar_Click(object? sender, EventArgs e) => await _presenter.SalvarAsync();

    private void btnGerar_Click(object? sender, EventArgs e) => _presenter.GerarSenha();

    private void chkMostrar_CheckedChanged(object? sender, EventArgs e)
        => txtSenha.UseSystemPasswordChar = !chkMostrar.Checked;
}
