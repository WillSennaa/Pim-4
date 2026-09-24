using TechQuest.Domain.Regras;
using Xunit;

namespace TechQuest.Tests.Regras;

public class RegrasQuestaoTests
{
    private static List<RegrasQuestao.AlternativaParaValidar> Alternativas(
        params (string letra, bool correta)[] itens)
        => itens.Select(i => new RegrasQuestao.AlternativaParaValidar(i.letra, i.correta)).ToList();

    [Fact]
    public void Questao_bem_formada_passa()
    {
        var erro = RegrasQuestao.Validar(
            "Qual palavra-chave declara uma constante em C#?",
            Alternativas(("A", false), ("B", true), ("C", false), ("D", false)));

        Assert.Null(erro);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Enunciado_vazio_e_recusado(string? enunciado)
        => Assert.NotNull(RegrasQuestao.Validar(enunciado, Alternativas(("A", true), ("B", false))));

    [Fact]
    public void Uma_alternativa_so_nao_e_questao()
        => Assert.NotNull(RegrasQuestao.Validar("Enunciado", Alternativas(("A", true))));

    [Fact]
    public void Questao_sem_gabarito_e_recusada()
        => Assert.NotNull(RegrasQuestao.Validar("Enunciado", Alternativas(("A", false), ("B", false))));

    [Fact]
    public void Questao_com_dois_gabaritos_e_recusada()
    {
        // Este e o caso perigoso: passaria batido e so quebraria na hora em
        // que um aluno estivesse fazendo a prova.
        var erro = RegrasQuestao.Validar("Enunciado", Alternativas(("A", true), ("B", true)));

        Assert.NotNull(erro);
    }

    [Fact]
    public void Letras_repetidas_sao_recusadas()
        => Assert.NotNull(RegrasQuestao.Validar(
            "Enunciado", Alternativas(("A", true), ("a", false), ("C", false))));

    [Fact]
    public void Alternativa_sem_letra_e_recusada()
        => Assert.NotNull(RegrasQuestao.Validar(
            "Enunciado", Alternativas((" ", true), ("B", false))));
}
