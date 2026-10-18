using System.Globalization;
using TechQuest.Desktop.Aplicacao.Comum;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Comum;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Apresentacao.Logs;

public interface ILogsView : IViewBase
{
    int Limite { get; }
    string TermoBusca { get; }
    void ExibirLogs(IReadOnlyList<LinhaLog> linhas, string resumo);
    void OcultarErro();
}

public sealed record LinhaLog(string Data, string Administrador, string Acao);

/// <summary>
/// Trilha de auditoria (RF11), do mais recente para o mais antigo.
///
/// LIMITACAO CONHECIDA, exibida na propria tela: as mudancas de status de
/// usuario sao gravadas pela trigger TRG_Auditoria_StatusUsuario, que nao
/// sabe qual administrador agiu (a API usa uma conexao unica com o banco) e
/// grava sempre ID_ADM = 1. As avaliacoes de curso nao tem o problema,
/// porque e a API que grava o log, com o ID real.
/// </summary>
public sealed class LogsPresenter : PresenterBase
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly ILogsView _view;
    private readonly AuditoriaService _servico;
    private readonly TimeZoneInfo? _fuso;
    private IReadOnlyList<LogAuditoria> _todos = Array.Empty<LogAuditoria>();
    private int _limiteCarregado;

    public LogsPresenter(ILogsView view, AuditoriaService servico, SessaoAdmin sessao, TimeZoneInfo? fuso = null)
        : base(view, sessao)
    {
        _view = view;
        _servico = servico;
        _fuso = fuso;
    }

    public async Task CarregarAsync()
    {
        _view.OcultarErro();
        var limite = _view.Limite;

        IReadOnlyList<LogAuditoria>? logs = null;
        if (!await ExecutarAsync(async () => logs = await _servico.ListarAsync(limite))) return;

        // A API ja ordena, mas a tela nao depende disso: auditoria fora de
        // ordem cronologica engana quem esta reconstruindo um acontecimento.
        _todos = logs!.OrderByDescending(l => l.Data).ThenByDescending(l => l.Id).ToList();
        _limiteCarregado = limite;
        Filtrar();
    }

    public void Filtrar()
    {
        var termo = _view.TermoBusca;
        var visiveis = _todos
            .Where(l => TextoBusca.Contem(termo, l.Acao, l.Administrador))
            .Select(l => new LinhaLog(
                HorarioServidor.ParaLocal(l.Data, _fuso).ToString("dd/MM/yyyy HH:mm", PtBr),
                l.Administrador ?? $"administrador nº {l.IdAdm}",
                l.Acao ?? "-"))
            .ToList();

        _view.ExibirLogs(visiveis, Resumo(visiveis.Count, _todos.Count, _limiteCarregado));
    }

    internal static string Resumo(int visiveis, int carregados, int limite)
    {
        if (carregados == 0) return "Nenhum registro de auditoria.";
        var base_ = carregados < limite
            ? $"Todos os {carregados} registros"
            : $"Os {carregados} registros mais recentes";
        return visiveis == carregados ? base_ + "." : $"{visiveis} de {carregados} registros correspondem à busca.";
    }
}
