using ES2.Models;

namespace ES2.Repositories.Interfaces;

public interface ICategoriaRepository : IGenericRepository<Categoria>
{
    Task<Categoria?> GetByNomeAsync(string nome);
}