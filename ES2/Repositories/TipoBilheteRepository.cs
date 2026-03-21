using ES2.Data;
using ES2.Models;
using ES2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ES2.Repositories;

public class TipoBilheteRepository : ITipoBilheteRepository
{
    private readonly AppDbContext _context;

    public TipoBilheteRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<TipoBilhete>> GetAllAsync() =>
        await _context.TipoBilhetes.ToListAsync();

    public async Task<TipoBilhete?> GetByIdAsync(int id) =>
        await _context.TipoBilhetes.FindAsync(id);

    public async Task<TipoBilhete?> GetByNomeAsync(string nome) =>
        await _context.TipoBilhetes.FirstOrDefaultAsync(t => t.Nome == nome);

    public async Task AddAsync(TipoBilhete tipoBilhete)
    {
        await _context.TipoBilhetes.AddAsync(tipoBilhete);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TipoBilhete tipoBilhete)
    {
        _context.TipoBilhetes.Update(tipoBilhete);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var tipoBilhete = await GetByIdAsync(id);
        if (tipoBilhete != null)
        {
            _context.TipoBilhetes.Remove(tipoBilhete);
            await _context.SaveChangesAsync();
        }
    }
}