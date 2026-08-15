using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PaymentFlow.Diagnostics.Models;
using PaymentFlow.Diagnostics.Options;
using PaymentFlow.Diagnostics.Services;

namespace PaymentFlow.Diagnostics.Middlewares;

/// <summary>
/// Middleware global de interceptação de exceções.
/// Captura erros não tratados no pipeline HTTP, despacha a telemetria para o pipeline de observabilidade 
/// e padroniza a resposta de erro devolvida ao cliente final.
/// </summary>
public class SentinelExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;

    /// <summary>
    /// Inicializa uma nova instância do middleware de exceções.
    /// </summary>
    /// <param name="next">O próximo delegate no pipeline de processamento HTTP.</param>
    /// <param name="env">Informações sobre o ambiente de hospedagem web atual (ex: Development, Production).</param>
    public SentinelExceptionMiddleware(RequestDelegate next, IHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    /// <summary>
    /// Método invocado automaticamente pelo runtime do ASP.NET Core a cada requisição HTTP.
    /// Dependências com escopo de requisição (Scoped/Transient) são injetadas diretamente aqui.
    /// </summary>
    /// <param name="context">O contexto HTTP da requisição atual.</param>
    /// <param name="telemetrySender">Serviço injetado para envio da telemetria.</param>
    /// <param name="options">Configurações do Sentinel vinculadas ao appsettings.</param>
    public async Task InvokeAsync(HttpContext context, ITelemetrySender telemetrySender, IOptions<SentinelOptions> options)
    {
        try
        {
            // Tenta avançar o fluxo normal da requisição para os próximos middlewares ou controllers
            await _next(context);
        }
        catch (Exception ex)
        {
            // Se qualquer exceção estourar e não for tratada pela aplicação, o fluxo cai aqui
            await HandleExceptionAsync(context, ex, telemetrySender, options.Value);
        }
    }

    /// <summary>
    /// Monta o payload de erro, despacha para a IA e constrói a resposta HTTP de falha.
    /// </summary>
    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        ITelemetrySender telemetrySender,
        SentinelOptions options)
    {
        var payload = new ErrorTelemetryPayload
        {
            ServiceName = options.ServiceName,
            Environment = _env.EnvironmentName,
            Endpoint = context.Request.Path,
            HttpMethod = context.Request.Method,
            StatusCode = (int)HttpStatusCode.InternalServerError,
            ExceptionType = exception.GetType().FullName ?? exception.GetType().Name,
            ErrorMessage = exception.Message,
            StackTrace = exception.StackTrace ?? string.Empty,
            Timestamp = DateTime.UtcNow
        };

        // ARQUITETURA DE RESILIÊNCIA: Fire-and-forget (Despacho assíncrono não-bloqueante)
        // Usamos Task.Run com o discart (_) para que a API não faça o usuário esperar a 
        // comunicação de rede com o n8n/Gemini terminar. A telemetria vai em segundo plano.
        _ = Task.Run(() => telemetrySender.SendAsync(payload));

        // PADRONIZAÇÃO DE RESPOSTA: RFC 7807 (Problem Details for HTTP APIs)
        // Garante que o cliente da API (ex: um front-end ou outro serviço) receba
        // sempre uma estrutura de erro previsível e profissional, sem expor dados sensíveis.
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var problemDetails = new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            title = "Ocorreu um erro interno ao processar a solicitação.",
            status = context.Response.StatusCode,
            instance = context.Request.Path.Value
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    }
}