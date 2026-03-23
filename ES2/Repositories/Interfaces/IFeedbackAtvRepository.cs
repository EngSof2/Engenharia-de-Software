using ES2.Models;

namespace ES2.Repositories.Interfaces;

public interface IFeedbackAtvRepository : IGenericRepository<FeedbackAtv>
{
    Task<IEnumerable<FeedbackAtv>> GetByAtividadeAsync(int atividadeId);
    Task<IEnumerable<FeedbackAtv>> GetByUtilizadorAsync(int utilizadorId);
}