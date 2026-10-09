using TechQuest.Desktop.Apresentacao.Comum;

namespace TechQuest.Desktop.Apresentacao.Aprovacoes;

public interface IAprovacoesView : IViewBase
{
    void ExibirFila(IReadOnlyList<LinhaSolicitacao> linhas, string resumo);

    void OcultarErro();

    /// <summary>Id do curso selecionado na grade, ou null.</summary>
    int? CursoSelecionado { get; }

    /// <summary>
    /// Abre a revisao completa do curso numa janela modal e so retorna
    /// quando ela for fechada.
    /// </summary>
    void AbrirRevisao(int idCurso);
}

/// <summary>Linha da grade da fila, ja formatada.</summary>
public sealed record LinhaSolicitacao(
    int IdCurso,
    string Curso,
    string Tutor,
    string Categoria,
    string Nivel,
    string Aulas,
    string Prova,
    bool Incompleto);
