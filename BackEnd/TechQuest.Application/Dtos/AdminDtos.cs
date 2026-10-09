namespace TechQuest.Application.Dtos;

public record ResumoAdminDto(
    int TotalUsuarios, int UsuariosAtivos, int Estudantes, int Tutores,
    int CursosPublicados, int CursosPendentes, int TotalMatriculas,
    int CertificadosEmitidos, int ChamadosAbertos);

public record SolicitacaoCursoDto(
    int IdCurso, string Nome, string? Descricao, string? Categoria, string? Nivel,
    string? Tutor, string Status, int TotalAulas, bool TemProva, int TotalQuestoes);

public record AvaliarCursoRequest(string? Motivo);

/// <summary>
/// Curso COMPLETO para o administrador avaliar antes de publicar.
///
/// POR QUE ELE PRECISA EXISTIR: a fila de aprovacao entregava apenas
/// contagens -- "2 aulas, 3 questoes" --, e com isso o administrador decidia
/// sem ler o conteudo. Aprovar passa a existir como ato de avaliacao, nao de
/// carimbo: publicar um curso o torna visivel a todos os estudantes.
///
/// REUSA MaterialTutorDto E ProvaCompletaTutorDto de proposito. Sao
/// exatamente a mesma informacao que o autor ve ao revisar o proprio
/// trabalho: material com conteudo, e prova COM gabarito. Criar um par de
/// DTOs identicos so para trocar o nome duplicaria manutencao.
///
/// O gabarito aqui e legitimo: sem ele nao da para avaliar se a prova esta
/// correta -- que e justamente o que se pede ao avaliador. O endpoint exige
/// o papel Admin; o estudante continua recebendo a prova sem resposta.
/// </summary>
public record CursoRevisaoDto(
    int IdCurso, string Nome, string? Descricao, string? Categoria, string? Nivel,
    int? DuracaoHoras, string? Tutor, string Status,
    IReadOnlyList<MaterialTutorDto> Materiais,
    ProvaCompletaTutorDto? Prova);

public record UsuarioAdminDto(
    int Id, string Nome, string? Email, string Papel, bool Ativo, DateTime DataCadastro);

public record CriarUsuarioRequest(string Nome, string Email, string Senha, string Papel);

public record AlterarStatusRequest(bool Ativo);

public record LogAuditoriaDto(int Id, int IdAdm, string? Administrador, string? Acao, DateTime Data);

/// <summary>
/// Medalha como catalogo gerenciavel.
///
/// A tabela Medalha existia desde o PIM III mas so era LIDA: as doze medalhas
/// vinham do script de carga e nao havia como criar outras. Com a plataforma
/// recebendo cursos novos, o catalogo precisa acompanhar.
/// </summary>
public record MedalhaAdminDto(
    int Id, string Nome, string? Raridade, string? Descricao, int TotalConquistas);

public record SalvarMedalhaRequest(string Nome, string? Raridade, string? Descricao);

