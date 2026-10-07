namespace TechQuest.Application.Dtos;

public record ResumoAdminDto(
    int TotalUsuarios, int UsuariosAtivos, int Estudantes, int Tutores,
    int CursosPublicados, int CursosPendentes, int TotalMatriculas,
    int CertificadosEmitidos, int ChamadosAbertos);

public record SolicitacaoCursoDto(
    int IdCurso, string Nome, string? Descricao, string? Categoria, string? Nivel,
    string? Tutor, string Status, int TotalAulas, bool TemProva, int TotalQuestoes);

public record AvaliarCursoRequest(string? Motivo);

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

