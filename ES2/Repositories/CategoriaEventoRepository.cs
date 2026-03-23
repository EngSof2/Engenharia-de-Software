using ES2.Data;
using ES2.Models;
using ES2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ES2.Repositories;

public class CategoriaEventoRepository : ICategoriaEventoRepository
{
    private readonly AppDbContext _context;

    public CategoriaEventoRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<CategoriaEvento>> GetAllAsync() =>
        await _context.CategoriaEventos.ToListAsync();

    public async Task<CategoriaEvento?> GetByIdAsync(int id) =>
        await _context.CategoriaEventos.FindAsync(id);

    public async Task<IEnumerable<CategoriaEvento>> GetByEventoAsync(int eventoId) =>
        await _context.CategoriaEventos
            .Where(ce => ce.IdEvento == eventoId)
            .ToListAsync();

    public async Task<IEnumerable<CategoriaEvento>> GetByCategoriaAsync(int categoriaId) =>
        await _context.CategoriaEventos
            .Where(ce => ce.IdCategoria == categoriaId)
            .ToListAsync();

    public async Task AddAsync(CategoriaEvento categoriaEvento)
    {
        await _context.CategoriaEventos.AddAsync(categoriaEvento);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(CategoriaEvento categoriaEvento)
    {
        _context.CategoriaEventos.Update(categoriaEvento);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var categoriaEvento = await GetByIdAsync(id);
        if (categoriaEvento != null)
        {
            _context.CategoriaEventos.Remove(categoriaEvento);
            await _context.SaveChangesAsync();
        }
    }
}