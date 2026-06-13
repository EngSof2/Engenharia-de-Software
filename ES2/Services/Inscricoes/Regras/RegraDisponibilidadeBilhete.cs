namespace ES2.Services.Inscricoes.Regras;

public class RegraDisponibilidadeBilhete : IRegraInscricaoEvento
{
    public Task<string?> ValidarAsync(InscricaoEventoContexto contexto)
    {
        return Task.FromResult<string?>(
            contexto.BilheteEvento.IsCancelado || contexto.BilheteEvento.IdEventoNavigation.IsCancelado
                ? "Este evento ou bilhete esta cancelado."
                : contexto.BilheteEvento.QuantidadeDisponivel <= 0
                ? "Este tipo de bilhete esta esgotado para este evento."
                : null);
    }
}
