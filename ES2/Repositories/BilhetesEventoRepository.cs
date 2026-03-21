using ES2.Data;
using ES2.Models;
using ES2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ES2.Repositories;

public class BilhetesEventoRepository : IBilhetesEventoRepository
{
    private readonly AppDbContext _context;

    public BilhetesEventoRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<BilhetesEvento>> GetAllAsync() =>
        await _context.BilhetesEventos.ToListAsync();

    public async Task<BilhetesEvento?> GetByIdAsync(int id) =>
        await _context.BilhetesEventos.FindAsync(id);

    public async Task<IEnumerable<BilhetesEvento>> GetByEventoAsync(int eventoId) =>
        await _context.BilhetesEventos
            .Where(be => be.IdEvento == eventoId)
            .ToListAsync();

    public async Task AddAsync(BilhetesEvento bilhetesEvento)
    {
        await _context.BilhetesEventos.AddAsync(bilhetesEvento);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(BilhetesEvento bilhetesEvento)
    {
        _context.BilhetesEventos.Update(bilhetesEvento);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var bilhetesEvento = await GetByIdAsync(id);
        if (bilhetesEvento != null)
        {
            _context.BilhetesEventos.Remove(bilhetesEvento);
            await _context.SaveChangesAsync();
        }
    }
}