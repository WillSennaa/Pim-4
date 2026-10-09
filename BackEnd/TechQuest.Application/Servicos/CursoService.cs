using TechQuest.Application.Dtos;
using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;

namespace TechQuest.Application.Servicos;

public class CursoService
{
    private readonly ICursoRepository _cursos;
    private readonly IProgressoRepository _progressos;
    private readonly IHistoricoRepository _historicos;

    public CursoService(
        ICursoRepository cursos, IProgressoRepository progressos,
        IHistoricoRepository historicos)
    {
        _cursos = cursos;
        _progressos = progressos;
        _historicos = historicos;
    }

    public async Task<IReadOnlyList<CursoResumoDto>> ListarAsync(CancellationToken ct = default)
    {
        var cursos = await _cursos.ListarPublicadosAsync(ct);
        return cursos.Select(c => new CursoResumoDto(
            c.IdCurso, c.Nome, c.Descricao, c.Categoria, c.Nivel, c.DuracaoHoras,
            c.TutorCriou?.Usuario.Nome, c.Status, c.IdProva is not null)).ToList();
    }

    /// <summary>
    /// idEstudante e opcional: sem aluno autenticado devolve o curso sem
    /// progresso; com aluno, marca cada material como concluido ou nao.
    /// </summary>
    public async Task<CursoDetalheDto?> ObterAsync(
        int idCurso, int? idEstudante, CancellationToken ct = default)
    {
        var curso = await _cursos.ObterComMateriaisAsync(idCurso, ct);
        if (curso is null) return null;

        IReadOnlyDictionary<int, Progresso> progresso = idEstudante is null
            ? new Dictionary<int, Progresso>()
            : await _progressos.ObterPorCursoAsync(idEstudante.Value, idCurso, ct);

        var materiais = curso.Materiais
            .OrderBy(m => m.IdMaterial)
            .Select(m =>
            {
                progresso.TryGetValue(m.IdMaterial, out var p);
                return new MaterialDto(
                    m.IdMaterial, m.Titulo, m.Tipo, m.Conteudo,
                    p?.Concluido ?? false,
                    p?.PorcentagemAssistida ?? 0);
            })
            .ToList();

        // Matricula: uma consulta a mais, pela chave (ID_Estudante, ID_Curso).
        // Sem ela a tela teria de inferir do historico completo do aluno, o que
        // significa baixar dados de todos os cursos para responder sobre um.
        var matriculado = idEstudante is not null
            && await _historicos.ObterAsync(idEstudante.Value, idCurso, ct) is not null;

        return new CursoDetalheDto(
            curso.IdCurso, curso.Nome, curso.Descricao, curso.Categoria, curso.Nivel,
            curso.DuracaoHoras,
            curso.TutorCriou?.Usuario.Nome,
            curso.TutorCriou?.Usuario.Iniciais(),
            curso.Status, curso.IdProva, matriculado, materiais);
    }
}
