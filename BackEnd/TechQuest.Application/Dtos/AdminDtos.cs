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
