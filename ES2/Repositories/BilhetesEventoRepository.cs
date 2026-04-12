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

    public async Task<IEnumerable<BilhetesEvento>> GetFilteredWithDetailsAsync(string? nome, DateOnly? data, string? local, int? idTipo)
    {
        var query = _context.BilhetesEventos
            .Include(be => be.IdBilheteNavigation)
            .ThenInclude(b => b.IdTipoNavigation)
            .Include(be => be.IdEventoNavigation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
        {
            var term = nome.Trim();
            query = query.Where(be =>
                EF.Functions.ILike(be.IdEventoNavigation.Nome, $"%{term}%") ||
                EF.Functions.ILike(be.IdBilheteNavigation.Nome, $"%{term}%"));
        }

        if (data.HasValue)
            query = query.Where(be => be.IdEventoNavigation.Data == data.Value);

        if (!string.IsNullOrWhiteSpace(local))
            query = query.Where(be =>
                be.IdEventoNavigation.Local != null &&
                EF.Functions.ILike(be.IdEventoNavigation.Local, $"%{local.Trim()}%"));

        if (idTipo.HasValue)
            query = query.Where(be => be.IdBilheteNavigation.IdTipo == idTipo.Value);

        return await query
            .OrderBy(be => be.IdEventoNavigation.Data)
            .ThenBy(be => be.IdEventoNavigation.HoraInicio)
            .ThenBy(be => be.IdBilheteNavigation.Nome)
            .ToListAsync();
    }

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