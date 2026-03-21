using ES2.Data;
using ES2.Models;
using ES2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ES2.Repositories;

public class RegistoEventoRepository : IRegistoEventoRepository
{
    private readonly AppDbContext _context;

    public RegistoEventoRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<RegistoEvento>> GetAllAsync() =>
        await _context.RegistoEventos.ToListAsync();

    public async Task<RegistoEvento?> GetByIdAsync(int id) =>
        await _context.RegistoEventos.FindAsync(id);

    public async Task<IEnumerable<RegistoEvento>> GetByEventoAsync(int eventoId) =>
        await _context.RegistoEventos
            .Where(r => r.IdEvento == eventoId)
            .ToListAsync();

    public async Task<IEnumerable<RegistoEvento>> GetByUtilizadorAsync(int utilizadorId) =>
        await _context.RegistoEventos
            .Where(r => r.IdUti == utilizadorId)
            .ToListAsync();

    public async Task<IEnumerable<RegistoEvento>> GetAtivosAsync() =>
        await _context.RegistoEventos
            .Where(r => !r.IsCancelado)
            .ToListAsync();

    public async Task AddAsync(RegistoEvento registoEvento)
    {
        await _context.RegistoEventos.AddAsync(registoEvento);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(RegistoEvento registoEvento)
    {
        _context.RegistoEventos.Update(registoEvento);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var registoEvento = await GetByIdAsync(id);
        if (registoEvento != null)
        {
            _context.RegistoEventos.Remove(registoEvento);
            await _context.SaveChangesAsync();
        }
    }
}