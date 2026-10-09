using TechQuest.Desktop.Apresentacao.Comum;

namespace TechQuest.Desktop.Apresentacao.Revisao;

public interface IRevisaoCursoView : IViewBase
{
    void Exibir(RevisaoExibicao revisao);

    string MotivoRejeicao { get; }

    void FocarMotivo();

    bool Confirmar(string mensagem, string titulo);

    /// <summary>Informa o desfecho e fecha a janela de revisao.</summary>
    void Concluir(string mensagem);
}

/// <summary>
/// Tudo o que a revisao mostra, ja formatado. Os blocos dos materiais ja
/// vem separados em texto e codigo; a tela so escolhe a fonte de cada um.
/// </summary>
public sealed record RevisaoExibicao(
    string Nome,
    string Status,
    bool PodeAvaliar,
    string Descricao,
    string Tutor,
    string Categoria,
    string Nivel,
    string CargaHoraria,
    IReadOnlyList<string> PontosDeAtencao,
    string TituloAbaMateriais,
    IReadOnlyList<MaterialExibicao> Materiais,
    string TituloAbaProva,
    ProvaExibicao? Prova);

public sealed record MaterialExibicao(
    string Titulo,
    string Tipo,
    bool SemConteudo,
    IReadOnlyList<BlocoConteudo> Blocos);

public sealed record ProvaExibicao(
    string Titulo,
    string Resumo,
    IReadOnlyList<QuestaoExibicao> Questoes);

public sealed record QuestaoExibicao(
    string Cabecalho,
    string Enunciado,
    string? Codigo,
    bool SemGabarito,
    IReadOnlyList<AlternativaExibicao> Alternativas);

public sealed record AlternativaExibicao(string Letra, string Texto, bool Correta);
