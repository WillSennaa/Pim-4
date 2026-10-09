using System.Globalization;
using TechQuest.Desktop.Aplicacao.Comum;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Comum;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Apresentacao.Suporte;

public interface ISuporteView : IViewBase
{
    /// <summary>"abertos" (Aberto + Em andamento), "resolvidos" ou null (todos).</summary>
    string? FiltroSituacao { get; }
    string TermoBusca { get; }
    int? ChamadoSelecionado { get; }
    string Resposta { get; }

    void ExibirChamados(IReadOnlyList<LinhaChamado> linhas, string resumo);
    void Selecionar(int idChamado);

    /// <summary>Painel lateral do chamado selecionado; null limpa o painel.</summary>
    void ExibirDetalhe(DetalheChamado? detalhe);

    void LimparResposta();
    void FocarResposta();
    void OcultarErro();
    bool Confirmar(string mensagem, string titulo);
    void Informar(string mensagem);
}

public sealed record LinhaChamado(int Id, string Numero, string Assunto, string Remetente, string AbertoEm, string Situacao);

public sealed record DetalheChamado(
    int Id, string Titulo, string Remetente, string AbertoEm, string Descricao,
    string Situacao, bool PodeAssumir, bool PodeResponder);

/// <summary>
/// Fila de suporte tecnico com o chamado selecionado ao lado: o admin le,
/// assume e responde sem abrir outra janela, como nos clientes de e-mail.
/// </summary>
public sealed class SuportePresenter : PresenterBase
{
    public const string FiltroAbertos = "abertos";
    public const string FiltroResolvidos = "resolvidos";

    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly ISuporteView _view;
    private readonly SuporteService _servico;
    private readonly TimeZoneInfo? _fuso;
    private IReadOnlyList<Chamado> _todos = Array.Empty<Chamado>();

    public SuportePresenter(ISuporteView view, SuporteService servico, SessaoAdmin sessao, TimeZoneInfo? fuso = null)
        : base(view, sessao)
    {
        _view = view;
        _servico = servico;
        _fuso = fuso;
    }

    public async Task CarregarAsync(int? selecionar = null)
    {
        _view.OcultarErro();
        var anterior = selecionar ?? _view.ChamadoSelecionado;

        IReadOnlyList<Chamado>? chamados = null;
        if (!await ExecutarAsync(async () => chamados = await _servico.ListarAsync()))
        {
            SelecaoMudou();
            return;
        }

        // Fila: o que espera ha mais tempo vem primeiro. Resolvidos por ultimo,
        // do mais recente para o mais antigo.
        _todos = chamados!
            .OrderBy(c => Ordem(c.Status))
            .ThenBy(c => c.Status == StatusChamado.Resolvido ? -c.DataAbertura.Ticks : c.DataAbertura.Ticks)
            .ToList();

        Filtrar();
        if (anterior is int id && _todos.Any(c => c.Id == id)) _view.Selecionar(id);
        SelecaoMudou();
    }

    public void Filtrar()
    {
        var filtro = _view.FiltroSituacao;
        var termo = _view.TermoBusca;

        var visiveis = _todos
            .Where(c => filtro switch
            {
                FiltroAbertos => c.Status != StatusChamado.Resolvido,
                FiltroResolvidos => c.Status == StatusChamado.Resolvido,
                _ => true
            })
            .Where(c => TextoBusca.Contem(termo, c.Assunto, c.Remetente, c.Descricao, "#" + c.Id))
            .ToList();

        _view.ExibirChamados(visiveis.Select(Linha).ToList(), Resumo(visiveis));
    }

    public void SelecaoMudou()
    {
        var c = Selecionado();
        _view.ExibirDetalhe(c is null ? null : new DetalheChamado(
            c.Id,
            $"#{c.Id} · {c.Assunto ?? "Sem assunto"}",
            c.Remetente,
            Data(c.DataAbertura),
            string.IsNullOrWhiteSpace(c.Descricao) ? "(sem descrição)" : c.Descricao!,
            c.Status,
            PodeAssumir: c.Status == StatusChamado.Aberto,
            PodeResponder: c.Status != StatusChamado.Resolvido));
    }

    public async Task AssumirAsync()
    {
        var c = Selecionado();
        if (c is null) return;

        ResultadoOperacao? r = null;
        if (!await ExecutarAsync(async () => r = await _servico.AssumirAsync(c))) return;
        if (!r!.Sucesso) { _view.MostrarErro(r.Mensagem!); return; }

        await CarregarAsync(selecionar: c.Id);
    }

    public async Task ResponderAsync()
    {
        var c = Selecionado();
        if (c is null) return;

        var texto = _view.Resposta;
        var previa = SuporteService.ValidarResposta(texto);
        if (!previa.Sucesso)
        {
            _view.MostrarErro(previa.Mensagem!);
            _view.FocarResposta();
            return;
        }

        if (!_view.Confirmar(
                $"Enviar a resposta para {c.Remetente} e marcar o chamado #{c.Id} como resolvido?",
                "Responder e resolver"))
            return;

        ResultadoOperacao? r = null;
        if (!await ExecutarAsync(async () => r = await _servico.ResponderAsync(c, texto))) return;
        if (!r!.Sucesso) { _view.MostrarErro(r.Mensagem!); return; }

        _view.LimparResposta();
        _view.Informar($"Resposta enviada. {c.Remetente} a recebe como mensagem nos chamados da plataforma.");
        await CarregarAsync(selecionar: c.Id);
    }

    private Chamado? Selecionado()
        => _view.ChamadoSelecionado is int id ? _todos.FirstOrDefault(c => c.Id == id) : null;

    private LinhaChamado Linha(Chamado c) => new(
        c.Id, "#" + c.Id, c.Assunto ?? "Sem assunto", c.Remetente, Data(c.DataAbertura), c.Status);

    private string Data(DateTime doServidor)
        => HorarioServidor.ParaLocal(doServidor, _fuso).ToString("dd/MM/yyyy HH:mm", PtBr);

    private static int Ordem(string status) => status switch
    {
        StatusChamado.Aberto => 0,
        StatusChamado.EmAndamento => 1,
        _ => 2
    };

    internal string Resumo(IReadOnlyList<Chamado> visiveis)
    {
        var abertos = _todos.Count(c => c.Status == StatusChamado.Aberto);
        var andamento = _todos.Count(c => c.Status == StatusChamado.EmAndamento);
        if (_todos.Count == 0) return "Nenhum chamado técnico registrado.";
        return $"{abertos} {(abertos == 1 ? "aberto" : "abertos")} · {andamento} em andamento · " +
               $"exibindo {visiveis.Count} de {_todos.Count}";
    }
}
