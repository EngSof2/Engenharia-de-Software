using ES2.Data;
using ES2.Models;
using ES2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ES2.Repositories;

public class TipoUtilizadorRepository : ITipoUtilizadorRepository
{
    private readonly AppDbContext _context;

    public TipoUtilizadorRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<TipoUtilizador>> GetAllAsync() =>
        await _context.TipoUtilizadors.ToListAsync();

    public async Task<TipoUtilizador?> GetByIdAsync(int id) =>
        await _context.TipoUtilizadors.FindAsync(id);

    public async Task<TipoUtilizador?> GetByNomeAsync(string nome) =>
        await _context.TipoUtilizadors.FirstOrDefaultAsync(t => t.Nome == nome);

    public async Task AddAsync(TipoUtilizador tipoUtilizador)
    {
        await _context.TipoUtilizadors.AddAsync(tipoUtilizador);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(TipoUtilizador tipoUtilizador)
    {
        _context.TipoUtilizadors.Update(tipoUtilizador);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var tipoUtilizador = await GetByIdAsync(id);
        if (tipoUtilizador != null)
        {
            _context.TipoUtilizadors.Remove(tipoUtilizador);
            await _context.SaveChangesAsync();
        }
    }
}