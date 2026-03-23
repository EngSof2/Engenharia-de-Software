using ES2.Models;

namespace ES2.Repositories.Interfaces;

public interface IRegistoAtividadeRepository : IGenericRepository<RegistoAtividade>
{
    Task<IEnumerable<RegistoAtividade>> GetByAtividadeAsync(int atividadeId);
    Task<IEnumerable<RegistoAtividade>> GetByUtilizadorAsync(int utilizadorId);
}