using TechQuest.Domain.Regras;
using Xunit;

namespace TechQuest.Tests.Regras;

/// <summary>
/// O CRITERIO de conclusao, isolado de banco e de orquestracao.
///
/// Estes testes cobrem "a regra esta certa". Eles NAO cobrem "a regra e
/// aplicada na hora certa" -- essa garantia esta em
/// ConclusaoCursoServiceTests, e foi a que faltava quando o certificado
/// deixou de ser emitido.
/// </summary>
public class RegrasConclusaoCursoTests
{
    [Theory]
    // total, concluidos, temProva, provaAprovada, esperado
    [InlineData(3, 3, true,  true,  true)]   // tudo pronto
    [InlineData(3, 3, true,  false, false)]  // materiais ok, prova pendente
    [InlineData(3, 2, true,  true,  false)]  // aprovado, mas falta material
    [InlineData(3, 0, true,  true,  false)]  // aprovado sem estudar nada
    [InlineData(3, 3, false, false, true)]   // curso sem prova: materiais bastam
    [InlineData(1, 1, false, false, true)]   // curso de um material so
    public void Criterio_exige_todos_os_materiais_e_a_prova_quando_ela_existe(
        int total, int concluidos, bool temProva, bool provaAprovada, bool esperado)
    {
        Assert.Equal(esperado,
            RegrasConclusaoCurso.CursoConcluido(total, concluidos, temProva, provaAprovada));
    }

    /// <summary>
    /// Curso vazio nunca conclui. Sem esta guarda, 0 >= 0 seria verdadeiro e
    /// um curso sem material emitiria certificado -- certificando o que nao
    /// houve. E o caso que uma comparacao ingenua erra.
    /// </summary>
    [Theory]
    [InlineData(0, 0, false, false)]
    [InlineData(0, 0, true,  true)]
    [InlineData(0, 5, true,  true)]
    public void Curso_sem_material_nunca_conclui(
        int total, int concluidos, bool temProva, bool provaAprovada)
    {
        Assert.False(
            RegrasConclusaoCurso.CursoConcluido(total, concluidos, temProva, provaAprovada));
    }

    /// <summary>
    /// Mais concluidos que o total nao deveria acontecer, mas se acontecer a
    /// regra nao pode travar o aluno: progresso acima do total ainda e
    /// progresso completo.
    /// </summary>
    [Fact]
    public void Concluidos_acima_do_total_ainda_conta_como_completo()
    {
        Assert.True(RegrasConclusaoCurso.CursoConcluido(3, 5, false, false));
    }

    [Theory]
    [InlineData(0, 0, 0)]      // curso vazio e 0%, nunca 100%
    [InlineData(4, 0, 0)]
    [InlineData(4, 1, 25)]
    [InlineData(3, 2, 67)]     // arredonda 66,67
    [InlineData(4, 4, 100)]
    [InlineData(4, 9, 100)]    // nao passa de 100
    [InlineData(4, -2, 0)]     // nem abaixo de 0
    public void Percentual_fica_entre_zero_e_cem(int total, int concluidos, int esperado)
    {
        Assert.Equal(esperado, RegrasConclusaoCurso.PercentualConcluido(total, concluidos));
    }
}
