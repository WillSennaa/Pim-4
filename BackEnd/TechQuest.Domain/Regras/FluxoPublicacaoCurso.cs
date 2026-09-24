namespace TechQuest.Domain.Regras;

/// <summary>
/// Maquina de estados da publicacao de um curso.
///
/// A coluna Status_Curso do PIM III e um VARCHAR livre: nada no banco impede
/// gravar "Publicaldo" ou pular de Rascunho direto para Publicado. Centralizar
/// as transicoes validas aqui transforma texto livre em regra verificavel, sem
/// alterar o modelo entregue.
///
/// Rascunho --submeter--> Pendente --aprovar--> Publicado
///                            |
///                            +---rejeitar--> Rejeitado --submeter--> Pendente
/// </summary>
public static class FluxoPublicacaoCurso
{
    public const string Rascunho = "Rascunho";
    public const string Pendente = "Pendente";
    public const string Publicado = "Publicado";
    public const string Rejeitado = "Rejeitado";

    private static readonly Dictionary<string, string[]> Permitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        [Rascunho] = new[] { Pendente },
        [Pendente] = new[] { Publicado, Rejeitado },
        [Rejeitado] = new[] { Pendente },
        [Publicado] = Array.Empty<string>()
    };

    public static bool PodeTransitar(string? de, string para)
        => de is not null
           && Permitidas.TryGetValue(de, out var destinos)
           && destinos.Contains(para, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Conteudo so pode ser alterado antes da publicacao: mexer em aulas ou
    /// questoes de um curso publicado mudaria o material debaixo de alunos
    /// que ja estao matriculados e de provas ja respondidas.
    /// </summary>
    public static bool PermiteEdicao(string? status)
        => string.Equals(status, Rascunho, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, Rejeitado, StringComparison.OrdinalIgnoreCase);
}
