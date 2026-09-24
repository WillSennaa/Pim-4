namespace TechQuest.Application.Dtos;

/// <summary>Linha devolvida pela procedure SP_RelatorioDesempenhoEstudante.</summary>
public record LinhaRelatorioDesempenhoDto(
    string Estudante,
    string? Prova,
    decimal? Nota,
    int Tentativas,
    DateTime DataRealizacao);
