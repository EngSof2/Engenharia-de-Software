using ES2.Models;

namespace ES2.Repositories.Interfaces;

public interface IAtividadeRepository : IGenericRepository<Atividade>
{
    Task<IEnumerable<Atividade>> GetByEventoAsync(int eventoId);
    Task<IEnumerable<Utilizador>> GetParticipantesByAtividadeAsync(int atividadeId);
}