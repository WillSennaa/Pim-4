namespace TechQuest.Application.Dtos;

// ----- Cursos do tutor -----
public record CriarCursoRequest(
    string Nome, string? Descricao, string? Categoria, string? Nivel, int? DuracaoHoras);

public record CursoTutorDto(
    int Id, string Nome, string? Descricao, string? Categoria, string? Nivel,
    int? DuracaoHoras, string Status, int TotalAulas, int TotalMatriculados,
    int? IdProva, int TotalQuestoes, bool PodeEditar);

// ----- Aulas -----
public record MaterialRequest(string Titulo, string Tipo);
public record MaterialTutorDto(int Id, string? Titulo, string? Tipo);

// ----- Prova e questoes -----
public record CriarProvaRequest(string Titulo, decimal NotaMinima, int TempoMinutos);

public record ProvaTutorDto(
    int Id, string? Titulo, decimal NotaMinima, int TempoMinutos, int TotalQuestoes);

public record AlternativaRequest(string Letra, string Texto, bool EhCorreta);

public record CriarQuestaoRequest(
    string Enunciado, string? CodigoExemplo, int Ordem,
    IReadOnlyList<AlternativaRequest> Alternativas);

/// <summary>O tutor ve a letra correta: ele e o autor da questao.</summary>
public record QuestaoTutorDto(
    int Id, int Ordem, string Enunciado, string? CodigoExemplo,
    string? LetraCorreta, int TotalAlternativas);

// ----- Acompanhamento de alunos -----
public record AlunoDoTutorDto(
    int IdEstudante, string Nome, string? Email,
    int IdCurso, string Curso, string? Status,
    int AulasConcluidas, int TotalAulas, int PercentualCurso,
    decimal? MelhorNota, int Tentativas);
