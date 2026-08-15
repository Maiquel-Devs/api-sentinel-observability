namespace PaymentFlow.Diagnostics.Options;

/// <summary>
/// Representa as opções de configuração para o módulo de observabilidade (Sentinel).
/// As propriedades desta classe são populadas automaticamente a partir do appsettings.json.
/// </summary>
public class SentinelOptions
{
    /// <summary>
    /// Nome exato da seção no appsettings.json onde estas configurações devem ser declaradas.
    /// </summary>
    public const string SectionName = "Sentinel";

    /// <summary>
    /// URL de destino (Webhook do n8n) para onde o payload de erro será disparado.
    /// </summary>
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>
    /// Nome amigável do serviço que está emitindo o alerta (ex: "PaymentFlow.Api").
    /// Utilizado para identificar a origem do erro no painel do Discord.
    /// </summary>
    public string ServiceName { get; set; } = "PaymentFlow.Api";

    /// <summary>
    /// Chave (flag) que ativa ou desativa globalmente o envio de telemetria.
    /// Útil para desativar temporariamente os alertas em ambiente de desenvolvimento local.
    /// </summary>
    public bool Enabled { get; set; } = true;
}