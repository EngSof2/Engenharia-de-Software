using ES2.Data;
using ES2.Models;
using ES2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ES2.Repositories;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly AppDbContext _context;

    public CategoriaRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Categoria>> GetAllAsync() =>
        await _context.Categorias.ToListAsync();

    public async Task<Categoria?> GetByIdAsync(int id) =>
        await _context.Categorias.FindAsync(id);

    public async Task<Categoria?> GetByNomeAsync(string nome) =>
        await _context.Categorias.FirstOrDefaultAsync(c => c.Nome == nome);

    public async Task AddAsync(Categoria categoria)
    {
        await _context.Categorias.AddAsync(categoria);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Categoria categoria)
    {
        _context.Categorias.Update(categoria);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var categoria = await GetByIdAsync(id);
        if (categoria != null)
        {
            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
        }
    }
}