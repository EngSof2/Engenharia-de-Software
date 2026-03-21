using ES2.Data;
using ES2.Models;
using ES2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ES2.Repositories;

public class EventoRepository : IEventoRepository
{
    private readonly AppDbContext _context;

    public EventoRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Evento>> GetAllAsync() =>
        await _context.Eventos.Include(e => e.IdCategoriaNavigation).ToListAsync();

    public async Task<Evento?> GetByIdAsync(int id) =>
        await _context.Eventos
            .Include(e => e.IdCategoriaNavigation)
            .Include(e => e.Atividades)
            .FirstOrDefaultAsync(e => e.IdEvento == id);

    public async Task AddAsync(Evento evento)
    {
        await _context.Eventos.AddAsync(evento);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Evento evento)
    {
        _context.Eventos.Update(evento);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var evento = await GetByIdAsync(id);
        if (evento != null)
        {
            _context.Eventos.Remove(evento);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Evento>> SearchAsync(string? nome, DateTime? data, string? local, int? categoriaId)
    {
        var query = _context.Eventos.Include(e => e.IdCategoriaNavigation).AsQueryable();

        if (!string.IsNullOrEmpty(nome))
            query = query.Where(e => e.Nome.Contains(nome));
        if (data.HasValue)
            query = query.Where(e => e.Data != null && e.Data.Value == DateOnly.FromDateTime(data.Value));
        if (!string.IsNullOrEmpty(local))
            query = query.Where(e => e.Local.Contains(local));
        if (categoriaId.HasValue)
            query = query.Where(e => e.IdCategoria == categoriaId.Value);

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Evento>> GetEventosComParticipantesAsync() =>
        await _context.Eventos
            .Include(e => e.RegistoEventos)
            .ThenInclude(r => r.IdUtiNavigation)
            .ToListAsync();
}