using Microsoft.AspNetCore.Mvc;

namespace PaymentFlow.Api.Controllers;

/// <summary>
/// Controller de testes destinado a validar o fluxo padrão (Happy Path) da aplicação.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class OrdersController : ControllerBase
{
    /// <summary>
    /// Simula a recuperação de pedidos, retornando um status de sucesso (HTTP 200).
    /// </summary>
    /// <returns>Retorna uma lista estática de pedidos para confirmar que o middleware não interfere em requisições bem-sucedidas.</returns>
    [HttpGet]
    public IActionResult GetOrders()
    {
        // Retorna dados simulados em memória para validar o processamento limpo e sem erros
        return Ok(new[]
        {
            new { OrderId = 101, Amount = 150.00, Status = "Completed" },
            new { OrderId = 102, Amount = 89.90, Status = "Pending" }
        });
    }
}