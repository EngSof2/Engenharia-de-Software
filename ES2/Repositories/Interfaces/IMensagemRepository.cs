using ES2.Models;

namespace ES2.Repositories.Interfaces;

public interface IMensagemRepository : IGenericRepository<Mensagem>
{
    Task<IEnumerable<Mensagem>> GetByRecetorAsync(int utilizadorId);
}