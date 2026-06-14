using System.Text.RegularExpressions;
using ES2.DTOs;

namespace ES2.Services.Inscricoes;

public static class ValidadorCheckoutPagamento
{
    private static readonly Regex TelemovelRegex = new(@"^\d{9}$", RegexOptions.Compiled);
    private static readonly Regex NumeroCartaoRegex = new(@"^\d{13,19}$", RegexOptions.Compiled);
    private static readonly Regex ValidadeCartaoRegex = new(@"^(0[1-9]|1[0-2])\/(\d{2}|\d{4})$", RegexOptions.Compiled);
    private static readonly Regex CvvRegex = new(@"^\d{3,4}$", RegexOptions.Compiled);

    public static IReadOnlyList<string> Validar(CheckoutBilheteDto dto, string? nomeMetodoPagamento)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.NomeComprador))
            erros.Add("O nome e obrigatorio.");

        if (string.IsNullOrWhiteSpace(dto.Email))
            erros.Add("O email e obrigatorio.");
        else if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(dto.Email))
            erros.Add("Indica um email valido.");

        if (string.IsNullOrWhiteSpace(dto.Morada))
            erros.Add("A morada e obrigatoria.");
        else if (dto.Morada.Length > 150)
            erros.Add("A morada nao pode ter mais de 150 caracteres.");

        if (dto.IdTipoPagamento is null or <= 0)
            erros.Add("Escolhe um metodo de pagamento.");

        var metodo = NormalizarMetodo(nomeMetodoPagamento);

        switch (metodo)
        {
            case "cartao bancario":
                ValidarCartao(dto, erros);
                ValidarTelemovel(dto.Telemovel, erros);
                break;
            case "mb way":
                ValidarTelemovel(dto.Telemovel, erros, "O telemovel MB Way deve ter 9 digitos.");
                break;
            case "paypal":
                ValidarPayPal(dto, erros);
                break;
            default:
                if (dto.IdTipoPagamento.HasValue)
                    erros.Add("Metodo de pagamento invalido.");
                ValidarTelemovel(dto.Telemovel, erros);
                break;
        }

        return erros;
    }

    private static void ValidarCartao(CheckoutBilheteDto dto, List<string> erros)
    {
        var numero = LimparDigitos(dto.NumeroCartao);
        if (string.IsNullOrWhiteSpace(numero) || !NumeroCartaoRegex.IsMatch(numero))
            erros.Add("Indica um numero de cartao valido (13 a 19 digitos).");

        if (string.IsNullOrWhiteSpace(dto.NomeTitular))
            erros.Add("O nome do titular do cartao e obrigatorio.");

        if (string.IsNullOrWhiteSpace(dto.ValidadeCartao) || !ValidadeCartaoRegex.IsMatch(dto.ValidadeCartao.Trim()))
            erros.Add("A validade do cartao deve estar no formato MM/AA ou MM/AAAA.");

        var cvv = LimparDigitos(dto.Cvv);
        if (string.IsNullOrWhiteSpace(cvv) || !CvvRegex.IsMatch(cvv))
            erros.Add("O CVV deve ter 3 ou 4 digitos.");
    }

    private static void ValidarPayPal(CheckoutBilheteDto dto, List<string> erros)
    {
        if (string.IsNullOrWhiteSpace(dto.EmailPaypal))
            erros.Add("O email PayPal e obrigatorio.");
        else if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(dto.EmailPaypal))
            erros.Add("Indica um email PayPal valido.");
    }

    private static void ValidarTelemovel(string? telemovel, List<string> erros, string? mensagem = null)
    {
        if (string.IsNullOrWhiteSpace(telemovel) || !TelemovelRegex.IsMatch(telemovel))
            erros.Add(mensagem ?? "O telemovel deve ter 9 digitos.");
    }

    private static string NormalizarMetodo(string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return string.Empty;

        return nome
            .Replace("ã", "a", StringComparison.OrdinalIgnoreCase)
            .Replace("á", "a", StringComparison.OrdinalIgnoreCase)
            .Trim()
            .ToLowerInvariant();
    }

    private static string LimparDigitos(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return new string(value.Where(char.IsDigit).ToArray());
    }
}
