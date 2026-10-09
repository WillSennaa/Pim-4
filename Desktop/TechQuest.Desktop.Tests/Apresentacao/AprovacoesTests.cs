using TechQuest.Desktop.Aplicacao.Erros;
using TechQuest.Desktop.Aplicacao.Estado;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Aprovacoes;
using TechQuest.Desktop.Apresentacao.Comum;
using TechQuest.Desktop.Apresentacao.Revisao;
using TechQuest.Desktop.Modelos;
using TechQuest.Desktop.Tests.Dubles;
using Xunit;

namespace TechQuest.Desktop.Tests.Apresentacao;

public class AprovacoesPresenterTests
{
    private static (AprovacoesPresenter presenter, AprovacoesViewFalsa tela, CursosAdminGatewayFalso api) Montar()
    {
        var sessao = new SessaoAdmin();
        sessao.Iniciar(Fabrica.Resposta(PapelUsuario.Admin));
        var api = new CursosAdminGatewayFalso();
        var tela = new AprovacoesViewFalsa();
        var servico = new AvaliacaoCursosService(api, new ContadorDePendencias());
        return (new AprovacoesPresenter(tela, servico, sessao), tela, api);
    }

    [Fact]
    public async Task Fila_formata_aulas_e_prova_e_marca_curso_incompleto()
    {
        var (presenter, tela, api) = Montar();
        api.Fila = new()
        {
            CursosDeTeste.Solicitacao(1, aulas: 1, questoes: 1),
            CursosDeTeste.Solicitacao(2, aulas: 4, prova: false, questoes: 0)
        };

        await presenter.CarregarAsync();

        var (a, b) = (tela.Linhas![0], tela.Linhas[1]);
        Assert.Equal(("1 aula", "1 questão", false), (a.Aulas, a.Prova, a.Incompleto));
        Assert.Equal(("4 aulas", "sem prova", true), (b.Aulas, b.Prova, b.Incompleto));
        Assert.Equal("2 cursos aguardando avaliação.", tela.Resumo);
    }

    [Fact]
    public async Task Fila_vazia_tem_resumo_proprio()
    {
        var (presenter, tela, _) = Montar();

        await presenter.CarregarAsync();

        Assert.Empty(tela.Linhas!);
        Assert.Equal("Nenhum curso aguardando avaliação.", tela.Resumo);
    }

    [Fact]
    public async Task Revisar_abre_o_curso_selecionado_e_recarrega_a_fila_ao_fechar()
    {
        var (presenter, tela, api) = Montar();
        api.Fila = new() { CursosDeTeste.Solicitacao(9) };
        await presenter.CarregarAsync();
        tela.CursoSelecionado = 9;

        await presenter.RevisarSelecionadoAsync();

        Assert.Equal(new[] { 9 }, tela.RevisoesAbertas);
        Assert.Equal(2, api.Listagens);
    }

    [Fact]
    public async Task Sem_selecao_nao_abre_revisao()
    {
        var (presenter, tela, api) = Montar();

        await presenter.RevisarSelecionadoAsync();

        Assert.Empty(tela.RevisoesAbertas);
        Assert.Equal(0, api.Listagens);
    }
}

public class RevisaoCursoPresenterTests
{
    private static (RevisaoCursoPresenter presenter, RevisaoViewFalsa tela, CursosAdminGatewayFalso api) Montar(
        CursoRevisao curso)
    {
        var sessao = new SessaoAdmin();
        sessao.Iniciar(Fabrica.Resposta(PapelUsuario.Admin));
        var api = new CursosAdminGatewayFalso { Curso = curso };
        var tela = new RevisaoViewFalsa();
        var servico = new AvaliacaoCursosService(api, new ContadorDePendencias());
        return (new RevisaoCursoPresenter(tela, servico, sessao, curso.IdCurso), tela, api);
    }

    [Fact]
    public async Task Exibe_o_curso_com_material_dividido_e_prova_com_gabarito()
    {
        var (presenter, tela, _) = Montar(CursosDeTeste.Completo());

        await presenter.CarregarAsync();

        var r = tela.Exibido!;
        Assert.True(r.PodeAvaliar);
        Assert.Equal("10 horas", r.CargaHoraria);
        Assert.Equal("Materiais (1)", r.TituloAbaMateriais);
        Assert.Equal("1. Variaveis", r.Materiais[0].Titulo);
        Assert.Equal(3, r.Materiais[0].Blocos.Count);       // texto, codigo, texto
        Assert.True(r.Materiais[0].Blocos[1].EhCodigo);
        Assert.Equal("Nota mínima 7,0 · 30 minutos · 1 questão", r.Prova!.Resumo);
        Assert.True(r.Prova.Questoes[0].Alternativas[1].Correta);
        Assert.Empty(r.PontosDeAtencao);
    }

    [Theory]
    [InlineData(StatusCurso.Publicado)]
    [InlineData(StatusCurso.Rejeitado)]
    public async Task Curso_fora_da_fila_abre_so_para_consulta(string status)
    {
        var (presenter, tela, api) = Montar(CursosDeTeste.Completo(status));
        await presenter.CarregarAsync();

        await presenter.AprovarAsync();
        tela.MotivoRejeicao = "Motivo suficientemente longo.";
        await presenter.RejeitarAsync();

        Assert.False(tela.Exibido!.PodeAvaliar);
        Assert.Empty(tela.Confirmacoes);
        Assert.Empty(api.Aprovados);
        Assert.Empty(api.Rejeitados);
    }

    [Fact]
    public async Task Aprovar_pede_confirmacao_e_publica()
    {
        var (presenter, tela, api) = Montar(CursosDeTeste.Completo());
        await presenter.CarregarAsync();

        await presenter.AprovarAsync();

        Assert.Single(tela.Confirmacoes);
        Assert.Contains("não poderá voltar a rascunho", tela.Confirmacoes[0]);
        Assert.Equal(new[] { 7 }, api.Aprovados);
        Assert.NotNull(tela.Conclusao);
    }

    [Fact]
    public async Task Confirmacao_de_curso_incompleto_lista_os_pontos_de_atencao()
    {
        var (presenter, tela, _) = Montar(CursosDeTeste.Completo() with { Prova = null });
        await presenter.CarregarAsync();

        await presenter.AprovarAsync();

        Assert.Contains("1 ponto de atenção", tela.Confirmacoes[0]);
        Assert.Contains("não tem prova", tela.Confirmacoes[0]);
    }

    [Fact]
    public async Task Aprovacao_cancelada_nao_chama_a_API()
    {
        var (presenter, tela, api) = Montar(CursosDeTeste.Completo());
        await presenter.CarregarAsync();
        tela.RespostaConfirmacao = false;

        await presenter.AprovarAsync();

        Assert.Empty(api.Aprovados);
        Assert.Null(tela.Conclusao);
    }

    [Fact]
    public async Task Curso_ja_avaliado_por_outro_admin_mostra_a_recusa_do_servidor()
    {
        var (presenter, tela, api) = Montar(CursosDeTeste.Completo());
        await presenter.CarregarAsync();
        api.FalhaNaDecisao = new OperacaoRecusadaException(
            "Um curso com status 'Publicado' nao pode ir para 'Publicado'.");

        await presenter.AprovarAsync();

        Assert.Contains("Publicado", tela.Erro);
        Assert.Null(tela.Conclusao);
    }

    [Fact]
    public async Task Motivo_invalido_e_avisado_antes_de_qualquer_confirmacao()
    {
        var (presenter, tela, api) = Montar(CursosDeTeste.Completo());
        await presenter.CarregarAsync();
        tela.MotivoRejeicao = "ruim";

        await presenter.RejeitarAsync();

        Assert.Empty(tela.Confirmacoes);
        Assert.True(tela.MotivoFocado);
        Assert.NotNull(tela.Erro);
        Assert.Empty(api.Rejeitados);
    }

    [Fact]
    public async Task Rejeitar_com_motivo_envia_ao_servidor_e_conclui()
    {
        var (presenter, tela, api) = Montar(CursosDeTeste.Completo());
        await presenter.CarregarAsync();
        tela.MotivoRejeicao = "A prova precisa de mais questões.";

        await presenter.RejeitarAsync();

        Assert.Single(tela.Confirmacoes);
        Assert.Equal((7, "A prova precisa de mais questões."), api.Rejeitados.Single());
        Assert.NotNull(tela.Conclusao);
    }
}

public class FormatadorConteudoTests
{
    [Fact]
    public void Separa_paragrafos_e_reconhece_codigo_pelo_recuo_de_quatro_espacos()
    {
        var blocos = FormatadorConteudo.Blocos(
            "Primeiro paragrafo\ncontinua aqui.\r\n\r\n    int x = 1;\n\n    x++;\n   \nFim.");

        Assert.Equal(new[]
        {
            new BlocoConteudo("Primeiro paragrafo\ncontinua aqui.", false),
            new BlocoConteudo("int x = 1;", true),
            new BlocoConteudo("x++;", true),
            new BlocoConteudo("Fim.", false)
        }, blocos);
    }

    [Fact]
    public void Bloco_com_uma_linha_sem_recuo_e_texto_e_nao_codigo()
    {
        var blocos = FormatadorConteudo.Blocos("    recuada\nnao recuada");

        Assert.False(Assert.Single(blocos).EhCodigo);
    }

    [Theory]
    [InlineData((string?)null)]
    [InlineData("")]
    [InlineData("  \n \n")]
    public void Conteudo_vazio_nao_gera_blocos(string? texto)
    {
        Assert.Empty(FormatadorConteudo.Blocos(texto));
    }
}
