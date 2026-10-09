using TechQuest.Application.Dtos;
using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;

namespace TechQuest.Application.Servicos;

public class ProgressoService
{
    private readonly IProgressoRepository _progressos;
    private readonly ICursoRepository _cursos;
    private readonly IHistoricoRepository _historicos;
    private readonly ConquistaService _conquistas;
    private readonly GamificacaoService _gamificacao;
    private readonly ConclusaoCursoService _conclusao;
    private readonly IUnidadeDeTrabalho _uow;

    public ProgressoService(
        IProgressoRepository progressos, ICursoRepository cursos,
        IHistoricoRepository historicos, ConquistaService conquistas,
        GamificacaoService gamificacao, ConclusaoCursoService conclusao,
        IUnidadeDeTrabalho uow)
    {
        _progressos = progressos; _cursos = cursos; _historicos = historicos;
        _conquistas = conquistas; _gamificacao = gamificacao;
        _conclusao = conclusao; _uow = uow;
    }

    /// <summary>
    /// Matricula o estudante no curso.
    ///
    /// O modelo do PIM III nao tem tabela Matricula: quem registra o vinculo
    /// aluno-curso e Historico. Matricular = criar Historico "Em andamento";
    /// concluir = mudar o status e preencher a data.
    /// </summary>
    public async Task<MatriculaDto?> MatricularAsync(
        int idEstudante, int idCurso, CancellationToken ct = default)
    {
        var curso = await _cursos.ObterComMateriaisAsync(idCurso, ct);
        if (curso is null || !curso.EstaPublicado()) return null;

        var existente = await _historicos.ObterAsync(idEstudante, idCurso, ct);
        if (existente is not null)
            return new MatriculaDto(existente.IdHistorico, idCurso, curso.Nome,
                                    existente.StatusConclusao ?? "Em andamento");

        var historico = new Historico
        {
            IdEstudante = idEstudante,
            IdCurso = idCurso,
            StatusConclusao = "Em andamento"
        };
        _historicos.Adicionar(historico);
        await _uow.SalvarAsync(ct);

        return new MatriculaDto(historico.IdHistorico, idCurso, curso.Nome, "Em andamento");
    }

    /// <summary>
    /// Marca a aula como assistida/concluida e reavalia o curso inteiro.
    /// Tudo em uma transacao: progresso, conclusao do curso, certificado e
    /// medalhas precisam valer juntos.
    /// </summary>
    public async Task<ProgressoAtualizadoDto?> AtualizarAsync(
        int idEstudante, int idMaterial, AtualizarProgressoRequest req, CancellationToken ct = default)
    {
        var material = await _cursos.ObterMaterialAsync(idMaterial, ct);
        if (material is null) return null;

        var porcentagem = Math.Clamp(req.PorcentagemAssistida, 0, 100);
        var concluido = req.Concluido || porcentagem >= 100;

        ProgressoAtualizadoDto? resultado = null;

        await _uow.ExecutarEmTransacaoAsync(async () =>
        {
            var progresso = await _progressos.ObterAsync(idEstudante, idMaterial, ct);
            if (progresso is null)
            {
                progresso = new Progresso
                {
                    IdEstudante = idEstudante,
                    IdMaterial = idMaterial,
                    Concluido = concluido,
                    PorcentagemAssistida = porcentagem,
                    DataVisualizacao = DateTime.UtcNow
                };
                _progressos.Adicionar(progresso);
            }
            else
            {
                // Progresso nunca retrocede: reassistir uma aula ja concluida
                // nao pode desmarca-la nem reduzir a porcentagem.
                progresso.Concluido = progresso.Concluido || concluido;
                progresso.PorcentagemAssistida = Math.Max(progresso.PorcentagemAssistida, porcentagem);
                progresso.DataVisualizacao = DateTime.UtcNow;
            }

            await _uow.SalvarAsync(ct);

            var (total, concluidas) = await _progressos.ContarAulasDoCursoAsync(
                idEstudante, material.IdCurso, ct);

            // A regra de conclusao saiu daqui para ConclusaoCursoService: ela
            // tem dois gatilhos (ultimo material / prova aprovada) e precisa de
            // uma definicao so. Ver o comentario daquele arquivo.
            var cursoConcluido = await _conclusao.AvaliarAsync(
                idEstudante, material.IdCurso, total, concluidas, ct);

            var novas = await _conquistas.AvaliarAsync(idEstudante, ct);
            await _uow.SalvarAsync(ct);

            var xp = await _gamificacao.ObterAsync(idEstudante, ct);

            resultado = new ProgressoAtualizadoDto(
                idMaterial, progresso.Concluido, progresso.PorcentagemAssistida,
                concluidas, total,
                total == 0 ? 0 : (int)Math.Round(concluidas * 100.0 / total),
                cursoConcluido, xp.Xp, xp.Nivel, novas);
        }, ct);

        return resultado;
    }

}
