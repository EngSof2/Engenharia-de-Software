using ES2.Models;

namespace ES2.Repositories.Interfaces;

public interface ITipoUtilizadorRepository : IGenericRepository<TipoUtilizador>
{
    Task<TipoUtilizador?> GetByNomeAsync(string nome);
}