using TechQuest.Desktop.Aplicacao.Comum;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Comum;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Apresentacao.Medalhas;

public interface IMedalhaEditorView : IViewBase
{
    string Nome { get; }
    string? Raridade { get; }
    string Descricao { get; }

    void Preencher(string titulo, string nome, string raridade, string descricao, string? aviso);
    bool Confirmar(string mensagem, string titulo);
    void Concluir(MedalhaAdmin salva);
}

/// <summary>Criacao e edicao de uma medalha do catalogo.</summary>
public sealed class MedalhaEditorPresenter : PresenterBase
{
    private readonly IMedalhaEditorView _view;
    private readonly MedalhasService _servico;
    private readonly MedalhaAdmin? _existente;

    public MedalhaEditorPresenter(
        IMedalhaEditorView view, MedalhasService servico, SessaoAdmin sessao, MedalhaAdmin? existente)
        : base(view, sessao)
    {
        _view = view;
        _servico = servico;
        _existente = existente;
    }

    public void Iniciar()
    {
        if (_existente is null)
        {
            _view.Preencher("Nova medalha", "", Raridades.Comum, "", null);
            return;
        }

        var aviso = _existente.TotalConquistas > 0
            ? $"Já conquistada por {_existente.TotalConquistas} " +
              (_existente.TotalConquistas == 1 ? "estudante" : "estudantes") +
              ": a alteração aparece no perfil de quem já a tem."
            : null;

        _view.Preencher("Editar medalha", _existente.Nome,
            Raridades.Normalizar(_existente.Raridade) ?? Raridades.Comum,
            _existente.Descricao ?? "", aviso);
    }

    public async Task SalvarAsync()
    {
        if (_existente is not null && MedalhasService.RenomeiaMedalha(_existente, _view.Nome)
            && !_view.Confirmar(
                $"Renomear \"{_existente.Nome}\" para \"{_view.Nome.Trim()}\"?\n\n" +
                "Algumas medalhas são concedidas automaticamente pelo nome (por exemplo, \"Primeiro Passo\"). " +
                "Se esta for uma delas, o servidor deixa de concedê-la depois da troca.",
                "Renomear medalha"))
            return;

        ResultadoOperacao<MedalhaAdmin>? r = null;
        if (!await ExecutarAsync(async () => r = await _servico.SalvarAsync(
                _existente?.Id, _view.Nome, _view.Raridade, _view.Descricao)))
            return;

        if (!r!.Sucesso) { _view.MostrarErro(r.Mensagem!); return; }
        _view.Concluir(r.Valor!);
    }
}
