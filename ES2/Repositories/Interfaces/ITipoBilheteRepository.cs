using ES2.Models;

namespace ES2.Repositories.Interfaces;

public interface ITipoBilheteRepository : IGenericRepository<TipoBilhete>
{
    Task<TipoBilhete?> GetByNomeAsync(string nome);
}