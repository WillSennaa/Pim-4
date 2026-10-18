namespace TechQuest.Domain.Regras;

/// <summary>
/// Criterios de concessao das medalhas, avaliados apos cada acao do estudante.
///
/// Ha dois tipos de medalha automatica:
///
///   1. GERAIS, por contagem (materiais, provas, notas maximas, todos os
///      cursos). Sao identificadas pelo NOME, porque a tabela Medalha nao tem
///      coluna de criterio.
///
///   2. DE CURSO, uma por curso, concedida quando o estudante conclui aquele
///      curso. O vinculo e a coluna Medalha.ID_Curso.
///
/// POR QUE O VINCULO DO TIPO 2 E UMA CHAVE ESTRANGEIRA, E NAO O NOME
/// A versao anterior procurava "C#" ou "Banco de Dados" no nome do curso
/// concluido. Funcionava para dois cursos, mas nao escala: cada curso novo
/// exigiria mudar o codigo, e renomear um curso quebraria a concessao em
/// silencio. Com ID_Curso, a medalha aponta para o curso; criar um curso com
/// medalha passa a ser dado, nao codigo.
/// ALTERNATIVA REJEITADA: tabela Medalha_Curso (N:N). Nenhum caso pede uma
/// medalha para varios cursos, e a coluna anulavel resolve com uma FK so.
///
/// LIMITE CONHECIDO: quatro medalhas do PIM III ("Mestre em Arrays",
/// "Maratonista", "POO Master", "Arquiteto SOLID") dependem de criterios que
/// o modelo nao registra (assunto da aula, dias consecutivos de estudo). Ficam
/// como concessao manual pelo tutor ate existir onde guardar isso.
/// </summary>
public static class RegrasMedalhas
{
    /// <param name="CursosConcluidos">Ids dos cursos que o estudante concluiu.</param>
    /// <param name="MedalhasDeCurso">Id do curso -> nome da medalha daquele curso.</param>
    public record Situacao(
        int MateriaisConcluidos,
        int ProvasAprovadas,
        int NotasMaximas,
        IReadOnlyCollection<int> CursosConcluidos,
        int TotalCursosPublicados,
        IReadOnlyDictionary<int, string> MedalhasDeCurso);

    /// <summary>Nomes das medalhas que o estudante ja deveria possuir.</summary>
    public static IReadOnlyList<string> Merecidas(Situacao s)
    {
        var nomes = new List<string>();

        if (s.MateriaisConcluidos >= 1) nomes.Add("Primeiro Passo");
        if (s.MateriaisConcluidos >= 10) nomes.Add("Estudante Dedicado");
        if (s.MateriaisConcluidos >= 20) nomes.Add("Bibliotecário");
        if (s.ProvasAprovadas >= 1) nomes.Add("Quiz Champion");
        if (s.NotasMaximas >= 1) nomes.Add("Pontuação Perfeita");

        foreach (var idCurso in s.CursosConcluidos)
        {
            if (s.MedalhasDeCurso.TryGetValue(idCurso, out var medalha))
                nomes.Add(medalha);
        }

        if (s.TotalCursosPublicados > 0 && s.CursosConcluidos.Count >= s.TotalCursosPublicados)
            nomes.Add("Lenda da Tech Quest");

        return nomes;
    }
}
