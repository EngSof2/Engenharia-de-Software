using ES2.Models;

namespace ES2.Repositories.Interfaces;

public interface IBilhetesEventoRepository : IGenericRepository<BilhetesEvento>
{
    Task<IEnumerable<BilhetesEvento>> GetByEventoAsync(int eventoId);
    Task<IEnumerable<BilhetesEvento>> GetFilteredWithDetailsAsync(string? nome, DateOnly? data, string? local, int? idTipo);
}