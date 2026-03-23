using ES2.Data;
using ES2.Models;
using ES2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ES2.Repositories;

public class BilheteUtilRepository : IBilheteUtilRepository
{
    private readonly AppDbContext _context;

    public BilheteUtilRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<BilheteUtil>> GetAllAsync() =>
        await _context.BilheteUtils.ToListAsync();

    public async Task<BilheteUtil?> GetByIdAsync(int id) =>
        await _context.BilheteUtils.FindAsync(id);

    public async Task<IEnumerable<BilheteUtil>> GetByUtilizadorAsync(int utilizadorId) =>
        await _context.BilheteUtils
            .Where(bu => bu.IdUtilizador == utilizadorId)
            .ToListAsync();

    public async Task<IEnumerable<BilheteUtil>> GetByBilheteEventoAsync(int bilheteEventoId) =>
        await _context.BilheteUtils
            .Where(bu => bu.IdBiEv == bilheteEventoId)
            .ToListAsync();

    public async Task AddAsync(BilheteUtil bilheteUtil)
    {
        await _context.BilheteUtils.AddAsync(bilheteUtil);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(BilheteUtil bilheteUtil)
    {
        _context.BilheteUtils.Update(bilheteUtil);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var bilheteUtil = await GetByIdAsync(id);
        if (bilheteUtil != null)
        {
            _context.BilheteUtils.Remove(bilheteUtil);
            await _context.SaveChangesAsync();
        }
    }
}