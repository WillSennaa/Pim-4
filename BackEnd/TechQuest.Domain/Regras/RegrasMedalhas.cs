namespace TechQuest.Domain.Regras;

/// <summary>
/// Criterios de concessao das medalhas, avaliados apos cada acao do estudante.
///
/// As medalhas sao identificadas pelo NOME, porque a tabela Medalha do PIM III
/// nao tem coluna de criterio — so nome e raridade. Mapear por nome mantem a
/// regra em C# sem alterar o modelo entregue.
///
/// LIMITE CONHECIDO: quatro medalhas do PIM III ("Mestre em Arrays",
/// "Maratonista", "POO Master", "Arquiteto SOLID") dependem de criterios que
/// o modelo nao registra (assunto da aula, dias consecutivos de estudo). Ficam
/// como concessao manual pelo tutor ate existir onde guardar isso.
/// </summary>
public static class RegrasMedalhas
{
    public record Situacao(
        int MateriaisConcluidos,
        int ProvasAprovadas,
        int NotasMaximas,
        IReadOnlyCollection<string> CursosConcluidos,
        int TotalCursosPublicados);

    /// <summary>Nomes das medalhas que o estudante ja deveria possuir.</summary>
    public static IReadOnlyList<string> Merecidas(Situacao s)
    {
        var nomes = new List<string>();

        if (s.MateriaisConcluidos >= 1) nomes.Add("Primeiro Passo");
        if (s.MateriaisConcluidos >= 10) nomes.Add("Estudante Dedicado");
        if (s.MateriaisConcluidos >= 20) nomes.Add("Bibliotecário");
        if (s.ProvasAprovadas >= 1) nomes.Add("Quiz Champion");
        if (s.NotasMaximas >= 1) nomes.Add("Pontuação Perfeita");

        if (s.CursosConcluidos.Any(c => c.Contains("C#", StringComparison.OrdinalIgnoreCase)))
            nomes.Add("Mestre em C#");

        if (s.CursosConcluidos.Any(c => c.Contains("Banco de Dados", StringComparison.OrdinalIgnoreCase)))
            nomes.Add("DBA Iniciante");

        if (s.TotalCursosPublicados > 0 && s.CursosConcluidos.Count >= s.TotalCursosPublicados)
            nomes.Add("Lenda da Tech Quest");

        return nomes;
    }
}
