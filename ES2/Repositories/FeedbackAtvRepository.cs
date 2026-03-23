using ES2.Data;
using ES2.Models;
using ES2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ES2.Repositories;

public class FeedbackAtvRepository : IFeedbackAtvRepository
{
    private readonly AppDbContext _context;

    public FeedbackAtvRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<FeedbackAtv>> GetAllAsync() =>
        await _context.FeedbackAtvs.ToListAsync();

    public async Task<FeedbackAtv?> GetByIdAsync(int id) =>
        await _context.FeedbackAtvs.FindAsync(id);

    public async Task<IEnumerable<FeedbackAtv>> GetByAtividadeAsync(int atividadeId) =>
        await _context.FeedbackAtvs
            .Where(f => f.IdAtividade == atividadeId)
            .ToListAsync();

    public async Task<IEnumerable<FeedbackAtv>> GetByUtilizadorAsync(int utilizadorId) =>
        await _context.FeedbackAtvs
            .Where(f => f.IdUti == utilizadorId)
            .ToListAsync();

    public async Task AddAsync(FeedbackAtv feedbackAtv)
    {
        await _context.FeedbackAtvs.AddAsync(feedbackAtv);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(FeedbackAtv feedbackAtv)
    {
        _context.FeedbackAtvs.Update(feedbackAtv);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var feedbackAtv = await GetByIdAsync(id);
        if (feedbackAtv != null)
        {
            _context.FeedbackAtvs.Remove(feedbackAtv);
            await _context.SaveChangesAsync();
        }
    }
}