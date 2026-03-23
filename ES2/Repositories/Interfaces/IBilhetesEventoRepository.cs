using ES2.Models;

namespace ES2.Repositories.Interfaces;

public interface IBilhetesEventoRepository : IGenericRepository<BilhetesEvento>
{
    Task<IEnumerable<BilhetesEvento>> GetByEventoAsync(int eventoId);
}