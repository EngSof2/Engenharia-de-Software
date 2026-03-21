using ES2.Data;
using ES2.Models;
using ES2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ES2.Repositories;

public class BilheteRepository : IBilheteRepository
{
    private readonly AppDbContext _context;

    public BilheteRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Bilhete>> GetAllAsync() =>
        await _context.Bilhetes.ToListAsync();

    public async Task<Bilhete?> GetByIdAsync(int id) =>
        await _context.Bilhetes.FindAsync(id);

    public async Task AddAsync(Bilhete bilhete)
    {
        await _context.Bilhetes.AddAsync(bilhete);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Bilhete bilhete)
    {
        _context.Bilhetes.Update(bilhete);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var bilhete = await GetByIdAsync(id);
        if (bilhete != null)
        {
            _context.Bilhetes.Remove(bilhete);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Bilhete>> GetByEventoAsync(int eventoId) =>
        await _context.BilhetesEventos
            .Where(be => be.IdEvento == eventoId)
            .Select(be => be.IdBilheteNavigation)
            .ToListAsync();

    public async Task<int> GetDisponiveisAsync(int bilhetesEventoId) =>
        await _context.BilhetesEventos
            .Where(be => be.IdBiEv == bilhetesEventoId)
            .CountAsync();
}