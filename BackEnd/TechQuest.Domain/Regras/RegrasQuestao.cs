namespace TechQuest.Domain.Regras;

/// <summary>
/// Validacao de uma questao de prova.
///
/// Estava dentro do TutorService, onde so podia ser testada subindo servico,
/// repositorios e banco. Movida para o dominio, recebe tipos primitivos e e
/// testavel isolada — que e exatamente o motivo de existir uma camada de
/// dominio sem dependencias.
/// </summary>
public static class RegrasQuestao
{
    public record AlternativaParaValidar(string? Letra, bool EhCorreta);

    /// <summary>Devolve a mensagem do erro, ou null se a questao for valida.</summary>
    public static string? Validar(string? enunciado, IReadOnlyList<AlternativaParaValidar> alternativas)
    {
        if (string.IsNullOrWhiteSpace(enunciado))
            return "O enunciado e obrigatorio.";

        if (alternativas is null || alternativas.Count < 2)
            return "Informe ao menos duas alternativas.";

        if (alternativas.Any(a => string.IsNullOrWhiteSpace(a.Letra)))
            return "Toda alternativa precisa de uma letra.";

        // Sem exatamente uma correta, a correcao da prova quebra no meio do
        // caminho — e ai o estrago ja aconteceu com o aluno.
        var corretas = alternativas.Count(a => a.EhCorreta);
        if (corretas == 0) return "A questao precisa ter uma alternativa correta.";
        if (corretas > 1) return "A questao precisa ter exatamente uma alternativa correta.";

        var letras = alternativas.Select(a => a.Letra!.Trim().ToUpperInvariant()).ToList();
        if (letras.Distinct().Count() != letras.Count)
            return "Ha letras repetidas nas alternativas.";

        return null;
    }
}
