using ES2.Models;

namespace ES2.Repositories.Interfaces;

public interface IFeedbackEvntRepository : IGenericRepository<FeedbackEvnt>
{
    Task<IEnumerable<FeedbackEvnt>> GetByEventoAsync(int eventoId);
    Task<IEnumerable<FeedbackEvnt>> GetByUtilizadorAsync(int utilizadorId);
}