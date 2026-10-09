using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Aplicacao.Interfaces;

/// <summary>Gestao de contas pelo administrador (/api/admin/usuarios). RF01.</summary>
public interface IUsuariosGateway
{
    Task<IReadOnlyList<UsuarioAdmin>> ListarAsync(CancellationToken ct = default);

    /// <exception cref="Erros.OperacaoRecusadaException">E-mail ja cadastrado, papel invalido.</exception>
    Task<UsuarioAdmin> CriarAsync(NovoUsuario novo, CancellationToken ct = default);

    Task AlterarStatusAsync(int idUsuario, bool ativo, CancellationToken ct = default);
}

/// <summary>Registros de auditoria (/api/admin/logs). RF11.</summary>
public interface IAuditoriaGateway
{
    Task<IReadOnlyList<LogAuditoria>> ListarAsync(int limite, CancellationToken ct = default);
}
