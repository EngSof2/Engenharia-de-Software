using ES2.Data;
using ES2.Models;
using ES2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ES2.Repositories;

public class FeedbackEvntRepository : IFeedbackEvntRepository
{
    private readonly AppDbContext _context;

    public FeedbackEvntRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<FeedbackEvnt>> GetAllAsync() =>
        await _context.FeedbackEvnts.ToListAsync();

    public async Task<FeedbackEvnt?> GetByIdAsync(int id) =>
        await _context.FeedbackEvnts.FindAsync(id);

    public async Task<IEnumerable<FeedbackEvnt>> GetByEventoAsync(int eventoId) =>
        await _context.FeedbackEvnts
            .Where(f => f.IdEvento == eventoId)
            .ToListAsync();

    public async Task<IEnumerable<FeedbackEvnt>> GetByUtilizadorAsync(int utilizadorId) =>
        await _context.FeedbackEvnts
            .Where(f => f.IdUti == utilizadorId)
            .ToListAsync();

    public async Task AddAsync(FeedbackEvnt feedbackEvnt)
    {
        await _context.FeedbackEvnts.AddAsync(feedbackEvnt);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(FeedbackEvnt feedbackEvnt)
    {
        _context.FeedbackEvnts.Update(feedbackEvnt);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var feedbackEvnt = await GetByIdAsync(id);
        if (feedbackEvnt != null)
        {
            _context.FeedbackEvnts.Remove(feedbackEvnt);
            await _context.SaveChangesAsync();
        }
    }
}