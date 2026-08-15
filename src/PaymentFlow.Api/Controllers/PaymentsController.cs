using Microsoft.AspNetCore.Mvc;

namespace PaymentFlow.Api.Controllers;

/// <summary>
/// Controller de testes destinado a validar o pipeline de observabilidade e telemetria.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class PaymentsController : ControllerBase
{
    /// <summary>
    /// Simula o processamento de um pagamento que resulta em uma falha crítica não tratada (HTTP 500).
    /// </summary>
    /// <returns>Retorna a resposta padronizada via RFC 7807 (Problem Details).</returns>
    [HttpPost("process")]
    public IActionResult ProcessPayment()
    {
        string? customerCardToken = null;

        // Força uma NullReferenceException para validar a captura pelo SentinelExceptionMiddleware
        // e garantir o despacho assíncrono do alerta para o webhook em produção.
        _ = customerCardToken!.Length;

        return Ok(new { status = "Payment approved" });
    }
}