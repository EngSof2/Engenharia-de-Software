using ES2.Data;
using Microsoft.EntityFrameworkCore;

namespace ES2.Services.Inscricoes.Regras;

public class RegraInscricaoDuplicadaEvento : IRegraInscricaoEvento
{
    private readonly AppDbContext _context;

    public RegraInscricaoDuplicadaEvento(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string?> ValidarAsync(InscricaoEventoContexto contexto)
    {
        var bilheteAtual = await _context.BilheteUtils
            .Where(bu => bu.IdUtilizador == contexto.Utilizador.IdUti &&
                         bu.IdBiEvNavigation != null &&
                         bu.IdBiEvNavigation.IdEvento == contexto.BilheteEvento.IdEvento &&
                         !bu.IdBiEvNavigation.IsCancelado &&
                         !bu.IdBiEvNavigation.IdEventoNavigation.IsCancelado &&
                         _context.RegistoEventos.Any(r =>
                             r.IdUti == contexto.Utilizador.IdUti &&
                             r.IdEvento == contexto.BilheteEvento.IdEvento &&
                             !r.IsCancelado))
            .OrderByDescending(bu => bu.IdBiUti)
            .Select(bu => bu.IdBiEvNavigation!.IdBilheteNavigation.IdTipoNavigation!.Nome)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(bilheteAtual))
            return null;

        var nivelAtual = NivelBilhete(bilheteAtual);
        var nivelNovo = NivelBilhete(contexto.BilheteEvento.IdBilheteNavigation.IdTipoNavigation?.Nome);

        return nivelNovo > nivelAtual
            ? null
            : "Ja tens um bilhete igual ou superior associado a este evento.";
    }

    private static int NivelBilhete(string? tipoBilhete) =>
        tipoBilhete?.Trim().ToUpperInvariant() switch
        {
            "VIP" => 3,
            "GOLD" => 2,
            "STANDARD" => 1,
            _ => 0
        };
}
