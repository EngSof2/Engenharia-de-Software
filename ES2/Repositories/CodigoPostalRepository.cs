using ES2.Data;
using ES2.Models;
using ES2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ES2.Repositories;

public class CodigoPostalRepository : ICodigoPostalRepository
{
    private readonly AppDbContext _context;

    public CodigoPostalRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<CodigoPostal>> GetAllAsync() =>
        await _context.CodigoPostals.ToListAsync();

    public async Task<CodigoPostal?> GetByIdAsync(int id) =>
        await _context.CodigoPostals.FindAsync(id);

    public async Task<CodigoPostal?> GetByCodPostalAsync(string codPostal) =>
        await _context.CodigoPostals.FirstOrDefaultAsync(c => c.CodPostal == codPostal);

    public async Task AddAsync(CodigoPostal codigoPostal)
    {
        await _context.CodigoPostals.AddAsync(codigoPostal);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(CodigoPostal codigoPostal)
    {
        _context.CodigoPostals.Update(codigoPostal);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var codigoPostal = await GetByIdAsync(id);
        if (codigoPostal != null)
        {
            _context.CodigoPostals.Remove(codigoPostal);
            await _context.SaveChangesAsync();
        }
    }
}