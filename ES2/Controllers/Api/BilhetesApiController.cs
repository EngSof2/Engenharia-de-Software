using ES2.Services.Inscricoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ES2.Controllers.Api;

[ApiController]
[Route("api/bilhetes")]
[Authorize]
public class BilhetesApiController : ControllerBase
{
    private readonly IInscricaoEventoService _inscricaoEventoService;

    public BilhetesApiController(IInscricaoEventoService inscricaoEventoService)
    {
        _inscricaoEventoService = inscricaoEventoService;
    }

    [HttpGet("historico")]
    public async Task<IActionResult> Historico()
    {
        var nomeUtilizador = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(nomeUtilizador))
            return Unauthorized();

        var historico = await _inscricaoEventoService.ObterHistoricoAsync(nomeUtilizador);

        return Ok(historico.Select(compra => new
        {
            recibo = $"#{compra.IdRecibo}",
            idRecibo = compra.IdRecibo,
            evento = compra.NomeEvento,
            nomeBilhete = compra.NomeBilhete,
            tipoBilhete = compra.TipoBilhete,
            acesso = compra.DescricaoAcesso,
            metodoPagamento = compra.MetodoPagamento,
            data = compra.DataCompra.ToString("dd/MM/yyyy"),
            valor = compra.ValorPago
        }));
    }
}
