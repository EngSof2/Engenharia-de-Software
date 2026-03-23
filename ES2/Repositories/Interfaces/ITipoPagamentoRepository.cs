using ES2.Models;

namespace ES2.Repositories.Interfaces;

public interface ITipoPagamentoRepository : IGenericRepository<TipoPagamento>
{
    Task<TipoPagamento?> GetByNomeAsync(string nome);
}