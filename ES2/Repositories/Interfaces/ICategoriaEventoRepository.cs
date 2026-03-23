using ES2.Models;

namespace ES2.Repositories.Interfaces;

public interface ICategoriaEventoRepository : IGenericRepository<CategoriaEvento>
{
    Task<IEnumerable<CategoriaEvento>> GetByEventoAsync(int eventoId);
    Task<IEnumerable<CategoriaEvento>> GetByCategoriaAsync(int categoriaId);
}