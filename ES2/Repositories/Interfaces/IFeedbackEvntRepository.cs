using ES2.Models;

namespace ES2.Repositories.Interfaces;

public interface IFeedbackEvntRepository : IGenericRepository<FeedbackEvnt>
{
    Task<IEnumerable<FeedbackEvnt>> GetByEventoAsync(int eventoId);
    Task<IEnumerable<FeedbackEvnt>> GetByUtilizadorAsync(int utilizadorId);

    /// <summary>Feedbacks de um evento com o utilizador autor incluido, do mais recente para o mais antigo.</summary>
    Task<IReadOnlyList<FeedbackEvnt>> GetByEventoComUtilizadorAsync(int eventoId);
}