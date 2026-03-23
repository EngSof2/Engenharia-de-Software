using ES2.Data;
using ES2.Models;
using ES2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ES2.Repositories;

public class TipoPagamentoRepository : ITipoPagamentoRepository
{
    private readonly AppDbContext _context;

    public TipoPagamentoRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<TipoPagamento>> GetAllAsync() =>
        await _context.TipoPagamentos.ToListAsync();

    public async Task<TipoPagamento?> GetByIdAsync(int id) =>
        await _context.TipoPagamentos.FindAsync(id);

    public async Task<TipoPagamento?> GetByNomeAsync(string nome) =>
        await _context.TipoPagamentos.FirstOrDefaultAsync(t => t.Nome == nome);

    public async Task AddAsync(TipoPagamento tipoPagamento)
    {
        await _context.TipoPagamentos.AddAsync(tipoPagamento);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TipoPagamento tipoPagamento)
    {
        _context.TipoPagamentos.Update(tipoPagamento);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var tipoPagamento = await GetByIdAsync(id);
        if (tipoPagamento != null)
        {
            _context.TipoPagamentos.Remove(tipoPagamento);
            await _context.SaveChangesAsync();
        }
    }
}