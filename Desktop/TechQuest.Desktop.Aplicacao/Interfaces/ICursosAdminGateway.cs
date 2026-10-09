using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Aplicacao.Interfaces;

/// <summary>Avaliacao e consulta de cursos pelo administrador (/api/admin/cursos).</summary>
public interface ICursosAdminGateway
{
    Task<IReadOnlyList<SolicitacaoCurso>> ListarAsync(string status, CancellationToken ct = default);

    /// <exception cref="Erros.RegistroNaoEncontradoException">Curso inexistente.</exception>
    Task<CursoRevisao> ObterParaRevisaoAsync(int idCurso, CancellationToken ct = default);

    /// <exception cref="Erros.OperacaoRecusadaException">
    /// O curso nao esta mais Pendente (outro administrador ja o avaliou).
    /// </exception>
    Task AprovarAsync(int idCurso, CancellationToken ct = default);

    Task RejeitarAsync(int idCurso, string motivo, CancellationToken ct = default);
}
