namespace TechQuest.Domain.Regras;

/// <summary>
/// XP e nivel do estudante sao GRANDEZAS DERIVADAS, calculadas a partir do que
/// ja esta persistido (Progresso, Desempenho, Conquista). Nao existe coluna de
/// XP no banco.
///
/// POR QUE: o valor nunca pode divergir do que o aluno realmente fez, e nenhuma
/// rotina precisa manter contador em sincronia a cada acao.
/// ALTERNATIVA REJEITADA: colunas XP e Nivel na tabela Estudante. Leitura mais
/// rapida, porem cria estado duplicado que sai de sincronia no primeiro erro de
/// atualizacao, e exigiria alterar a tabela do PIM III.
///
/// Classe estatica e sem dependencias: regra de negocio pura, testavel isolada.
/// </summary>
public static class RegrasGamificacao
{
    public const int XpPorMaterialConcluido = 50;
    public const int XpPorProvaAprovada = 100;
    public const int BonusNotaMaxima = 50;
    public const int XpPorMedalha = 75;
    public const int XpPorNivel = 350;

    public static int CalcularXp(
        int materiaisConcluidos,
        int provasAprovadas,
        int provasComNotaMaxima,
        int medalhasConquistadas)
        => materiaisConcluidos * XpPorMaterialConcluido
         + provasAprovadas * XpPorProvaAprovada
         + provasComNotaMaxima * BonusNotaMaxima
         + medalhasConquistadas * XpPorMedalha;

    /// <summary>Nivel 1 e o inicial; cada nivel seguinte custa XpPorNivel.</summary>
    public static int CalcularNivel(int xp) => 1 + (xp / XpPorNivel);

    public static int XpDoProximoNivel(int xp) => CalcularNivel(xp) * XpPorNivel;

    /// <summary>Percentual percorrido dentro do nivel atual (0 a 100).</summary>
    public static int ProgressoNoNivel(int xp)
    {
        var xpNoNivel = xp % XpPorNivel;
        return (int)Math.Round(xpNoNivel * 100.0 / XpPorNivel);
    }
}
