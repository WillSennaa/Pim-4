using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace TechQuest.API.Middlewares;

/// <summary>
/// Captura qualquer excecao nao tratada e devolve uma resposta JSON
/// padronizada.
///
/// POR QUE: sem isto, um erro inesperado devolve HTML de stack trace em
/// desenvolvimento e um 500 vazio em producao. Nos dois casos o cliente nao
/// tem o que exibir, e no primeiro o servidor entrega detalhes internos
/// (nomes de tabela, caminho de arquivo, versao de biblioteca) a quem
/// estiver do outro lado.
///
/// O detalhe tecnico so aparece em ambiente de desenvolvimento; em producao
/// vai para o log do servidor, e o cliente recebe apenas o identificador do
/// erro — que ele pode informar ao suporte para localizar a ocorrencia.
/// </summary>
public class TratamentoDeErrosMiddleware
{
    private readonly RequestDelegate _proximo;
    private readonly ILogger<TratamentoDeErrosMiddleware> _log;
    private readonly IHostEnvironment _ambiente;

    public TratamentoDeErrosMiddleware(
        RequestDelegate proximo,
        ILogger<TratamentoDeErrosMiddleware> log,
        IHostEnvironment ambiente)
    {
        _proximo = proximo;
        _log = log;
        _ambiente = ambiente;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _proximo(contexto);
        }
        catch (OperationCanceledException) when (contexto.RequestAborted.IsCancellationRequested)
        {
            // O cliente desistiu da requisicao (fechou a aba, perdeu a rede).
            // Nao e erro do servidor e nao deve poluir o log.
            _log.LogInformation("Requisicao cancelada pelo cliente: {Caminho}", contexto.Request.Path);
        }
        catch (Exception ex)
        {
            await ResponderAsync(contexto, ex);
        }
    }

    private async Task ResponderAsync(HttpContext contexto, Exception ex)
    {
        var identificador = contexto.TraceIdentifier;

        var (status, mensagem) = Classificar(ex);

        _log.LogError(ex,
            "Erro {Status} em {Metodo} {Caminho} (id {Id})",
            (int)status, contexto.Request.Method, contexto.Request.Path, identificador);

        // Se a resposta ja comecou a ser enviada, nao da para trocar o status.
        if (contexto.Response.HasStarted) return;

        contexto.Response.Clear();
        contexto.Response.StatusCode = (int)status;
        contexto.Response.ContentType = "application/json";

        var corpo = new
        {
            mensagem,
            identificador,
            detalhe = _ambiente.IsDevelopment() ? ex.ToString() : null
        };

        await contexto.Response.WriteAsync(JsonSerializer.Serialize(corpo,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            }));
    }

    /// <summary>
    /// Traduz a excecao para um status HTTP com significado. Violacao de chave
    /// estrangeira, por exemplo, e conflito de dados (409) e nao falha do
    /// servidor (500) — a diferenca importa para o cliente saber se adianta
    /// tentar de novo.
    /// </summary>
    private static (HttpStatusCode, string) Classificar(Exception ex) => ex switch
    {
        DbUpdateConcurrencyException =>
            (HttpStatusCode.Conflict,
             "O registro foi alterado por outra pessoa. Recarregue e tente novamente."),

        DbUpdateException =>
            (HttpStatusCode.Conflict,
             "Nao foi possivel gravar: a operacao conflita com dados ja existentes."),

        ArgumentException or InvalidOperationException =>
            (HttpStatusCode.BadRequest,
             "Requisicao invalida."),

        UnauthorizedAccessException =>
            (HttpStatusCode.Forbidden,
             "Operacao nao permitida."),

        TimeoutException =>
            (HttpStatusCode.ServiceUnavailable,
             "O servico demorou para responder. Tente novamente em instantes."),

        _ => (HttpStatusCode.InternalServerError,
              "Erro interno. Informe o identificador abaixo ao suporte.")
    };
}
