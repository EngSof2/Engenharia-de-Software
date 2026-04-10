using ES2.Data;
using ES2.DTOs;
using ES2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ES2.Controllers;

[Authorize]
public class EventoController : Controller
{
    private readonly AppDbContext _context;

    public EventoController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? nome, DateOnly? data, string? local, int? idCategoria)
    {
        var query = _context.Eventos.Include(e => e.IdCategoriaNavigation).AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(e => EF.Functions.ILike(e.Nome, $"%{nome}%"));

        if (data.HasValue)
            query = query.Where(e => e.Data == data.Value);

        if (!string.IsNullOrWhiteSpace(local))
            query = query.Where(e => e.Local != null && EF.Functions.ILike(e.Local, $"%{local}%"));

        if (idCategoria.HasValue)
            query = query.Where(e => e.IdCategoria == idCategoria.Value);

        var eventos = await query
            .OrderBy(e => e.Data)
            .ThenBy(e => e.HoraInicio)
            .ToListAsync();

        ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.Nome).ToListAsync();
        ViewBag.FiltroNome = nome;
        ViewBag.FiltroData = data?.ToString("yyyy-MM-dd");
        ViewBag.FiltroLocal = local;
        ViewBag.FiltroCategoria = idCategoria;

        return View(eventos);
    }

    [HttpGet]
    public async Task<IActionResult> Pesquisar(string? nome, DateOnly? data, string? local, int? idCategoria)
    {
        var query = _context.Eventos.Include(e => e.IdCategoriaNavigation).AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(e => EF.Functions.ILike(e.Nome, $"%{nome}%"));

        if (data.HasValue)
            query = query.Where(e => e.Data == data.Value);

        if (!string.IsNullOrWhiteSpace(local))
            query = query.Where(e => e.Local != null && EF.Functions.ILike(e.Local, $"%{local}%"));

        if (idCategoria.HasValue)
            query = query.Where(e => e.IdCategoria == idCategoria.Value);

        var eventos = await query
            .OrderBy(e => e.Data)
            .ThenBy(e => e.HoraInicio)
            .ToListAsync();

        return PartialView("_TabelaEventos", eventos);
    }

    [HttpGet]
    public async Task<IActionResult> Criar()
    {
        ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.Nome).ToListAsync();
        return View(new CriarEventoDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Criar(CriarEventoDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.Nome).ToListAsync();
            return View(dto);
        }

        // Se o utilizador quer criar uma nova categoria
        if (!string.IsNullOrWhiteSpace(dto.NovaCategoria))
        {
            var categoriaExistente = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Nome.ToLower() == dto.NovaCategoria.Trim().ToLower());

            if (categoriaExistente != null)
            {
                dto.IdCategoria = categoriaExistente.IdCategoria;
            }
            else
            {
                var novaCategoria = new Categoria { Nome = dto.NovaCategoria.Trim() };
                _context.Categorias.Add(novaCategoria);
                await _context.SaveChangesAsync();
                dto.IdCategoria = novaCategoria.IdCategoria;
            }
        }

        var evento = new Evento
        {
            Nome = dto.Nome,
            Data = dto.Data,
            HoraInicio = dto.HoraInicio,
            Local = dto.Local,
            Descricao = dto.Descricao,
            CapMax = dto.Capacidade,
            IdCategoria = dto.IdCategoria
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.Eventos.Add(evento);
            await _context.SaveChangesAsync();

            var bilheteBase = new Bilhete
            {
                Nome = "Entrada Normal"
            };

            _context.Bilhetes.Add(bilheteBase);
            await _context.SaveChangesAsync();

            var bilheteEvento = new BilhetesEvento
            {
                IdEvento = evento.IdEvento,
                IdBilhete = bilheteBase.IdBilhete,
                Preco = Convert.ToDouble(dto.Preco!.Value)
            };

            _context.BilhetesEventos.Add(bilheteEvento);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            TempData["Sucesso"] = "Evento criado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty, "Não foi possível criar o evento. Tenta novamente.");
            return View(dto);
        }
    }
}
