namespace TechQuest.Application.Dtos;

public record ProvaDto(
    int Id,
    string? Titulo,
    decimal NotaMinima,
    int TempoMinutos,
    int TotalQuestoes,
    IReadOnlyList<QuestaoDto> Questoes);

/// <summary>
/// Nao existe campo de gabarito neste DTO nem no AlternativaDto. A correcao
/// acontece no servidor; o cliente nunca recebe a resposta correta.
/// </summary>
public record QuestaoDto(
    int Id,
    int Ordem,
    string Enunciado,
    string? CodigoExemplo,
    IReadOnlyList<AlternativaDto> Alternativas);

public record AlternativaDto(
    int Id,
    string? Letra,
    string? Texto);

public record RespostaSubmetidaDto(int IdQuestao, string Letra);

public record TentativaRequest(IReadOnlyList<RespostaSubmetidaDto> Respostas);

public record ResultadoTentativaDto(
    int IdProva,
    int Acertos,
    int TotalQuestoes,
    decimal Nota,
    decimal NotaMinima,
    bool Aprovado,
    int NumeroTentativa,
    int XpGanho,
    IReadOnlyList<string> MedalhasNovas,
    IReadOnlyList<CorrecaoQuestaoDto> Correcao);

/// <summary>
/// O gabarito e revelado APENAS na resposta da submissao, ja que a prova
/// terminou. Isso alimenta as telas de resultado do PIM III.
/// </summary>
public record CorrecaoQuestaoDto(
    int IdQuestao,
    int Ordem,
    string? LetraMarcada,
    string? LetraCorreta,
    bool Acertou);
