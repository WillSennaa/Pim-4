namespace TechQuest.Desktop.Modelos;

/// <summary>
/// Estados de um curso, com a grafia exata que a API usa
/// (FluxoPublicacaoCurso no Domain do servidor).
///
///   Rascunho --submeter--> Pendente --aprovar--> Publicado
///                              +----rejeitar--> Rejeitado --submeter--> Pendente
///
/// O desktop NAO reimplementa a maquina de estados: so o servidor decide se
/// uma transicao e valida. Estas constantes servem para filtrar listas e
/// para saber se a tela oferece os botoes de avaliacao.
/// </summary>
public static class StatusCurso
{
    public const string Rascunho = "Rascunho";
    public const string Pendente = "Pendente";
    public const string Publicado = "Publicado";
    public const string Rejeitado = "Rejeitado";

    public static bool EhPendente(string? status)
        => string.Equals(status, Pendente, StringComparison.OrdinalIgnoreCase);
}

/// <summary>Linha de GET /api/admin/cursos?status=: resumo para a fila.</summary>
public record SolicitacaoCurso(
    int IdCurso,
    string Nome,
    string? Descricao,
    string? Categoria,
    string? Nivel,
    string? Tutor,
    string Status,
    int TotalAulas,
    bool TemProva,
    int TotalQuestoes);

/// <summary>
/// GET /api/admin/cursos/{id}: o curso COMPLETO, com o texto dos materiais e
/// a prova COM gabarito. O avaliador precisa ler o que vai publicar.
/// </summary>
public record CursoRevisao(
    int IdCurso,
    string Nome,
    string? Descricao,
    string? Categoria,
    string? Nivel,
    int? DuracaoHoras,
    string? Tutor,
    string Status,
    IReadOnlyList<MaterialRevisao> Materiais,
    ProvaRevisao? Prova);

public record MaterialRevisao(int Id, string? Titulo, string? Tipo, string? Conteudo);

public record ProvaRevisao(
    int Id,
    string? Titulo,
    decimal NotaMinima,
    int TempoMinutos,
    IReadOnlyList<QuestaoRevisao> Questoes);

public record QuestaoRevisao(
    int Id,
    int Ordem,
    string Enunciado,
    string? CodigoExemplo,
    IReadOnlyList<AlternativaRevisao> Alternativas);

public record AlternativaRevisao(int Id, string? Letra, string? Texto, bool EhCorreta);

/// <summary>Corpo de POST /api/admin/cursos/{id}/rejeitar.</summary>
public record RejeicaoCurso(string? Motivo);
