using ES2.Data;
using ES2.Models;
using ES2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ES2.Repositories;

public class ReciboRepository : IReciboRepository
{
    private readonly AppDbContext _context;

    public ReciboRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Recibo>> GetAllAsync() =>
        await _context.Recibos.ToListAsync();

    public async Task<Recibo?> GetByIdAsync(int id) =>
        await _context.Recibos.FindAsync(id);

    public async Task<IEnumerable<Recibo>> GetByUtilizadorAsync(int utilizadorId) =>
        await _context.Recibos
            .Where(r => r.IdUtilizador == utilizadorId)
            .ToListAsync();

    public async Task AddAsync(Recibo recibo)
    {
        await _context.Recibos.AddAsync(recibo);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Recibo recibo)
    {
        _context.Recibos.Update(recibo);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var recibo = await GetByIdAsync(id);
        if (recibo != null)
        {
            _context.Recibos.Remove(recibo);
            await _context.SaveChangesAsync();
        }
    }
}