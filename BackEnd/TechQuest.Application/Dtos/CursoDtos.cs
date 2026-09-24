namespace TechQuest.Application.Dtos;

public record CursoResumoDto(
    int Id,
    string Nome,
    string? Descricao,
    string? Categoria,
    string? Nivel,
    int? DuracaoHoras,
    string? Instrutor,
    string Status,
    bool TemProva);

public record CursoDetalheDto(
    int Id,
    string Nome,
    string? Descricao,
    string? Categoria,
    string? Nivel,
    int? DuracaoHoras,
    string? Instrutor,
    string? InstrutorIniciais,
    string Status,
    int? IdProva,
    IReadOnlyList<MaterialDto> Materiais);

public record MaterialDto(
    int Id,
    string? Titulo,
    string? Tipo,
    bool Concluido,
    int PorcentagemAssistida);
