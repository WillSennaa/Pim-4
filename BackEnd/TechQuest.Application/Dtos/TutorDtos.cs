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
/// <summary>
/// IdEstudante e IdUsuario sao coisas diferentes e os dois precisam vir: o
/// primeiro identifica a MATRICULA (usado para progresso, notas e medalhas),
/// o segundo identifica a PESSOA (usado para enderecar um chamado). Mandar so
/// um dos dois obriga a tela a adivinhar o outro.
/// </summary>
public record AlunoDoTutorDto(
    int IdEstudante, int IdUsuario, string Nome, string? Email,
    int IdCurso, string Curso, string? Status,
    int AulasConcluidas, int TotalAulas, int PercentualCurso,
    decimal? MelhorNota, int Tentativas);

/// <summary>
/// Prova como o TUTOR a ve: com o gabarito.
///
/// Existe separada do ProvaDto porque sao publicos diferentes. O aluno recebe
/// a prova sem a resposta correta -- esse e o ponto da correcao no servidor.
/// O tutor escreveu as questoes e precisa conferir o gabarito antes de
/// submeter o curso; esconder dele seria esconder o proprio trabalho.
/// </summary>
public record ProvaCompletaTutorDto(
    int Id, string? Titulo, decimal NotaMinima, int TempoMinutos,
    IReadOnlyList<QuestaoCompletaTutorDto> Questoes);

public record QuestaoCompletaTutorDto(
    int Id, int Ordem, string Enunciado, string? CodigoExemplo,
    IReadOnlyList<AlternativaCompletaDto> Alternativas);

public record AlternativaCompletaDto(int Id, string? Letra, string? Texto, bool EhCorreta);

