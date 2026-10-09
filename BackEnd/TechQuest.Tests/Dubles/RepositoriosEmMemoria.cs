using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;

namespace TechQuest.Tests.Dubles;

/// <summary>
/// Dubles de teste escritos a mao, sem biblioteca de mock.
///
/// POR QUE A MAO, e nao Moq ou NSubstitute: sao cinco interfaces e um punhado
/// de metodos. Uma biblioteca de mock acrescentaria uma dependencia, uma
/// sintaxe propria para quem for ler o codigo na banca, e nada que estas
/// classes nao facam. Para uma suite maior a conta inverteria.
///
/// POR QUE SEM BANCO: estes dubles implementam as INTERFACES declaradas na
/// camada Application. O projeto de testes nao referencia Infrastructure,
/// nao conhece EF Core e nao abre conexao. Isso e consequencia direta da
/// inversao de dependencia -- se fosse preciso subir SQL Server para testar
/// a emissao de um certificado, a arquitetura em camadas nao estaria
/// cumprindo o que promete.
///
/// Metodo nao implementado lanca NotSupportedException com mensagem clara,
/// em vez de devolver nulo em silencio: se um teste futuro tocar nele, o
/// erro diz exatamente o que falta.
/// </summary>
public class CursoRepositorioFalso : ICursoRepository
{
    public readonly List<Curso> Cursos = new();

    public Task<Curso?> ObterComMateriaisAsync(int idCurso, CancellationToken ct = default)
        => Task.FromResult(Cursos.FirstOrDefault(c => c.IdCurso == idCurso));

    public Task<int?> ObterIdCursoPorProvaAsync(int idProva, CancellationToken ct = default)
        => Task.FromResult(Cursos.FirstOrDefault(c => c.IdProva == idProva)?.IdCurso);

    // ---- nao usados por estes testes ----
    private static T NaoUsado<T>(string metodo)
        => throw new NotSupportedException(
            $"{metodo} nao e usado pelos testes de conclusao de curso. " +
            "Se um teste novo precisar dele, implemente aqui.");

    public Task<IReadOnlyList<Curso>> ListarPublicadosAsync(CancellationToken ct = default)
        => NaoUsado<Task<IReadOnlyList<Curso>>>(nameof(ListarPublicadosAsync));
    public Task<Material?> ObterMaterialAsync(int idMaterial, CancellationToken ct = default)
        => NaoUsado<Task<Material?>>(nameof(ObterMaterialAsync));
    public Task<int> ContarPublicadosAsync(CancellationToken ct = default)
        => NaoUsado<Task<int>>(nameof(ContarPublicadosAsync));
    public Task<Curso?> ObterParaEdicaoAsync(int idCurso, CancellationToken ct = default)
        => NaoUsado<Task<Curso?>>(nameof(ObterParaEdicaoAsync));
    public Task<IReadOnlyList<Curso>> ListarPorTutorAsync(int idTutor, CancellationToken ct = default)
        => NaoUsado<Task<IReadOnlyList<Curso>>>(nameof(ListarPorTutorAsync));
    public Task<IReadOnlyList<Curso>> ListarPorStatusAsync(string status, CancellationToken ct = default)
        => NaoUsado<Task<IReadOnlyList<Curso>>>(nameof(ListarPorStatusAsync));
    public Task<int> ContarMatriculadosAsync(int idCurso, CancellationToken ct = default)
        => NaoUsado<Task<int>>(nameof(ContarMatriculadosAsync));
    public Task<int> ContarQuestoesAsync(int idProva, CancellationToken ct = default)
        => NaoUsado<Task<int>>(nameof(ContarQuestoesAsync));
    public void Adicionar(Curso curso) => NaoUsado<object>(nameof(Adicionar));
    public void AdicionarMaterial(Material material) => NaoUsado<object>(nameof(AdicionarMaterial));
    public void RemoverMaterial(Material material) => NaoUsado<object>(nameof(RemoverMaterial));
    public Task<Material?> ObterMaterialParaEdicaoAsync(int idMaterial, CancellationToken ct = default)
        => NaoUsado<Task<Material?>>(nameof(ObterMaterialParaEdicaoAsync));
    public Task<bool> MaterialTemProgressoAsync(int idMaterial, CancellationToken ct = default)
        => NaoUsado<Task<bool>>(nameof(MaterialTemProgressoAsync));
    public Task<int?> ObterUsuarioDoTutorDoCursoAsync(int idCurso, CancellationToken ct = default)
        => NaoUsado<Task<int?>>(nameof(ObterUsuarioDoTutorDoCursoAsync));
}

public class ProgressoRepositorioFalso : IProgressoRepository
{
    /// <summary>(idEstudante, idCurso) -> (total de materiais, concluidos).</summary>
    public readonly Dictionary<(int, int), (int Total, int Concluidas)> Contagens = new();

    public Task<(int Total, int Concluidas)> ContarAulasDoCursoAsync(
        int idEstudante, int idCurso, CancellationToken ct = default)
        => Task.FromResult(Contagens.TryGetValue((idEstudante, idCurso), out var v) ? v : (0, 0));

    public Task<IReadOnlyDictionary<int, Progresso>> ObterPorCursoAsync(
        int idEstudante, int idCurso, CancellationToken ct = default)
        => throw new NotSupportedException("ObterPorCursoAsync nao e usado por estes testes.");
    public Task<Progresso?> ObterAsync(int idEstudante, int idMaterial, CancellationToken ct = default)
        => throw new NotSupportedException("ObterAsync nao e usado por estes testes.");
    public void Adicionar(Progresso progresso)
        => throw new NotSupportedException("Adicionar nao e usado por estes testes.");
}

public class DesempenhoRepositorioFalso : IDesempenhoRepository
{
    /// <summary>(idEstudante, idProva) presentes aqui estao aprovados.</summary>
    public readonly HashSet<(int, int)> Aprovacoes = new();

    public Task<bool> AprovadoNaProvaAsync(int idEstudante, int idProva, CancellationToken ct = default)
        => Task.FromResult(Aprovacoes.Contains((idEstudante, idProva)));

    public Task<int> ContarTentativasAsync(int idEstudante, int idProva, CancellationToken ct = default)
        => Task.FromResult(0);
    public void Registrar(Desempenho desempenho) { /* irrelevante aqui */ }
}

public class HistoricoRepositorioFalso : IHistoricoRepository
{
    public readonly List<Historico> Historicos = new();
    private int _proximoId = 1;

    public Task<Historico?> ObterAsync(int idEstudante, int idCurso, CancellationToken ct = default)
        => Task.FromResult(Historicos.FirstOrDefault(
            h => h.IdEstudante == idEstudante && h.IdCurso == idCurso));

    public void Adicionar(Historico historico)
    {
        // O banco atribui o id no INSERT; aqui o duble faz o mesmo, porque o
        // Certificado precisa de um IdHistorico valido para apontar.
        historico.IdHistorico = _proximoId++;
        Historicos.Add(historico);
    }

    public Task<IReadOnlyList<Historico>> ListarPorEstudanteAsync(
        int idEstudante, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Historico>>(
            Historicos.Where(h => h.IdEstudante == idEstudante).ToList());

    public Task<IReadOnlyList<string>> NomesDeCursosConcluidosAsync(
        int idEstudante, CancellationToken ct = default)
        => throw new NotSupportedException("NomesDeCursosConcluidosAsync nao e usado por estes testes.");
}

public class CertificadoRepositorioFalso : ICertificadoRepository
{
    public readonly List<Certificado> Emitidos = new();

    public void Adicionar(Certificado certificado) => Emitidos.Add(certificado);

    public Task<IReadOnlyList<Certificado>> ListarPorEstudanteAsync(
        int idEstudante, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Certificado>>(Emitidos);

    public Task<Certificado?> ObterPorCodigoAsync(Guid codigo, CancellationToken ct = default)
        => Task.FromResult(Emitidos.FirstOrDefault(c => c.CodigoAutenticacao == codigo));
}

/// <summary>
/// Unidade de trabalho falsa. Conta as gravacoes e executa a transacao
/// direto -- nao ha banco para transacionar. O contador existe para os
/// testes poderem afirmar que houve gravacao, nao so que o metodo devolveu
/// true.
/// </summary>
public class UnidadeDeTrabalhoFalsa : IUnidadeDeTrabalho
{
    public int Gravacoes { get; private set; }

    public Task SalvarAsync(CancellationToken ct = default)
    {
        Gravacoes++;
        return Task.CompletedTask;
    }

    public Task ExecutarEmTransacaoAsync(Func<Task> acao, CancellationToken ct = default) => acao();
}
