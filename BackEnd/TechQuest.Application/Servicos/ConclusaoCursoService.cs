using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;
using TechQuest.Domain.Regras;

namespace TechQuest.Application.Servicos;

/// <summary>
/// Decide se um curso esta concluido e emite o certificado.
///
/// POR QUE ISTO E UM SERVICO PROPRIO (e o defeito que o originou)
/// A regra tem UM significado -- "todos os materiais concluidos E, se houver
/// prova, a prova aprovada" -- mas DOIS gatilhos: o aluno pode satisfazer a
/// ultima condicao marcando o ultimo material, ou passando na prova.
///
/// Antes, a regra morava dentro de ProgressoService como metodo privado, e so
/// o primeiro gatilho existia. Quem concluisse todos os materiais ANTES de
/// fazer a prova nunca recebia o certificado: a avaliacao rodava com a prova
/// ainda reprovada e nada a executava de novo depois da aprovacao. Fazer a
/// prova primeiro e o ultimo material depois funcionava -- por acidente da
/// ordem, nao por desenho.
///
/// ALTERNATIVAS CONSIDERADAS E REJEITADAS
///   (a) Tornar o metodo publico em ProgressoService e injetar esse servico em
///       ProvaService. Criaria dependencia entre dois servicos de mesmo nivel
///       por causa de uma regra que nao pertence a nenhum dos dois.
///   (b) Repetir a verificacao em ProvaService. Duas copias da mesma regra
///       divergem na primeira manutencao -- e foi justamente a ausencia de uma
///       fonte unica que produziu o defeito.
///   (c) Trigger no banco, disparada por INSERT em Desempenho e em Progresso.
///       Emitir certificado e regra de negocio; escondida numa trigger, ela
///       fica invisivel ao codigo, aos testes e a quem le a aplicacao.
///
/// IDEMPOTENTE: chamar varias vezes nao emite certificado duplicado. O curso
/// ja marcado como "Concluido" devolve true e sai antes de gravar.
///
/// NAO ABRE TRANSACAO: quem chama ja esta dentro de uma. Progresso, conclusao,
/// certificado e medalhas precisam valer juntos ou nao valer.
/// </summary>
public class ConclusaoCursoService
{
    private readonly ICursoRepository _cursos;
    private readonly IProgressoRepository _progressos;
    private readonly IDesempenhoRepository _desempenhos;
    private readonly IHistoricoRepository _historicos;
    private readonly ICertificadoRepository _certificados;
    private readonly IUnidadeDeTrabalho _uow;

    public ConclusaoCursoService(
        ICursoRepository cursos, IProgressoRepository progressos,
        IDesempenhoRepository desempenhos, IHistoricoRepository historicos,
        ICertificadoRepository certificados, IUnidadeDeTrabalho uow)
    {
        _cursos = cursos;
        _progressos = progressos;
        _desempenhos = desempenhos;
        _historicos = historicos;
        _certificados = certificados;
        _uow = uow;
    }

    /// <summary>
    /// Reavalia a conclusao do curso para este aluno. Devolve true se o curso
    /// esta concluido (agora ou ja estava).
    /// </summary>
    public async Task<bool> AvaliarAsync(
        int idEstudante, int idCurso, CancellationToken ct = default)
    {
        var (total, concluidas) = await _progressos.ContarAulasDoCursoAsync(
            idEstudante, idCurso, ct);

        return await AvaliarAsync(idEstudante, idCurso, total, concluidas, ct);
    }

    /// <summary>
    /// Sobrecarga para quem ja contou as aulas -- evita repetir a consulta
    /// logo depois de gravar progresso.
    /// </summary>
    public async Task<bool> AvaliarAsync(
        int idEstudante, int idCurso, int totalAulas, int aulasConcluidas,
        CancellationToken ct = default)
    {
        var curso = await _cursos.ObterComMateriaisAsync(idCurso, ct);
        if (curso is null) return false;

        var temProva = curso.IdProva is not null;
        var provaAprovada = temProva
            && await _desempenhos.AprovadoNaProvaAsync(idEstudante, curso.IdProva!.Value, ct);

        // O CRITERIO, aplicado UMA VEZ e num lugar so.
        //
        // Nao ha corte antecipado antes de carregar o curso de proposito:
        // repetir "se faltam materiais, desiste" aqui seria uma segunda
        // copia da regra, e duas copias divergem na primeira manutencao --
        // que e a mesma armadilha que produziu o defeito do certificado.
        // O custo e uma leitura a mais no caso em que o curso ainda nao
        // acabou; este metodo roda uma vez por material concluido ou prova
        // enviada, nunca em laco.
        if (!RegrasConclusaoCurso.CursoConcluido(
                totalAulas, aulasConcluidas, temProva, provaAprovada))
            return false;

        var historico = await _historicos.ObterAsync(idEstudante, idCurso, ct);
        if (historico is null)
        {
            historico = new Historico { IdEstudante = idEstudante, IdCurso = idCurso };
            _historicos.Adicionar(historico);
        }

        // Ja concluido: nao regrava nem emite um segundo certificado.
        if (historico.StatusConclusao == "Concluido") return true;

        historico.StatusConclusao = "Concluido";
        historico.DataConclusao = DateOnly.FromDateTime(DateTime.Today);
        await _uow.SalvarAsync(ct);

        // O codigo de autenticacao nao e informado: quem gera e o banco, pelo
        // DEFAULT NEWID() da coluna. Gerar no C# significaria duas fontes para
        // o mesmo identificador.
        _certificados.Adicionar(new Certificado
        {
            IdHistorico = historico.IdHistorico,
            DataEmissao = DateTime.UtcNow
        });

        return true;
    }

    /// <summary>
    /// Reavalia pelo id da PROVA, que e o que a submissao conhece.
    ///
    /// Prova nao guarda o curso: a chave estrangeira esta em Curso.ID_Prova.
    /// A direcao faz sentido no modelo (um curso TEM uma prova; a prova nao
    /// precisa saber de quem e), mas obriga esta consulta de volta.
    /// </summary>
    public async Task<bool> AvaliarPorProvaAsync(
        int idEstudante, int idProva, CancellationToken ct = default)
    {
        var idCurso = await _cursos.ObterIdCursoPorProvaAsync(idProva, ct);
        if (idCurso is null) return false;

        return await AvaliarAsync(idEstudante, idCurso.Value, ct);
    }
}
