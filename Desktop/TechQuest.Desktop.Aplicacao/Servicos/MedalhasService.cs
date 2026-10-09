using TechQuest.Desktop.Aplicacao.Comum;
using TechQuest.Desktop.Aplicacao.Interfaces;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Aplicacao.Servicos;

/// <summary>
/// Catalogo de medalhas (RF08): listar, criar e editar.
///
/// SEM EXCLUSAO: a API nao tem a rota, e com razao. Conquista referencia
/// Medalha; apagar uma medalha ja conquistada tiraria do perfil do estudante
/// algo que ele ganhou.
/// </summary>
public sealed class MedalhasService
{
    // Colunas Nome_Medalha VARCHAR(50) e Descricao_Medalha VARCHAR(200).
    public const int TamanhoMaximoNome = 50;
    public const int TamanhoMaximoDescricao = 200;

    private readonly IMedalhasGateway _gateway;
    public MedalhasService(IMedalhasGateway gateway) => _gateway = gateway;

    public Task<IReadOnlyList<MedalhaAdmin>> ListarAsync(CancellationToken ct = default)
        => _gateway.ListarAsync(ct);

    public async Task<ResultadoOperacao<MedalhaAdmin>> SalvarAsync(
        int? idExistente, string? nome, string? raridade, string? descricao, CancellationToken ct = default)
    {
        var n = nome?.Trim() ?? "";
        var d = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();
        var r = Raridades.Normalizar(raridade);

        if (n.Length == 0)
            return ResultadoOperacao<MedalhaAdmin>.Falha("Informe o nome da medalha.");
        if (n.Length > TamanhoMaximoNome)
            return ResultadoOperacao<MedalhaAdmin>.Falha($"O nome pode ter no máximo {TamanhoMaximoNome} caracteres.");
        if (r is null)
            return ResultadoOperacao<MedalhaAdmin>.Falha("Escolha a raridade da medalha.");
        if (d?.Length > TamanhoMaximoDescricao)
            return ResultadoOperacao<MedalhaAdmin>.Falha($"A descrição pode ter no máximo {TamanhoMaximoDescricao} caracteres.");

        var dados = new DadosMedalha(n, r, d);
        var salva = idExistente is int id
            ? await _gateway.AtualizarAsync(id, dados, ct)
            : await _gateway.CriarAsync(dados, ct);
        return ResultadoOperacao<MedalhaAdmin>.Ok(salva);
    }

    /// <summary>
    /// Verdadeiro se o nome muda de fato (ignorando espacos e maiusculas).
    ///
    /// IMPORTA PORQUE AS MEDALHAS AUTOMATICAS SAO RECONHECIDAS PELO NOME
    /// (RegrasMedalhas, no Domain do servidor): a tabela Medalha nao tem coluna
    /// de criterio. Renomear "Primeiro Passo" faz o servidor parar de concede-la.
    /// A tela usa isto para pedir confirmacao antes de renomear.
    /// </summary>
    public static bool RenomeiaMedalha(MedalhaAdmin existente, string? nomeNovo)
        => !string.Equals(existente.Nome.Trim(), nomeNovo?.Trim(), StringComparison.OrdinalIgnoreCase);
}
