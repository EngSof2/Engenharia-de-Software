using ES2.Data;
using ES2.Models;
using ES2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ES2.Repositories;

public class MensagemRepository : IMensagemRepository
{
    private readonly AppDbContext _context;

    public MensagemRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Mensagem>> GetAllAsync() =>
        await _context.Mensagens.ToListAsync();

    public async Task<Mensagem?> GetByIdAsync(int id) =>
        await _context.Mensagens.FindAsync(id);

    public async Task AddAsync(Mensagem mensagem)
    {
        await _context.Mensagens.AddAsync(mensagem);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Mensagem mensagem)
    {
        _context.Mensagens.Update(mensagem);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var mensagem = await GetByIdAsync(id);
        if (mensagem != null)
        {
            _context.Mensagens.Remove(mensagem);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Mensagem>> GetByRecetorAsync(int utilizadorId) =>
        await _context.Mensagens
            .Where(m => m.IdRecetor == utilizadorId)
            .ToListAsync();
}