using ES2.Models;

namespace ES2.Repositories.Interfaces;

public interface ICodigoPostalRepository : IGenericRepository<CodigoPostal>
{
    Task<CodigoPostal?> GetByCodPostalAsync(string codPostal);
}