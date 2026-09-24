using TechQuest.Application.Dtos;
using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;

namespace TechQuest.Application.Servicos;

public class ProgressoService
{
    private readonly IProgressoRepository _progressos;
    private readonly ICursoRepository _cursos;
    private readonly IHistoricoRepository _historicos;
    private readonly ICertificadoRepository _certificados;
    private readonly IDesempenhoRepository _desempenhos;
    private readonly ConquistaService _conquistas;
    private readonly GamificacaoService _gamificacao;
    private readonly IUnidadeDeTrabalho _uow;

    public ProgressoService(
        IProgressoRepository progressos, ICursoRepository cursos,
        IHistoricoRepository historicos, ICertificadoRepository certificados,
        IDesempenhoRepository desempenhos, ConquistaService conquistas,
        GamificacaoService gamificacao, IUnidadeDeTrabalho uow)
    {
        _progressos = progressos; _cursos = cursos; _historicos = historicos;
        _certificados = certificados; _desempenhos = desempenhos;
        _conquistas = conquistas; _gamificacao = gamificacao; _uow = uow;
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

            var cursoConcluido = await AvaliarConclusaoAsync(
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

    /// <summary>
    /// Regra de conclusao: todas as aulas concluidas E, se o curso tiver prova,
    /// a prova aprovada. Ao concluir, emite o certificado.
    /// </summary>
    private async Task<bool> AvaliarConclusaoAsync(
        int idEstudante, int idCurso, int totalAulas, int aulasConcluidas, CancellationToken ct)
    {
        if (totalAulas == 0 || aulasConcluidas < totalAulas) return false;

        var curso = await _cursos.ObterComMateriaisAsync(idCurso, ct);
        if (curso is null) return false;

        if (curso.IdProva is not null)
        {
            var aprovado = await _desempenhos.AprovadoNaProvaAsync(idEstudante, curso.IdProva.Value, ct);
            if (!aprovado) return false;
        }

        var historico = await _historicos.ObterAsync(idEstudante, idCurso, ct);
        if (historico is null)
        {
            historico = new Historico { IdEstudante = idEstudante, IdCurso = idCurso };
            _historicos.Adicionar(historico);
        }

        if (historico.StatusConclusao == "Concluido") return true;

        historico.StatusConclusao = "Concluido";
        historico.DataConclusao = DateOnly.FromDateTime(DateTime.Today);
        await _uow.SalvarAsync(ct);

        // O codigo de autenticacao nao e informado: quem gera e o banco,
        // pelo DEFAULT NEWID() da coluna.
        _certificados.Adicionar(new Certificado
        {
            IdHistorico = historico.IdHistorico,
            DataEmissao = DateTime.UtcNow
        });

        return true;
    }
}
