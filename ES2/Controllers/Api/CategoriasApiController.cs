using ES2.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ES2.Controllers.Api;

[ApiController]
[Route("api/categorias")]
public class CategoriasApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriasApiController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var categorias = await _context.Categorias
            .OrderBy(c => c.Nome)
            .Select(c => new
            {
                id = c.IdCategoria,
                nome = c.Nome
            })
            .ToListAsync();

        return Ok(categorias);
    }
}

