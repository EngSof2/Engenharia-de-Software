using ES2.Models;

namespace ES2.Repositories.Interfaces;

public interface IBilheteUtilRepository : IGenericRepository<BilheteUtil>
{
    Task<IEnumerable<BilheteUtil>> GetByUtilizadorAsync(int utilizadorId);
    Task<IEnumerable<BilheteUtil>> GetByBilheteEventoAsync(int bilheteEventoId);
}