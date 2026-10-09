using TechQuest.Application.Servicos;
using TechQuest.Domain.Entidades;
using TechQuest.Tests.Dubles;
using Xunit;

namespace TechQuest.Tests.Servicos;

/// <summary>
/// Testes da ORQUESTRACAO da conclusao de curso.
///
/// POR QUE ESTA CLASSE EXISTE: um defeito real chegou a producao. O
/// certificado nao era emitido para quem concluia todos os materiais ANTES
/// de fazer a prova -- a ordem natural de quem estuda antes de ser avaliado.
/// A causa nao estava no criterio de conclusao, que sempre esteve correto:
/// estava em so existir UM gatilho (concluir material) quando existem DOIS
/// (concluir material, passar na prova).
///
/// Nenhum teste de regra pura pegaria isso. O criterio, isolado, responde
/// certo a qualquer combinacao de entradas -- o problema era ninguem
/// perguntar a ele depois da prova. Por isso estes testes exercitam o
/// SERVICO, com dubles em memoria, e nao a funcao.
///
/// E a diferenca entre "a regra esta certa" e "a regra e aplicada na hora
/// certa". A suite tinha a primeira garantia e nao tinha a segunda.
/// </summary>
public class ConclusaoCursoServiceTests
{
    private const int Aluno = 1;
    private const int IdCurso = 10;
    private const int IdProva = 99;

    private readonly CursoRepositorioFalso _cursos = new();
    private readonly ProgressoRepositorioFalso _progressos = new();
    private readonly DesempenhoRepositorioFalso _desempenhos = new();
    private readonly HistoricoRepositorioFalso _historicos = new();
    private readonly CertificadoRepositorioFalso _certificados = new();
    private readonly UnidadeDeTrabalhoFalsa _uow = new();

    private ConclusaoCursoService CriarServico() => new(
        _cursos, _progressos, _desempenhos, _historicos, _certificados, _uow);

    /// <param name="comProva">false cria um curso sem prova vinculada.</param>
    private void DadoUmCurso(int totalMateriais, bool comProva = true)
    {
        _cursos.Cursos.Add(new Curso
        {
            IdCurso = IdCurso,
            Nome = "Curso de teste",
            Status = "Publicado",
            IdProva = comProva ? IdProva : null
        });
        _progressos.Contagens[(Aluno, IdCurso)] = (totalMateriais, 0);
    }

    private void AlunoConcluiuMateriais(int quantos)
    {
        var (total, _) = _progressos.Contagens[(Aluno, IdCurso)];
        _progressos.Contagens[(Aluno, IdCurso)] = (total, quantos);
    }

    private void AlunoPassouNaProva() => _desempenhos.Aprovacoes.Add((Aluno, IdProva));

    // =====================================================================
    // O DEFEITO QUE ORIGINOU ESTA CLASSE
    // =====================================================================

    /// <summary>
    /// A ORDEM QUE QUEBRAVA: materiais primeiro, prova depois.
    ///
    /// Antes da correcao, a conclusao so era reavaliada ao concluir um
    /// material. Quem terminava os materiais e so entao fazia a prova ficava
    /// sem certificado para sempre: a ultima avaliacao rodou com a prova
    /// ainda reprovada, e a aprovacao nao disparava nova avaliacao.
    /// </summary>
    [Fact]
    public async Task Emite_certificado_quando_a_prova_e_aprovada_depois_dos_materiais()
    {
        DadoUmCurso(totalMateriais: 3);
        var servico = CriarServico();

        // 1. Aluno conclui os tres materiais. Ainda nao ha prova aprovada.
        AlunoConcluiuMateriais(3);
        var antes = await servico.AvaliarAsync(Aluno, IdCurso);

        Assert.False(antes);
        Assert.Empty(_certificados.Emitidos);

        // 2. Agora passa na prova -- este e o segundo gatilho.
        AlunoPassouNaProva();
        var depois = await servico.AvaliarPorProvaAsync(Aluno, IdProva);

        Assert.True(depois);
        Assert.Single(_certificados.Emitidos);
        Assert.Equal("Concluido", _historicos.Historicos.Single().StatusConclusao);
    }

    /// <summary>
    /// A ORDEM QUE FUNCIONAVA POR ACIDENTE: prova primeiro, ultimo material
    /// depois. Continua funcionando -- a correcao acrescentou um gatilho, nao
    /// trocou o que havia.
    /// </summary>
    [Fact]
    public async Task Emite_certificado_quando_o_ultimo_material_e_concluido_depois_da_prova()
    {
        DadoUmCurso(totalMateriais: 3);
        var servico = CriarServico();

        AlunoPassouNaProva();
        AlunoConcluiuMateriais(2);
        Assert.False(await servico.AvaliarAsync(Aluno, IdCurso));
        Assert.Empty(_certificados.Emitidos);

        AlunoConcluiuMateriais(3);
        Assert.True(await servico.AvaliarAsync(Aluno, IdCurso));
        Assert.Single(_certificados.Emitidos);
    }

    // =====================================================================
    // CASOS QUE NAO PODEM EMITIR CERTIFICADO
    // =====================================================================

    [Fact]
    public async Task Nao_emite_com_materiais_pendentes_mesmo_aprovado_na_prova()
    {
        DadoUmCurso(totalMateriais: 5);
        AlunoPassouNaProva();
        AlunoConcluiuMateriais(4);

        Assert.False(await CriarServico().AvaliarAsync(Aluno, IdCurso));
        Assert.Empty(_certificados.Emitidos);
    }

    [Fact]
    public async Task Nao_emite_com_todos_os_materiais_mas_sem_aprovacao_na_prova()
    {
        DadoUmCurso(totalMateriais: 3);
        AlunoConcluiuMateriais(3);

        Assert.False(await CriarServico().AvaliarAsync(Aluno, IdCurso));
        Assert.Empty(_certificados.Emitidos);
    }

    /// <summary>
    /// Curso vazio nao certifica. Sem a guarda, "0 de 0 materiais" seria
    /// 100% e emitiria certificado de um curso onde nao havia o que estudar.
    /// </summary>
    [Fact]
    public async Task Nao_emite_para_curso_sem_material_nenhum()
    {
        DadoUmCurso(totalMateriais: 0, comProva: false);

        Assert.False(await CriarServico().AvaliarAsync(Aluno, IdCurso));
        Assert.Empty(_certificados.Emitidos);
    }

    [Fact]
    public async Task Curso_inexistente_nao_quebra_e_nao_emite()
    {
        Assert.False(await CriarServico().AvaliarAsync(Aluno, idCurso: 404));
        Assert.Empty(_certificados.Emitidos);
    }

    [Fact]
    public async Task Prova_que_nao_pertence_a_curso_algum_nao_emite()
    {
        DadoUmCurso(totalMateriais: 2);
        AlunoConcluiuMateriais(2);
        AlunoPassouNaProva();

        Assert.False(await CriarServico().AvaliarPorProvaAsync(Aluno, idProva: 777));
        Assert.Empty(_certificados.Emitidos);
    }

    // =====================================================================
    // CURSO SEM PROVA
    // =====================================================================

    /// <summary>
    /// ID_Prova e opcional em Curso. Um curso so de leitura se conclui com os
    /// materiais -- exigir prova inexistente travaria o aluno para sempre.
    /// </summary>
    [Fact]
    public async Task Curso_sem_prova_conclui_so_com_os_materiais()
    {
        DadoUmCurso(totalMateriais: 2, comProva: false);
        AlunoConcluiuMateriais(2);

        Assert.True(await CriarServico().AvaliarAsync(Aluno, IdCurso));
        Assert.Single(_certificados.Emitidos);
    }

    // =====================================================================
    // IDEMPOTENCIA
    // =====================================================================

    /// <summary>
    /// Os dois gatilhos podem disparar para o mesmo curso -- concluir o
    /// ultimo material e, logo depois, refazer a prova. Sem a guarda de
    /// "ja concluido", o aluno acumularia um certificado por avaliacao.
    /// </summary>
    [Fact]
    public async Task Avaliar_varias_vezes_nao_emite_certificado_duplicado()
    {
        DadoUmCurso(totalMateriais: 2);
        AlunoConcluiuMateriais(2);
        AlunoPassouNaProva();
        var servico = CriarServico();

        Assert.True(await servico.AvaliarAsync(Aluno, IdCurso));
        Assert.True(await servico.AvaliarAsync(Aluno, IdCurso));
        Assert.True(await servico.AvaliarPorProvaAsync(Aluno, IdProva));

        Assert.Single(_certificados.Emitidos);
        Assert.Single(_historicos.Historicos);
    }

    /// <summary>
    /// O aluno ja matriculado tem Historico "Em andamento"; a conclusao deve
    /// ATUALIZAR essa linha, nao criar uma segunda matricula no mesmo curso.
    /// </summary>
    [Fact]
    public async Task Reaproveita_o_historico_da_matricula_em_vez_de_criar_outro()
    {
        DadoUmCurso(totalMateriais: 1);
        _historicos.Adicionar(new Historico
        {
            IdEstudante = Aluno, IdCurso = IdCurso, StatusConclusao = "Em andamento"
        });
        AlunoConcluiuMateriais(1);
        AlunoPassouNaProva();

        Assert.True(await CriarServico().AvaliarAsync(Aluno, IdCurso));

        var h = Assert.Single(_historicos.Historicos);
        Assert.Equal("Concluido", h.StatusConclusao);
        Assert.NotNull(h.DataConclusao);
        Assert.Equal(h.IdHistorico, _certificados.Emitidos.Single().IdHistorico);
    }

    /// <summary>
    /// O codigo de autenticacao NAO e gerado pela aplicacao: a coluna tem
    /// DEFAULT NEWID() no banco. Dois geradores para o mesmo identificador
    /// seriam duas fontes da verdade. Aqui ele sai Guid.Empty porque nao ha
    /// banco -- e e isso que o teste fixa.
    /// </summary>
    [Fact]
    public async Task Nao_gera_o_codigo_de_autenticacao_na_aplicacao()
    {
        DadoUmCurso(totalMateriais: 1, comProva: false);
        AlunoConcluiuMateriais(1);

        await CriarServico().AvaliarAsync(Aluno, IdCurso);

        Assert.Equal(Guid.Empty, _certificados.Emitidos.Single().CodigoAutenticacao);
    }
}
