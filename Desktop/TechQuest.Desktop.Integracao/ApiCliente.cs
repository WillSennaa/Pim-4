using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TechQuest.Desktop.Aplicacao.Erros;
using TechQuest.Desktop.Aplicacao.Interfaces;

namespace TechQuest.Desktop.Integracao;

/// <summary>
/// Unico ponto do desktop que fala HTTP com a API do Tech Quest.
///
/// RESPONSABILIDADES, e so estas:
///   1. anexar o token JWT a cada requisicao;
///   2. serializar e desserializar JSON;
///   3. traduzir status HTTP em excecoes com significado para a aplicacao.
/// Os gateways (um por area da API) usam esta classe e nao repetem nada disso.
///
/// HTTPCLIENT UNICO, recebido pronto: criar um HttpClient por requisicao
/// esgota as portas do sistema operacional, porque cada instancia descartada
/// deixa a conexao em TIME_WAIT por minutos. A documentacao da Microsoft
/// recomenda uma instancia longeva; ela e criada em Program.cs.
///
/// ALTERNATIVA REJEITADA: IHttpClientFactory. Resolve o mesmo problema, mas
/// vem do pacote Microsoft.Extensions.Http e de um container de injecao. Para
/// uma aplicacao que fala com um unico servidor, uma instancia compartilhada
/// e suficiente e dispensa dependencias.
/// </summary>
public sealed class ApiCliente
{
    private readonly HttpClient _http;
    private readonly IProvedorDeToken _sessao;

    /// <summary>
    /// JsonSerializerDefaults.Web: nomes em camelCase e leitura sem
    /// diferenciar maiusculas, o mesmo padrao que o ASP.NET Core usa ao
    /// responder. JsonStringEnumConverter aceita o enum tanto como numero
    /// (como a API envia hoje) quanto como texto, se um dia mudar.
    /// </summary>
    internal static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public ApiCliente(HttpClient http, IProvedorDeToken sessao)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _sessao = sessao ?? throw new ArgumentNullException(nameof(sessao));
    }

    // ------------------------------------------------------------------
    // Verbos. "rota" e relativa, sem barra inicial: "api/admin/resumo".
    // ------------------------------------------------------------------

    public Task<T> GetAsync<T>(string rota, CancellationToken ct = default)
        => EnviarAsync<T>(HttpMethod.Get, rota, null, ct);

    public Task<T> PostAsync<T>(string rota, object? corpo, CancellationToken ct = default)
        => EnviarAsync<T>(HttpMethod.Post, rota, corpo, ct);

    public Task PostAsync(string rota, object? corpo = null, CancellationToken ct = default)
        => EnviarSemRetornoAsync(HttpMethod.Post, rota, corpo, ct);

    public Task<T> PutAsync<T>(string rota, object? corpo, CancellationToken ct = default)
        => EnviarAsync<T>(HttpMethod.Put, rota, corpo, ct);

    /// <summary>PUT cuja resposta e 204 (sem corpo), como a troca de senha.</summary>
    public Task PutSemRetornoAsync(string rota, object? corpo, CancellationToken ct = default)
        => EnviarSemRetornoAsync(HttpMethod.Put, rota, corpo, ct);

    public Task PatchAsync(string rota, object? corpo, CancellationToken ct = default)
        => EnviarSemRetornoAsync(HttpMethod.Patch, rota, corpo, ct);

    // ------------------------------------------------------------------

    private async Task<T> EnviarAsync<T>(HttpMethod metodo, string rota, object? corpo, CancellationToken ct)
    {
        using var resposta = await ExecutarAsync(metodo, rota, corpo, ct);

        try
        {
            var valor = await resposta.Content.ReadFromJsonAsync<T>(Json, ct);
            return valor ?? throw new ServidorIndisponivelException(
                "O servidor respondeu sem conteúdo onde era esperado um resultado.");
        }
        catch (JsonException ex)
        {
            // Contrato quebrado: a API mudou o formato e este cliente nao
            // acompanhou. Nao e culpa do usuario, entao a mensagem diz isso.
            throw new ServidorIndisponivelException(
                "A resposta do servidor veio num formato inesperado. " +
                "Verifique se o desktop está atualizado com a versão da API.", ex);
        }
    }

    private async Task EnviarSemRetornoAsync(HttpMethod metodo, string rota, object? corpo, CancellationToken ct)
    {
        using var _ = await ExecutarAsync(metodo, rota, corpo, ct);
    }

    /// <summary>
    /// Envia e devolve a resposta SO se for 2xx. Qualquer outro desfecho vira
    /// excecao aqui, num lugar so.
    /// </summary>
    private async Task<HttpResponseMessage> ExecutarAsync(
        HttpMethod metodo, string rota, object? corpo, CancellationToken ct)
    {
        using var requisicao = new HttpRequestMessage(metodo, rota);

        var token = _sessao.Token;
        if (token is not null)
            requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        if (corpo is not null)
            requisicao.Content = JsonContent.Create(corpo, corpo.GetType(), options: Json);

        HttpResponseMessage resposta;
        try
        {
            resposta = await _http.SendAsync(requisicao, ct);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            // HttpClient sinaliza tempo esgotado como cancelamento. Se nao foi
            // o chamador que cancelou, foi o Timeout.
            throw new ServidorIndisponivelException(
                "O servidor demorou para responder. Se for o primeiro acesso do dia, " +
                "o banco pode estar retomando da pausa automática; tente novamente.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new ServidorIndisponivelException(
                "Não foi possível conectar ao servidor. Verifique a conexão com a internet.", ex);
        }

        if (resposta.IsSuccessStatusCode) return resposta;

        try
        {
            throw await TraduzirAsync(resposta, ct);
        }
        finally
        {
            resposta.Dispose();
        }
    }

    /// <summary>
    /// Status HTTP para excecao. A mensagem preferida e a do proprio servidor
    /// (campo "mensagem" que AdminController.Traduzir e o middleware de erros
    /// devolvem); o texto fixo so entra quando o corpo nao trouxer nenhuma.
    /// </summary>
    private static async Task<Exception> TraduzirAsync(HttpResponseMessage resposta, CancellationToken ct)
    {
        var doServidor = await LerMensagemAsync(resposta, ct);

        return resposta.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new NaoAutenticadoException(
                doServidor ?? "Sua sessão expirou. Entre novamente."),

            HttpStatusCode.Forbidden => new AcessoNegadoException(
                doServidor ?? "Operação não permitida para este usuário."),

            HttpStatusCode.NotFound => new RegistroNaoEncontradoException(
                doServidor ?? "Registro não encontrado. Ele pode ter sido alterado por outra pessoa."),

            HttpStatusCode.BadRequest or HttpStatusCode.Conflict => new OperacaoRecusadaException(
                doServidor ?? "O servidor recusou a operação."),

            _ => new ServidorIndisponivelException(
                (doServidor ?? "Erro no servidor.") + $" (HTTP {(int)resposta.StatusCode})")
        };
    }

    private static async Task<string?> LerMensagemAsync(HttpResponseMessage resposta, CancellationToken ct)
    {
        try
        {
            var corpo = await resposta.Content.ReadFromJsonAsync<CorpoDeErro>(Json, ct);
            return string.IsNullOrWhiteSpace(corpo?.Mensagem) ? null : corpo.Mensagem;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException)
        {
            // Corpo vazio ou nao-JSON (ex.: 401 do proprio JwtBearer, que nao
            // traz corpo). Fica a mensagem padrao do status.
            return null;
        }
    }

    private sealed record CorpoDeErro(string? Mensagem);
}
