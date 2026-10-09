using System.Text.Json;

namespace TechQuest.Desktop.UI.Configuracao;

/// <summary>
/// Le o appsettings.json que acompanha o executavel.
///
/// POR QUE ARQUIVO E NAO CONSTANTE NO CODIGO: a mesma compilacao precisa
/// apontar para a API no Azure (demonstracao) ou para a API rodando no
/// Visual Studio (https://localhost:porta/) durante o desenvolvimento.
/// Com a URL em arquivo, isso e trocar uma linha, sem recompilar.
///
/// POR QUE System.Text.Json DIRETO E NAO Microsoft.Extensions.Configuration:
/// o desktop le duas chaves. O pacote de configuracao resolveria o mesmo
/// com mais recursos (variavel de ambiente, user secrets), mas traria
/// dependencias de NuGet para uma necessidade que cabe em vinte linhas.
/// Aqui nao ha segredo a proteger: a URL da API e publica.
/// </summary>
internal sealed class ConfiguracaoDesktop
{
    public required Uri UrlBaseApi { get; init; }
    public required TimeSpan Timeout { get; init; }

    public static ConfiguracaoDesktop Carregar()
    {
        var caminho = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(caminho))
            throw new InvalidOperationException($"Arquivo de configuração não encontrado:\n{caminho}");

        ArquivoJson? arquivo;
        try
        {
            arquivo = JsonSerializer.Deserialize<ArquivoJson>(
                File.ReadAllText(caminho),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"appsettings.json inválido: {ex.Message}", ex);
        }

        var url = arquivo?.Api?.UrlBase;
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
            throw new InvalidOperationException("Informe Api:UrlBase com um endereço completo no appsettings.json.");

        // Sem a barra final, new Uri(base, "api/x") DESCARTA o ultimo
        // segmento da base. Garantir aqui evita um erro dificil de enxergar.
        if (!uri.AbsoluteUri.EndsWith('/'))
            uri = new Uri(uri.AbsoluteUri + "/");

        var segundos = arquivo!.Api!.TimeoutSegundos is > 0 and var s ? s : 60;

        return new ConfiguracaoDesktop
        {
            UrlBaseApi = uri,
            Timeout = TimeSpan.FromSeconds(segundos)
        };
    }

    private sealed class ArquivoJson
    {
        public SecaoApi? Api { get; set; }
    }

    private sealed class SecaoApi
    {
        public string? UrlBase { get; set; }
        public int TimeoutSegundos { get; set; }
    }
}
