using TechQuest.Application.Dtos;
using TechQuest.Domain.Entidades;

namespace TechQuest.Application.Interfaces;

/// <summary>
/// Contratos de persistencia declarados na camada Application e implementados
/// na Infrastructure: a regra de negocio depende da abstracao, nunca do EF Core
/// (inversao de dependencia).
///
/// Os metodos de escrita NAO gravam: quem grava e a IUnidadeDeTrabalho,
/// chamada pelo servico que conhece o limite da operacao.
/// </summary>
public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct = default);
    Task<Usuario?> ObterPorIdAsync(int idUsuario, CancellationToken ct = default);

    /// <summary>Com rastreamento do EF: usado quando o objeto sera alterado.</summary>
    Task<Usuario?> ObterParaEdicaoAsync(int idUsuario, CancellationToken ct = default);

    Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken ct = default);
    Task<bool> EmailExisteAsync(string email, CancellationToken ct = default);
    void Adicionar(Usuario usuario);

    // Sobrecargas para a especializacao, definida pelo papel escolhido.
    void Adicionar(Adm adm);
    void Adicionar(Tutor tutor);
    void Adicionar(Estudante estudante);
}

public interface ICursoRepository
{
    Task<IReadOnlyList<Curso>> ListarPublicadosAsync(CancellationToken ct = default);
    Task<Curso?> ObterComMateriaisAsync(int idCurso, CancellationToken ct = default);
    Task<Material?> ObterMaterialAsync(int idMaterial, CancellationToken ct = default);
    Task<int> ContarPublicadosAsync(CancellationToken ct = default);

    // --- escrita e gestao (tutor / admin) ---
    Task<Curso?> ObterParaEdicaoAsync(int idCurso, CancellationToken ct = default);
    Task<IReadOnlyList<Curso>> ListarPorTutorAsync(int idTutor, CancellationToken ct = default);
    Task<IReadOnlyList<Curso>> ListarPorStatusAsync(string status, CancellationToken ct = default);
    Task<int> ContarMatriculadosAsync(int idCurso, CancellationToken ct = default);
    Task<int> ContarQuestoesAsync(int idProva, CancellationToken ct = default);
    void Adicionar(Curso curso);
    void AdicionarMaterial(Material material);
    void RemoverMaterial(Material material);
    Task<Material?> ObterMaterialParaEdicaoAsync(int idMaterial, CancellationToken ct = default);
    Task<bool> MaterialTemProgressoAsync(int idMaterial, CancellationToken ct = default);
}

public interface IProvaRepository
{
    Task<Prova?> ObterComQuestoesAsync(int idProva, CancellationToken ct = default);
    Task<Prova?> ObterParaEdicaoAsync(int idProva, CancellationToken ct = default);
    void Adicionar(Prova prova);
    void AdicionarQuestao(Questao questao);
    Task<Questao?> ObterQuestaoAsync(int idQuestao, CancellationToken ct = default);
    void RemoverQuestao(Questao questao);
}

public interface IDesempenhoRepository
{
    Task<int> ContarTentativasAsync(int idEstudante, int idProva, CancellationToken ct = default);
    void Registrar(Desempenho desempenho);
    Task<bool> AprovadoNaProvaAsync(int idEstudante, int idProva, CancellationToken ct = default);
}

public interface IGamificacaoRepository
{
    Task<(int MateriaisConcluidos, int ProvasAprovadas, int NotasMaximas, int Medalhas)>
        ObterContagensAsync(int idEstudante, CancellationToken ct = default);
}

public interface IProgressoRepository
{
    Task<IReadOnlyDictionary<int, Progresso>> ObterPorCursoAsync(
        int idEstudante, int idCurso, CancellationToken ct = default);

    Task<Progresso?> ObterAsync(int idEstudante, int idMaterial, CancellationToken ct = default);
    void Adicionar(Progresso progresso);

    Task<(int Total, int Concluidas)> ContarAulasDoCursoAsync(
        int idEstudante, int idCurso, CancellationToken ct = default);
}

public interface IHistoricoRepository
{
    Task<Historico?> ObterAsync(int idEstudante, int idCurso, CancellationToken ct = default);
    Task<IReadOnlyList<Historico>> ListarPorEstudanteAsync(int idEstudante, CancellationToken ct = default);
    Task<IReadOnlyList<string>> NomesDeCursosConcluidosAsync(int idEstudante, CancellationToken ct = default);
    void Adicionar(Historico historico);
}

public interface ICertificadoRepository
{
    Task<IReadOnlyList<Certificado>> ListarPorEstudanteAsync(int idEstudante, CancellationToken ct = default);
    Task<Certificado?> ObterPorCodigoAsync(Guid codigo, CancellationToken ct = default);
    void Adicionar(Certificado certificado);
}

public interface IConquistaRepository
{
    Task<IReadOnlyList<Medalha>> ListarMedalhasAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Conquista>> ListarDoEstudanteAsync(int idEstudante, CancellationToken ct = default);
    Task<IReadOnlyList<Medalha>> ObterMedalhasPorNomeAsync(
        IEnumerable<string> nomes, CancellationToken ct = default);
    Task<Medalha?> ObterMedalhaAsync(int idMedalha, CancellationToken ct = default);
    void Adicionar(Conquista conquista);
}

public interface IChamadoRepository
{
    Task<IReadOnlyList<Chamado>> ListarDoUsuarioAsync(int idUsuario, CancellationToken ct = default);
    Task<Chamado?> ObterAsync(int idChamado, CancellationToken ct = default);
    void Adicionar(Chamado chamado);
    Task<int> ContarAbertosAsync(CancellationToken ct = default);
}

public interface IRelatorioRepository
{
    /// <summary>Executa a procedure SP_RelatorioDesempenhoEstudante.</summary>
    Task<IReadOnlyList<LinhaRelatorioDesempenhoDto>> DesempenhoDoEstudanteAsync(
        int idEstudante, CancellationToken ct = default);
}

/// <summary>Consultas de acompanhamento dos alunos de um tutor.</summary>
public interface ITutorRepository
{
    Task<IReadOnlyList<AlunoDoTutorDto>> ListarAlunosAsync(
        int idTutor, int? idCurso, CancellationToken ct = default);

    Task<bool> EstudanteExisteAsync(int idEstudante, CancellationToken ct = default);
}

public interface IAdminRepository
{
    Task<ResumoAdminDto> ResumoAsync(CancellationToken ct = default);
    Task<IReadOnlyList<LogAuditoria>> ListarLogsAsync(int limite, CancellationToken ct = default);
    void RegistrarLog(LogAuditoria log);
}
