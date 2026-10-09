using System.Globalization;
using TechQuest.Desktop.Aplicacao.Comum;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Comum;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Apresentacao.Medalhas;

public interface IMedalhasView : IViewBase
{
    /// <summary>Uma das Raridades, ou null (todas).</summary>
    string? FiltroRaridade { get; }
    string TermoBusca { get; }
    int? MedalhaSelecionada { get; }

    void ExibirFiltros(IReadOnlyList<OpcaoFiltro> opcoes);
    void ExibirMedalhas(IReadOnlyList<LinhaMedalha> linhas, string resumo);
    void Selecionar(int idMedalha);
    void OcultarErro();
    void DefinirEdicaoDisponivel(bool disponivel);

    /// <summary>Abre o editor (null = nova). Devolve a medalha salva, ou null se cancelou.</summary>
    MedalhaAdmin? AbrirEditor(MedalhaAdmin? existente);
}

public sealed record LinhaMedalha(int Id, string Nome, string Raridade, string Descricao, string Conquistas);

/// <summary>Catalogo de medalhas: lista, filtro por raridade, criar e editar.</summary>
public sealed class MedalhasPresenter : PresenterBase
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IMedalhasView _view;
    private readonly MedalhasService _servico;
    private IReadOnlyList<MedalhaAdmin> _todas = Array.Empty<MedalhaAdmin>();

    public MedalhasPresenter(IMedalhasView view, MedalhasService servico, SessaoAdmin sessao)
        : base(view, sessao)
    {
        _view = view;
        _servico = servico;
    }

    public async Task CarregarAsync(int? selecionar = null)
    {
        _view.OcultarErro();
        var anterior = selecionar ?? _view.MedalhaSelecionada;

        IReadOnlyList<MedalhaAdmin>? medalhas = null;
        if (!await ExecutarAsync(async () => medalhas = await _servico.ListarAsync())) return;

        // Mais comum para mais rara, como uma vitrine; dentro da raridade, por nome.
        _todas = medalhas!
            .OrderBy(m => OrdemRaridade(m.Raridade))
            .ThenBy(m => m.Nome, StringComparer.Create(PtBr, ignoreCase: true))
            .ToList();

        _view.ExibirFiltros(Filtros(_todas));
        Filtrar();
        if (anterior is int id && _todas.Any(m => m.Id == id)) _view.Selecionar(id);
        SelecaoMudou();
    }

    public void Filtrar()
    {
        var raridade = _view.FiltroRaridade;
        var termo = _view.TermoBusca;

        var visiveis = _todas
            .Where(m => raridade is null || Raridades.Normalizar(m.Raridade) == raridade)
            .Where(m => TextoBusca.Contem(termo, m.Nome, m.Descricao))
            .Select(Linha)
            .ToList();

        _view.ExibirMedalhas(visiveis, visiveis.Count == _todas.Count
            ? $"{_todas.Count} medalhas no catálogo."
            : $"Exibindo {visiveis.Count} de {_todas.Count} medalhas.");
    }

    public void SelecaoMudou() => _view.DefinirEdicaoDisponivel(Selecionada() is not null);

    public async Task NovaAsync()
    {
        var salva = _view.AbrirEditor(null);
        if (salva is not null) await CarregarAsync(selecionar: salva.Id);
    }

    public async Task EditarSelecionadaAsync()
    {
        var m = Selecionada();
        if (m is null) return;
        var salva = _view.AbrirEditor(m);
        if (salva is not null) await CarregarAsync(selecionar: salva.Id);
    }

    private MedalhaAdmin? Selecionada()
        => _view.MedalhaSelecionada is int id ? _todas.FirstOrDefault(m => m.Id == id) : null;

    internal static LinhaMedalha Linha(MedalhaAdmin m) => new(
        m.Id, m.Nome, NomeDaRaridade(m.Raridade), m.Descricao ?? "",
        m.TotalConquistas switch
        {
            0 => "nenhuma ainda",
            1 => "1 estudante",
            var n => $"{n} estudantes"
        });

    /// <summary>Rotulo de exibicao; raridade fora do vocabulario aparece como veio.</summary>
    public static string NomeDaRaridade(string? raridade) => Raridades.Normalizar(raridade) switch
    {
        Raridades.Comum => "Comum",
        Raridades.Raro => "Rara",
        Raridades.Epico => "Épica",
        Raridades.Lendario => "Lendária",
        _ => string.IsNullOrWhiteSpace(raridade) ? "-" : raridade!
    };

    private static string Plural(string raridade) => raridade switch
    {
        Raridades.Comum => "Comuns",
        Raridades.Raro => "Raras",
        Raridades.Epico => "Épicas",
        Raridades.Lendario => "Lendárias",
        _ => raridade
    };

    private static int OrdemRaridade(string? r)
    {
        var i = Raridades.Todas.ToList().IndexOf(Raridades.Normalizar(r) ?? "");
        return i < 0 ? int.MaxValue : i;
    }

    internal static IReadOnlyList<OpcaoFiltro> Filtros(IReadOnlyList<MedalhaAdmin> todas)
    {
        var opcoes = new List<OpcaoFiltro> { new(null, $"Todas ({todas.Count})") };
        foreach (var r in Raridades.Todas)
        {
            var n = todas.Count(m => Raridades.Normalizar(m.Raridade) == r);
            opcoes.Add(new OpcaoFiltro(r, $"{Plural(r)} ({n})"));
        }
        return opcoes;
    }
}
