using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentFlow.Diagnostics.Middlewares;
using PaymentFlow.Diagnostics.Options;
using PaymentFlow.Diagnostics.Services;

namespace PaymentFlow.Diagnostics.Extensions;

/// <summary>
/// Fornece métodos de extensão (Extension Methods) para facilitar a integração 
/// do módulo de observabilidade Sentinel no arquivo Program.cs de aplicações ASP.NET Core.
/// </summary>
public static class SentinelServiceExtensions
{
    /// <summary>
    /// Registra os serviços necessários para o funcionamento da telemetria de erros 
    /// no contêiner de Injeção de Dependências (DI).
    /// </summary>
    /// <param name="services">A coleção de serviços do ASP.NET Core.</param>
    /// <param name="configuration">A interface de configuração para acessar o appsettings.json.</param>
    /// <returns>A própria coleção de serviços para encadeamento de chamadas (Fluent API).</returns>
    public static IServiceCollection AddSentinelDiagnostics(this IServiceCollection services, IConfiguration configuration)
    {
        // Realiza o bind automático da seção "Sentinel" do appsettings.json para a classe SentinelOptions
        services.Configure<SentinelOptions>(configuration.GetSection(SentinelOptions.SectionName));
        
        // Configura o HttpClient responsável pelo disparo com regras de resiliência.
        // O timeout baixo (5s) garante que uma possível instabilidade na rede ou no webhook do n8n 
        // não cause esgotamento de portas (socket exhaustion) nem segure recursos do servidor .NET.
        services.AddHttpClient<ITelemetrySender, HttpTelemetrySender>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }

    /// <summary>
    /// Adiciona o middleware global de interceptação de exceções ao pipeline HTTP da aplicação.
    /// Deve ser chamado o mais cedo possível no Program.cs para capturar erros de todos os middlewares subsequentes.
    /// </summary>
    /// <param name="app">O construtor do pipeline da aplicação (IApplicationBuilder).</param>
    /// <returns>O construtor da aplicação para encadeamento de chamadas (Fluent API).</returns>
    public static IApplicationBuilder UseSentinelDiagnostics(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SentinelExceptionMiddleware>();
    }
}