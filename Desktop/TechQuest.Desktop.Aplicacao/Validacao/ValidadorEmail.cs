using System.Text.RegularExpressions;

namespace TechQuest.Desktop.Aplicacao.Validacao;

/// <summary>
/// Conferencia de FORMATO de e-mail, feita antes de ir a rede.
///
/// POR QUE VALIDAR NO CLIENTE SE O SERVIDOR VALIDA: nao e por seguranca (o
/// servidor continua sendo a autoridade), e por resposta imediata. Um erro
/// de digitacao aparece na hora, sem esperar a API, que no Azure serverless
/// pode levar segundos para acordar.
///
/// A regra e propositalmente simples: algo@algo.algo, sem espacos. Regex que
/// tenta cobrir a RFC 5322 inteira rejeita enderecos validos e nao acrescenta
/// nada que a propria API ja nao confira.
/// </summary>
public static partial class ValidadorEmail
{
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex Formato();

    public static bool EhValido(string? email)
        => !string.IsNullOrWhiteSpace(email) && Formato().IsMatch(email.Trim());
}
