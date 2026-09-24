using TechQuest.Domain.Entidades;
using Xunit;

namespace TechQuest.Tests.Entidades;

/// <summary>
/// As entidades nao sao sacos de propriedades: carregam comportamento, e o
/// comportamento e testavel sem banco nenhum.
/// </summary>
public class ComportamentoDasEntidadesTests
{
    [Theory]
    [InlineData("Felipe Almeida", "FA")]
    [InlineData("Ana Paula Souza", "AS")]   // primeira e ultima, nao as duas primeiras
    [InlineData("Ana", "A")]
    [InlineData("", "??")]
    public void Iniciais_do_avatar_sao_derivadas_do_nome(string nome, string esperado)
        => Assert.Equal(esperado, new Usuario { Nome = nome }.Iniciais());

    // O parametro vem como double porque atributo C# nao aceita decimal;
    // a conversao acontece dentro do teste.
    [Theory]
    [InlineData(7.0, true)]     // exatamente a nota minima aprova
    [InlineData(6.99, false)]
    [InlineData(10.0, true)]
    [InlineData(0.0, false)]
    public void Aprovacao_usa_a_nota_minima_da_propria_prova(double nota, bool esperado)
        => Assert.Equal(esperado, new Prova { NotaMinima = 7.0m }.Aprovado((decimal)nota));

    [Fact]
    public void Prova_com_nota_minima_diferente_muda_o_resultado()
    {
        var exigente = new Prova { NotaMinima = 9.0m };

        Assert.False(exigente.Aprovado(8.5m));
        Assert.True(exigente.Aprovado(9.0m));
    }

    [Theory]
    [InlineData("Publicado", true)]
    [InlineData("publicado", true)]
    [InlineData("Rascunho", false)]
    [InlineData("Pendente", false)]
    public void Curso_publicado_e_reconhecido_sem_diferenciar_caixa(string status, bool esperado)
        => Assert.Equal(esperado, new Curso { Status = status }.EstaPublicado());

    [Fact]
    public void Questao_encontra_a_propria_alternativa_correta()
    {
        var questao = new Questao { Enunciado = "Teste" };
        questao.Alternativas.Add(new Alternativa { Letra = "A", EhCorreta = false });
        questao.Alternativas.Add(new Alternativa { Letra = "B", EhCorreta = true });

        Assert.Equal("B", questao.AlternativaCorreta()?.Letra);
    }

    [Fact]
    public void Questao_sem_gabarito_devolve_nulo_em_vez_de_estourar()
    {
        var questao = new Questao { Enunciado = "Teste" };
        questao.Alternativas.Add(new Alternativa { Letra = "A", EhCorreta = false });

        Assert.Null(questao.AlternativaCorreta());
    }
}
