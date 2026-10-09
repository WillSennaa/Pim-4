using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Revisao;
using TechQuest.Desktop.UI.Componentes;
using TechQuest.Desktop.UI.Estilo;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.UI.Formularios;

/// <summary>
/// Janela de revisao de um curso. Tres abas: visao geral (com os pontos de
/// atencao), materiais com o texto completo e prova com o gabarito.
///
/// JANELA MODAL, e nao mais uma secao do menu: a revisao e uma tarefa com
/// comeco e fim (ler, decidir, fechar). Enquanto ela esta aberta o admin nao
/// troca de secao no meio de uma avaliacao, e ao fechar volta exatamente para
/// a fila de onde saiu.
/// </summary>
public partial class FormRevisaoCurso : Form, IRevisaoCursoView
{
    private readonly RevisaoCursoPresenter _presenter;
    private RevisaoExibicao? _revisao;

    public FormRevisaoCurso(AvaliacaoCursosService servico, SessaoAdmin sessao, int idCurso)
    {
        InitializeComponent();
        _presenter = new RevisaoCursoPresenter(this, servico, sessao, idCurso);
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await _presenter.CarregarAsync();
    }

    // ---------------- IRevisaoCursoView ----------------

    public string MotivoRejeicao => txtMotivo.Text;

    public void Exibir(RevisaoExibicao r)
    {
        if (IsDisposed) return;
        _revisao = r;

        Text = "Revisão de curso — " + r.Nome;
        lblNome.Text = r.Nome;
        lblInfo.Text = $"Tutor: {r.Tutor}   ·   {r.Categoria}   ·   {r.Nivel}   ·   {r.CargaHoraria}";
        PintarStatus(r.Status);

        tabMateriais.Text = r.TituloAbaMateriais;
        tabProva.Text = r.TituloAbaProva;
        tabs.Enabled = true;

        EscreverVisaoGeral(r);
        PreencherMateriais(r);
        EscreverProva(r);

        // Curso que nao esta Pendente: a janela vira consulta, sem avaliacao.
        lblMotivo.Visible = txtMotivo.Visible = r.PodeAvaliar;
        btnAprovar.Visible = btnRejeitar.Visible = r.PodeAvaliar;
        btnAprovar.Enabled = btnRejeitar.Enabled = r.PodeAvaliar;
        if (!r.PodeAvaliar)
        {
            lblErro.ForeColor = Tema.TextoSecundario;
            lblErro.Text = $"Curso {r.Status.ToLowerInvariant()}: não há avaliação pendente. " +
                           "Esta janela serve para consulta e auditoria do conteúdo.";
        }
    }

    public void DefinirOcupado(bool ocupado)
    {
        if (IsDisposed) return;
        UseWaitCursor = ocupado;
        var podeAvaliar = _revisao?.PodeAvaliar == true;
        btnAprovar.Enabled = !ocupado && podeAvaliar;
        btnRejeitar.Enabled = !ocupado && podeAvaliar;
        txtMotivo.ReadOnly = ocupado;
        btnFechar.Enabled = !ocupado;
        if (ocupado && podeAvaliar) lblErro.Text = "";
    }

    public void MostrarErro(string mensagem)
    {
        if (IsDisposed) return;
        lblErro.ForeColor = Tema.Perigo;
        lblErro.Text = mensagem;
    }

    public void FocarMotivo() => txtMotivo.Focus();

    public bool Confirmar(string mensagem, string titulo)
        // Botao padrao = "Nao": um Enter distraido nao publica nem rejeita.
        => MessageBox.Show(this, mensagem, titulo, MessageBoxButtons.YesNo,
               MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;

    public void Concluir(string mensagem)
    {
        MessageBox.Show(this, mensagem, "Avaliação registrada",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        DialogResult = DialogResult.OK;
        Close();
    }

    // ---------------- renderizacao ----------------

    private void PintarStatus(string status)
    {
        lblStatus.Text = status.ToUpperInvariant();
        (lblStatus.BackColor, lblStatus.ForeColor) = status switch
        {
            StatusCurso.Pendente => (Tema.AlertaClaro, Tema.Alerta),
            StatusCurso.Publicado => (Tema.SucessoClaro, Tema.Sucesso),
            StatusCurso.Rejeitado => (Tema.PerigoClaro, Tema.Perigo),
            _ => (Tema.NeutroClaro, Tema.Neutro)
        };
    }

    private void EscreverVisaoGeral(RevisaoExibicao r)
    {
        var e = new EscritorRichText(rtbGeral);

        if (r.PontosDeAtencao.Count > 0)
        {
            e.Linha("Pontos de atenção antes de publicar", Fontes.Negrito, Tema.Perigo);
            foreach (var ponto in r.PontosDeAtencao)
                e.Linha("•  " + ponto, Fontes.Normal, Tema.Perigo, recuo: 8);
        }
        else
        {
            e.Linha("✓  Materiais com conteúdo e prova com gabarito completo.", Fontes.Negrito, Tema.Sucesso);
        }

        e.Linha()
         .Linha("Descrição", Fontes.Negrito)
         .Linha(r.Descricao)
         .Linha()
         .Linha("Ficha do curso", Fontes.Negrito)
         .Texto("Tutor: ", Fontes.Normal, Tema.TextoSecundario).Linha(r.Tutor)
         .Texto("Categoria: ", Fontes.Normal, Tema.TextoSecundario).Linha(r.Categoria)
         .Texto("Nível: ", Fontes.Normal, Tema.TextoSecundario).Linha(r.Nivel)
         .Texto("Carga horária: ", Fontes.Normal, Tema.TextoSecundario).Linha(r.CargaHoraria)
         .Concluir();
    }

    private void PreencherMateriais(RevisaoExibicao r)
    {
        lstMateriais.Items.Clear();
        foreach (var m in r.Materiais)
            lstMateriais.Items.Add(m.SemConteudo ? m.Titulo + "  (sem conteúdo)" : m.Titulo);

        if (r.Materiais.Count > 0)
        {
            lstMateriais.SelectedIndex = 0; // dispara a exibicao do primeiro
        }
        else
        {
            new EscritorRichText(rtbMaterial)
                .Linha("Nenhum material cadastrado neste curso.", Fontes.Negrito, Tema.Perigo)
                .Concluir();
        }
    }

    private void lstMateriais_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_revisao is null || lstMateriais.SelectedIndex < 0) return;
        var m = _revisao.Materiais[lstMateriais.SelectedIndex];

        var escritor = new EscritorRichText(rtbMaterial)
            .Linha(m.Titulo, Fontes.Titulo)
            .Linha(m.Tipo.ToUpperInvariant(), Fontes.PequenaNegrito, Tema.TextoSuave)
            .Linha();

        if (m.SemConteudo)
        {
            escritor.Linha("Este material não tem conteúdo escrito. " +
                           "O aluno o abriria e não encontraria texto algum.", Fontes.Normal, Tema.Perigo);
        }

        foreach (var bloco in m.Blocos)
        {
            if (bloco.EhCodigo) escritor.Codigo(bloco.Texto);
            else escritor.Linha(bloco.Texto).Linha();
        }

        escritor.Concluir();
    }

    private void EscreverProva(RevisaoExibicao r)
    {
        var e = new EscritorRichText(rtbProva);

        if (r.Prova is null)
        {
            e.Linha("Este curso não tem prova.", Fontes.Negrito, Tema.Perigo)
             .Linha("Sem prova, o aluno conclui o curso apenas lendo os materiais.", Fontes.Normal, Tema.Perigo)
             .Concluir();
            return;
        }

        e.Linha(r.Prova.Titulo, Fontes.Titulo)
         .Linha(r.Prova.Resumo, Fontes.Pequena, Tema.TextoSecundario)
         .Linha();

        foreach (var q in r.Prova.Questoes)
        {
            e.Linha(q.Cabecalho.ToUpperInvariant(), Fontes.PequenaNegrito, Tema.TextoSuave)
             .Linha(q.Enunciado);

            if (q.Codigo is not null) e.Codigo(q.Codigo);

            foreach (var a in q.Alternativas)
            {
                if (a.Correta)
                    e.Linha($"✓  {a.Letra})  {a.Texto}", Fontes.Negrito, Tema.Sucesso, Tema.SucessoClaro, recuo: 8);
                else
                    e.Linha($"○  {a.Letra})  {a.Texto}", Fontes.Normal, Tema.Texto, recuo: 8);
            }

            if (q.SemGabarito)
                e.Linha("Nenhuma alternativa marcada como correta.", Fontes.Negrito, Tema.Perigo, recuo: 8);

            e.Linha();
        }

        e.Concluir();
    }

    // ---------------- eventos ----------------

    private async void btnAprovar_Click(object? sender, EventArgs e) => await _presenter.AprovarAsync();

    private async void btnRejeitar_Click(object? sender, EventArgs e) => await _presenter.RejeitarAsync();
}
