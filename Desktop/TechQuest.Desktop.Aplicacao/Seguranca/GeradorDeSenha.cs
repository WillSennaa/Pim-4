using System.Security.Cryptography;

namespace TechQuest.Desktop.Aplicacao.Seguranca;

/// <summary>
/// Senha inicial para contas criadas pelo administrador.
///
/// POR QUE RandomNumberGenerator E NAO Random: Random (e o Math.random do
/// web) e previsivel: a sequencia sai de uma semente, e quem conhece a
/// semente reproduz as senhas. RandomNumberGenerator usa o gerador
/// criptografico do sistema operacional, feito para segredos.
///
/// O alfabeto exclui caracteres que se confundem ao ditar ou ler (0/O,
/// 1/l/I): a senha vai ser repassada a pessoa pelo admin.
/// </summary>
public static class GeradorDeSenha
{
    private const string Letras = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz";
    private const string Digitos = "23456789";
    public const int Tamanho = 12;

    public static string Gerar()
    {
        // Garante ao menos uma letra e um digito, e sorteia o resto do
        // alfabeto completo. Depois embaralha, para os garantidos nao ficarem
        // sempre nas mesmas posicoes.
        var alfabeto = Letras + Digitos;
        var chars = new char[Tamanho];
        chars[0] = Letras[RandomNumberGenerator.GetInt32(Letras.Length)];
        chars[1] = Digitos[RandomNumberGenerator.GetInt32(Digitos.Length)];
        for (var i = 2; i < Tamanho; i++)
            chars[i] = alfabeto[RandomNumberGenerator.GetInt32(alfabeto.Length)];

        RandomNumberGenerator.Shuffle(chars.AsSpan());
        return new string(chars);
    }
}
