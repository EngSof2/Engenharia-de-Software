using ES2.Models;

namespace ES2.Repositories.Interfaces;

public interface IReciboRepository : IGenericRepository<Recibo>
{
    Task<IEnumerable<Recibo>> GetByUtilizadorAsync(int utilizadorId);
}