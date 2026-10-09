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
    /// <summary>
    /// Se ESTE aluno ja esta matriculado. Quem sabe disso e o servidor, que tem
    /// a tabela Historico; a tela nao pode deduzir sozinha, e era por isso que
    /// o botao "Matricular-me" aparecia para quem ja estava matriculado.
    /// Sem aluno autenticado vem false.
    /// </summary>
    bool Matriculado,
    IReadOnlyList<MaterialDto> Materiais);

public record MaterialDto(
    int Id,
    string? Titulo,
    string? Tipo,
    string? Conteudo,
    bool Concluido,
    int PorcentagemAssistida);
