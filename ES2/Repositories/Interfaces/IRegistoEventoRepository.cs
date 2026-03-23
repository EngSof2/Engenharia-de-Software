using ES2.Models;

namespace ES2.Repositories.Interfaces;

public interface IRegistoEventoRepository : IGenericRepository<RegistoEvento>
{
    Task<IEnumerable<RegistoEvento>> GetByEventoAsync(int eventoId);
    Task<IEnumerable<RegistoEvento>> GetByUtilizadorAsync(int utilizadorId);
    Task<IEnumerable<RegistoEvento>> GetAtivosAsync();
}