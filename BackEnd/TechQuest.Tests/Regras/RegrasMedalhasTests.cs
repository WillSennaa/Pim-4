using TechQuest.Domain.Regras;
using Xunit;

namespace TechQuest.Tests.Regras;

public class RegrasMedalhasTests
{
    // Catalogo de exemplo: curso 1 e 2 tem medalha propria, curso 3 nao.
    private static readonly IReadOnlyDictionary<int, string> MedalhasDeCurso =
        new Dictionary<int, string> { [1] = "Mestre em C#", [2] = "DBA Iniciante" };

    private static RegrasMedalhas.Situacao Situacao(
        int aulas = 0, int provas = 0, int notasMaximas = 0,
        int[]? cursosConcluidos = null, int totalCursos = 3)
        => new(aulas, provas, notasMaximas,
               cursosConcluidos ?? Array.Empty<int>(), totalCursos, MedalhasDeCurso);

    [Fact]
    public void Quem_nao_fez_nada_nao_ganha_medalha()
        => Assert.Empty(RegrasMedalhas.Merecidas(Situacao()));

    [Fact]
    public void Primeira_aula_concluida_da_Primeiro_Passo()
        => Assert.Contains("Primeiro Passo", RegrasMedalhas.Merecidas(Situacao(aulas: 1)));

    [Fact]
    public void Dez_aulas_acumulam_as_medalhas_anteriores()
    {
        var medalhas = RegrasMedalhas.Merecidas(Situacao(aulas: 10));

        Assert.Contains("Primeiro Passo", medalhas);
        Assert.Contains("Estudante Dedicado", medalhas);
        Assert.DoesNotContain("Bibliotecário", medalhas);
    }

    [Fact]
    public void Prova_aprovada_da_Quiz_Champion_e_nota_maxima_da_Pontuacao_Perfeita()
    {
        var medalhas = RegrasMedalhas.Merecidas(Situacao(provas: 1, notasMaximas: 1));

        Assert.Contains("Quiz Champion", medalhas);
        Assert.Contains("Pontuação Perfeita", medalhas);
    }

    [Fact]
    public void Curso_concluido_da_a_medalha_daquele_curso()
    {
        var medalhas = RegrasMedalhas.Merecidas(Situacao(cursosConcluidos: new[] { 1 }));

        Assert.Contains("Mestre em C#", medalhas);
        Assert.DoesNotContain("DBA Iniciante", medalhas);
    }

    [Fact]
    public void Curso_sem_medalha_vinculada_nao_concede_medalha_de_curso()
    {
        var medalhas = RegrasMedalhas.Merecidas(Situacao(cursosConcluidos: new[] { 3 }));

        Assert.Empty(medalhas);
    }

    [Fact]
    public void Concluir_todos_os_cursos_da_a_Lenda()
    {
        var medalhas = RegrasMedalhas.Merecidas(Situacao(
            cursosConcluidos: new[] { 1, 2, 3 }, totalCursos: 3));

        Assert.Contains("Lenda da Tech Quest", medalhas);
        Assert.Contains("Mestre em C#", medalhas);
        Assert.Contains("DBA Iniciante", medalhas);
    }

    [Fact]
    public void Sem_curso_publicado_ninguem_vira_Lenda()
    {
        // Protege contra o caso degenerado: com zero cursos na plataforma,
        // zero concluidos satisfaria "concluiu todos".
        var medalhas = RegrasMedalhas.Merecidas(Situacao(totalCursos: 0));

        Assert.DoesNotContain("Lenda da Tech Quest", medalhas);
    }

    [Fact]
    public void Medalhas_sem_criterio_automatico_nunca_sao_concedidas()
    {
        // Dependem de dados que o modelo nao guarda; ficam para o tutor.
        var medalhas = RegrasMedalhas.Merecidas(Situacao(
            aulas: 100, provas: 50, notasMaximas: 50,
            cursosConcluidos: new[] { 1, 2, 3 }, totalCursos: 3));

        Assert.DoesNotContain("Mestre em Arrays", medalhas);
        Assert.DoesNotContain("Maratonista", medalhas);
        Assert.DoesNotContain("POO Master", medalhas);
        Assert.DoesNotContain("Arquiteto SOLID", medalhas);
    }
}
