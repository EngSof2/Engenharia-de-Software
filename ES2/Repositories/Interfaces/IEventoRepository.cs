using ES2.Models;

namespace ES2.Repositories.Interfaces;

public interface IEventoRepository : IGenericRepository<Evento>
{
    Task<IEnumerable<Evento>> SearchAsync(string? nome, DateTime? data, string? local, int? categoriaId);
    Task<IEnumerable<Evento>> GetEventosComParticipantesAsync();
}