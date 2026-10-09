using TechQuest.Desktop.Aplicacao.Interfaces;
using TechQuest.Desktop.Apresentacao.Aprovacoes;
using TechQuest.Desktop.Apresentacao.Revisao;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Tests.Dubles;

/// <summary>
/// API de cursos falsa: devolve o que o teste configurar e registra as
/// decisoes recebidas, para o teste conferir o que foi (ou nao foi) enviado.
/// </summary>
public sealed class CursosAdminGatewayFalso : ICursosAdminGateway
{
    public List<SolicitacaoCurso> Fila { get; set; } = new();
    public CursoRevisao? Curso { get; set; }

    /// <summary>Se definido, Aprovar/Rejeitar lancam esta excecao (recusa do servidor).</summary>
    public Exception? FalhaNaDecisao { get; set; }

    public int Listagens { get; private set; }
    public List<int> Aprovados { get; } = new();
    public List<(int Id, string Motivo)> Rejeitados { get; } = new();

    public List<string> EstadosPedidos { get; } = new();

    /// <summary>Como a API: devolve so os cursos do estado pedido.</summary>
    public Task<IReadOnlyList<SolicitacaoCurso>> ListarAsync(string status, CancellationToken ct = default)
    {
        Listagens++;
        EstadosPedidos.Add(status);
        return Task.FromResult<IReadOnlyList<SolicitacaoCurso>>(
            Fila.Where(c => c.Status == status).ToList());
    }

    public Task<CursoRevisao> ObterParaRevisaoAsync(int idCurso, CancellationToken ct = default)
        => Task.FromResult(Curso ?? throw new InvalidOperationException("Curso nao configurado no teste."));

    public Task AprovarAsync(int idCurso, CancellationToken ct = default)
    {
        if (FalhaNaDecisao is not null) throw FalhaNaDecisao;
        Aprovados.Add(idCurso);
        return Task.CompletedTask;
    }

    public Task RejeitarAsync(int idCurso, string motivo, CancellationToken ct = default)
    {
        if (FalhaNaDecisao is not null) throw FalhaNaDecisao;
        Rejeitados.Add((idCurso, motivo));
        return Task.CompletedTask;
    }
}

public sealed class AprovacoesViewFalsa : IAprovacoesView
{
    public IReadOnlyList<LinhaSolicitacao>? Linhas { get; private set; }
    public string? Resumo { get; private set; }
    public string? Erro { get; private set; }
    public int? CursoSelecionado { get; set; }
    public List<int> RevisoesAbertas { get; } = new();

    public void ExibirFila(IReadOnlyList<LinhaSolicitacao> linhas, string resumo) { Linhas = linhas; Resumo = resumo; }
    public void OcultarErro() => Erro = null;
    public void AbrirRevisao(int idCurso) => RevisoesAbertas.Add(idCurso);
    public void DefinirOcupado(bool ocupado) { }
    public void MostrarErro(string mensagem) => Erro = mensagem;
}

public sealed class RevisaoViewFalsa : IRevisaoCursoView
{
    public RevisaoExibicao? Exibido { get; private set; }
    public string MotivoRejeicao { get; set; } = "";
    public bool RespostaConfirmacao { get; set; } = true;
    public List<string> Confirmacoes { get; } = new();
    public string? Erro { get; private set; }
    public string? Conclusao { get; private set; }
    public bool MotivoFocado { get; private set; }

    public void Exibir(RevisaoExibicao revisao) => Exibido = revisao;
    public void FocarMotivo() => MotivoFocado = true;
    public bool Confirmar(string mensagem, string titulo) { Confirmacoes.Add(mensagem); return RespostaConfirmacao; }
    public void Concluir(string mensagem) => Conclusao = mensagem;
    public void DefinirOcupado(bool ocupado) { }
    public void MostrarErro(string mensagem) => Erro = mensagem;
}

public static class CursosDeTeste
{
    public static SolicitacaoCurso Solicitacao(int id, int aulas = 3, bool prova = true, int questoes = 5,
                                               string status = StatusCurso.Pendente, string? nome = null,
                                               string tutor = "Tutor Teste")
        => new(id, nome ?? $"Curso {id}", null, "Back-end", "Iniciante", tutor,
               status, aulas, prova, questoes);

    /// <summary>Curso pronto para publicar: material com texto, prova com gabarito.</summary>
    public static CursoRevisao Completo(string status = StatusCurso.Pendente) => new(
        IdCurso: 7,
        Nome: "C# Basico",
        Descricao: "Introducao a linguagem.",
        Categoria: "Back-end",
        Nivel: "Iniciante",
        DuracaoHoras: 10,
        Tutor: "Tutor Teste",
        Status: status,
        Materiais: new[]
        {
            new MaterialRevisao(1, "Variaveis", "texto", "Paragrafo um.\n\n    int x = 1;\n    x++;\n\nParagrafo dois.")
        },
        Prova: new ProvaRevisao(1, "Prova final", 7m, 30, new[]
        {
            new QuestaoRevisao(1, 1, "Quanto e 1+1?", null, new[]
            {
                new AlternativaRevisao(1, "A", "1", false),
                new AlternativaRevisao(2, "B", "2", true)
            })
        }));
}
