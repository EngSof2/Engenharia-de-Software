namespace ES2.DTOs;

public class CheckoutBilheteDto
{
    public int IdBilheteEvento { get; set; }

    public int IdEvento { get; set; }

    public string NomeEvento { get; set; } = string.Empty;

    public DateOnly? DataEvento { get; set; }

    public TimeOnly? HoraEvento { get; set; }

    public string? LocalEvento { get; set; }

    public string NomeBilhete { get; set; } = string.Empty;

    public string TipoBilhete { get; set; } = string.Empty;

    public string DescricaoAcesso { get; set; } = string.Empty;

    public decimal Preco { get; set; }

    public int QuantidadeDisponivel { get; set; }

    public string NomeComprador { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Telemovel { get; set; } = string.Empty;

    public string Morada { get; set; } = string.Empty;

    public int? IdTipoPagamento { get; set; }

    public string? NumeroCartao { get; set; }

    public string? NomeTitular { get; set; }

    public string? ValidadeCartao { get; set; }

    public string? Cvv { get; set; }

    public string? EmailPaypal { get; set; }

    public IReadOnlyCollection<OpcaoPagamentoDto> TiposPagamento { get; set; } = Array.Empty<OpcaoPagamentoDto>();
}
