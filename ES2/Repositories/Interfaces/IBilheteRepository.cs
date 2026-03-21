using ES2.Models;

namespace ES2.Repositories.Interfaces;

public interface IBilheteRepository : IGenericRepository<Bilhete>
{
    Task<IEnumerable<Bilhete>> GetByEventoAsync(int eventoId);
    Task<int> GetDisponiveisAsync(int bilhetesEventoId);
}