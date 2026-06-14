using ES2.DTOs;
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

    [HttpGet("checkout/{id:int}")]
    public async Task<IActionResult> ObterCheckout(int id)
    {
        var nomeUtilizador = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(nomeUtilizador))
            return Unauthorized();

        var checkout = await _inscricaoEventoService.ObterCheckoutAsync(id, nomeUtilizador);
        if (checkout == null)
            return NotFound(new { message = "Bilhete nao encontrado." });

        return Ok(MapCheckoutResponse(checkout));
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Comprar([FromBody] CheckoutBilheteDto dto)
    {
        var nomeUtilizador = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(nomeUtilizador))
            return Unauthorized();

        var checkout = await _inscricaoEventoService.ObterCheckoutAsync(dto.IdBilheteEvento, nomeUtilizador);
        if (checkout == null)
            return NotFound(new { message = "Bilhete nao encontrado." });

        dto.TiposPagamento = checkout.TiposPagamento;
        var metodo = checkout.TiposPagamento
            .FirstOrDefault(tp => tp.IdTipoPagamento == dto.IdTipoPagamento)?.Nome;

        var erros = ValidadorCheckoutPagamento.Validar(dto, metodo);
        if (erros.Count > 0)
            return BadRequest(new { message = erros[0], errors = erros });

        var resultado = await _inscricaoEventoService.ComprarAsync(dto, nomeUtilizador);
        if (!resultado.Sucesso)
            return BadRequest(new { message = resultado.Mensagem });

        return Ok(new { message = resultado.Mensagem });
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

    private static object MapCheckoutResponse(CheckoutBilheteDto checkout) => new
    {
        idBilheteEvento = checkout.IdBilheteEvento,
        idEvento = checkout.IdEvento,
        nomeEvento = checkout.NomeEvento,
        dataEvento = checkout.DataEvento?.ToString("dd/MM/yyyy"),
        horaEvento = checkout.HoraEvento?.ToString("HH:mm"),
        localEvento = checkout.LocalEvento,
        nomeBilhete = checkout.NomeBilhete,
        tipoBilhete = checkout.TipoBilhete,
        descricaoAcesso = checkout.DescricaoAcesso,
        preco = checkout.Preco,
        quantidadeDisponivel = checkout.QuantidadeDisponivel,
        nomeComprador = checkout.NomeComprador,
        email = checkout.Email,
        telemovel = checkout.Telemovel,
        morada = checkout.Morada,
        tiposPagamento = checkout.TiposPagamento.Select(tp => new
        {
            idTipoPagamento = tp.IdTipoPagamento,
            nome = tp.Nome
        })
    };
}
