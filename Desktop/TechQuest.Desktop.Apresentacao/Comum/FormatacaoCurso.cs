using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Apresentacao.Comum;

/// <summary>
/// Textos de curso usados em mais de uma tela (fila de Aprovacoes e lista de
/// Cursos). Num lugar so, para "1 aula" e "sem prova" sairem iguais nas duas.
/// </summary>
public static class FormatacaoCurso
{
    public static string Aulas(int n) => n == 1 ? "1 aula" : $"{n} aulas";

    public static string Prova(bool temProva, int questoes)
        => !temProva ? "sem prova" : questoes == 1 ? "1 questão" : $"{questoes} questões";

    /// <summary>Sem aula, sem prova ou prova vazia: o curso nao se sustenta publicado.</summary>
    public static bool Incompleto(SolicitacaoCurso c)
        => c.TotalAulas == 0 || !c.TemProva || c.TotalQuestoes == 0;

    /// <summary>Ordem de exibicao: o que pede acao do admin primeiro.</summary>
    public static int OrdemDoStatus(string? status) => status switch
    {
        StatusCurso.Pendente => 0,
        StatusCurso.Publicado => 1,
        StatusCurso.Rejeitado => 2,
        StatusCurso.Rascunho => 3,
        _ => 4
    };
}

/// <summary>Opcao de um filtro em lista suspensa: valor interno + rotulo com contagem.</summary>
public sealed record OpcaoFiltro(string? Valor, string Rotulo);
