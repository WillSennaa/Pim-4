using TechQuest.Domain.Regras;
using Xunit;

namespace TechQuest.Tests.Regras;

public class RegrasGamificacaoTests
{
    [Fact]
    public void Estudante_sem_atividade_comeca_zerado_no_nivel_um()
    {
        var xp = RegrasGamificacao.CalcularXp(0, 0, 0, 0);

        Assert.Equal(0, xp);
        Assert.Equal(1, RegrasGamificacao.CalcularNivel(xp));
        Assert.Equal(0, RegrasGamificacao.ProgressoNoNivel(xp));
    }

    [Fact]
    public void Xp_soma_aulas_provas_bonus_e_medalhas()
    {
        // 2 aulas (100) + 1 prova aprovada (100) + 1 nota maxima (50) + 1 medalha (75)
        var xp = RegrasGamificacao.CalcularXp(
            materiaisConcluidos: 2, provasAprovadas: 1, provasComNotaMaxima: 1, medalhasConquistadas: 1);

        Assert.Equal(325, xp);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(349, 1)]
    [InlineData(350, 2)]   // limite exato do nivel
    [InlineData(699, 2)]
    [InlineData(700, 3)]
    public void Nivel_sobe_a_cada_350_de_xp(int xp, int nivelEsperado)
        => Assert.Equal(nivelEsperado, RegrasGamificacao.CalcularNivel(xp));

    [Theory]
    [InlineData(0, 350)]
    [InlineData(100, 350)]
    [InlineData(350, 700)]
    public void Proximo_nivel_e_sempre_o_limite_seguinte(int xp, int esperado)
        => Assert.Equal(esperado, RegrasGamificacao.XpDoProximoNivel(xp));

    [Theory]
    [InlineData(0, 0)]
    [InlineData(175, 50)]    // metade do caminho
    [InlineData(350, 0)]     // acabou de subir de nivel, recomeca do zero
    [InlineData(525, 50)]
    public void Progresso_no_nivel_e_percentual_dentro_da_faixa(int xp, int esperado)
        => Assert.Equal(esperado, RegrasGamificacao.ProgressoNoNivel(xp));

    [Fact]
    public void Progresso_no_nivel_nunca_passa_de_cem()
    {
        for (var xp = 0; xp <= 2000; xp += 7)
        {
            var progresso = RegrasGamificacao.ProgressoNoNivel(xp);
            Assert.InRange(progresso, 0, 100);
        }
    }
}
