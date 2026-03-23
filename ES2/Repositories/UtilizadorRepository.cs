using ES2.Data;
using ES2.Models;
using ES2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ES2.Repositories;

public class UtilizadorRepository : IUtilizadorRepository
{
    private readonly AppDbContext _context;

    public UtilizadorRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Utilizador>> GetAllAsync() =>
        await _context.Utilizadores.ToListAsync();

    public async Task<Utilizador?> GetByIdAsync(int id) =>
        await _context.Utilizadores.FindAsync(id);

    public async Task<Utilizador?> GetByEmailAsync(string email) =>
        await _context.Utilizadores.FirstOrDefaultAsync(u => u.Email == email);

    public async Task AddAsync(Utilizador utilizador)
    {
        await _context.Utilizadores.AddAsync(utilizador);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Utilizador utilizador)
    {
        _context.Utilizadores.Update(utilizador);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var utilizador = await GetByIdAsync(id);
        if (utilizador != null)
        {
            _context.Utilizadores.Remove(utilizador);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Utilizador>> GetParticipantesByEventoAsync(int eventoId) =>
        await _context.RegistoEventos
            .Where(r => r.IdEvento == eventoId && !r.IsCancelado)
            .Select(r => r.IdUtiNavigation)
            .ToListAsync();
}