namespace TechQuest.Application.Dtos;

// ----- Progresso em aula -----
public record AtualizarProgressoRequest(bool Concluido, int PorcentagemAssistida);

public record ProgressoAtualizadoDto(
    int IdMaterial,
    bool Concluido,
    int PorcentagemAssistida,
    int AulasConcluidas,
    int TotalAulas,
    int PercentualCurso,
    bool CursoConcluido,
    int XpAtual,
    int Nivel,
    IReadOnlyList<string> MedalhasNovas);

// ----- Matricula / historico -----
public record MatriculaDto(int IdHistorico, int IdCurso, string Curso, string Status);

public record ItemHistoricoDto(
    int IdHistorico,
    int IdCurso,
    string Curso,
    string? Status,
    DateOnly? DataConclusao,
    int AulasConcluidas,
    int TotalAulas,
    int PercentualCurso,
    Guid? CodigoCertificado);

// ----- Perfil -----
public record PerfilDto(
    int Id,
    string Nome,
    string? Email,
    string Iniciais,
    string Papel,
    string? Telefone,
    DateOnly? DataNascimento,
    string? Cidade,
    DateTime DataCadastro,
    int Xp,
    int Nivel,
    int XpProximoNivel,
    int ProgressoNivel,
    int CursosConcluidos,
    int AulasConcluidas,
    int ProvasAprovadas,
    int Medalhas);

public record AtualizarPerfilRequest(string? Telefone, DateOnly? DataNascimento, string? Cidade);

// ----- Conquistas -----
public record MedalhaDto(
    int Id,
    string Nome,
    string? Descricao,
    string? Raridade,
    bool Conquistada,
    DateTime? DataConquista);

// ----- Certificados -----
public record CertificadoDto(
    int Id,
    Guid CodigoAutenticacao,
    int IdCurso,
    string Curso,
    string Estudante,
    DateTime DataEmissao,
    DateOnly? DataConclusao,
    int? CargaHoraria);
