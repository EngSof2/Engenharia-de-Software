using ES2.Data;
using ES2.Models;
using ES2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ES2.Repositories;

public class RegistoAtividadeRepository : IRegistoAtividadeRepository
{
    private readonly AppDbContext _context;

    public RegistoAtividadeRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<RegistoAtividade>> GetAllAsync() =>
        await _context.RegistoAtividades.ToListAsync();

    public async Task<RegistoAtividade?> GetByIdAsync(int id) =>
        await _context.RegistoAtividades.FindAsync(id);

    public async Task<IEnumerable<RegistoAtividade>> GetByAtividadeAsync(int atividadeId) =>
        await _context.RegistoAtividades
            .Where(r => r.IdAtividade == atividadeId)
            .ToListAsync();

    public async Task<IEnumerable<RegistoAtividade>> GetByUtilizadorAsync(int utilizadorId) =>
        await _context.RegistoAtividades
            .Where(r => r.IdUti == utilizadorId)
            .ToListAsync();

    public async Task AddAsync(RegistoAtividade registoAtividade)
    {
        await _context.RegistoAtividades.AddAsync(registoAtividade);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(RegistoAtividade registoAtividade)
    {
        _context.RegistoAtividades.Update(registoAtividade);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var registoAtividade = await GetByIdAsync(id);
        if (registoAtividade != null)
        {
            _context.RegistoAtividades.Remove(registoAtividade);
            await _context.SaveChangesAsync();
        }
    }
}