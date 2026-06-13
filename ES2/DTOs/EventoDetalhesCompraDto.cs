using ES2.Models;

namespace ES2.DTOs;

public class EventoDetalhesCompraDto
{
    public Evento Evento { get; set; } = null!;

    public IReadOnlyCollection<OfertaBilheteEventoDto> OfertasBilhete { get; set; } = Array.Empty<OfertaBilheteEventoDto>();

    public bool JaInscrito { get; set; }

    public int? IdBilheteAtivo { get; set; }

    /// <summary>Avaliacoes (estrelas + comentario) submetidas para este evento.</summary>
    public IReadOnlyList<FeedbackEvnt> Feedbacks { get; set; } = Array.Empty<FeedbackEvnt>();

    /// <summary>Media das classificacoes em estrelas (0 quando ainda nao ha avaliacoes).</summary>
    public double MediaClassificacao { get; set; }

    /// <summary>Numero total de avaliacoes submetidas.</summary>
    public int TotalFeedbacks => Feedbacks.Count;
}
