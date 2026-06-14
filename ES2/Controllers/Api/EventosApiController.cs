using System.Data;
using ES2.Data;
using ES2.Models;
using ES2.Repositories.Interfaces;
using ES2.Services.Inscricoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ES2.Controllers.Api;

[ApiController]
[Route("api/eventos")]
public class EventosApiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguradorBilhetesService _configuradorBilhetes;
    private readonly IInscricaoEventoService _inscricaoEventoService;
    private readonly IFeedbackEvntRepository _feedbackEvntRepository;

    public EventosApiController(
        AppDbContext context,
        IConfiguradorBilhetesService configuradorBilhetes,
        IInscricaoEventoService inscricaoEventoService,
        IFeedbackEvntRepository feedbackEvntRepository)
    {
        _context = context;
        _configuradorBilhetes = configuradorBilhetes;
        _inscricaoEventoService = inscricaoEventoService;
        _feedbackEvntRepository = feedbackEvntRepository;
    }

    private async Task<string?> TryGetEventoImageUrlColumnAsync(CancellationToken ct)
    {
        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
                          select column_name
                          from information_schema.columns
                          where table_schema ilike 'ES2'
                            and table_name ilike 'Evento'
                            and (
                              lower(column_name) in ('imageurl', 'image_url')
                              or lower(column_name) like '%image%url%'
                            )
                          limit 1;
                          """;

        var result = await cmd.ExecuteScalarAsync(ct);
        return result as string;
    }

    private async Task<Dictionary<int, string?>> LoadImageUrlsAsync(string? columnName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            return new Dictionary<int, string?>();

        var conn = _context.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(ct);

        // Column name comes from information_schema. Still quote it, and double-quote escape to be safe.
        var safeCol = columnName.Replace("\"", "\"\"");

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = $"""
                           select "ID_Evento", "{safeCol}"
                           from "ES2"."Evento";
                           """;

        var map = new Dictionary<int, string?>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var id = reader.GetInt32(0);
            var url = reader.IsDBNull(1) ? null : reader.GetString(1);
            map[id] = url;
        }

        return map;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        var imageCol = await TryGetEventoImageUrlColumnAsync(ct);
        var imageUrls = await LoadImageUrlsAsync(imageCol, ct);

        var eventos = await _context.Eventos
            .Include(e => e.IdCategoriaNavigation)
            .Where(e => !e.IsCancelado)
            .OrderBy(e => e.Data)
            .ThenBy(e => e.HoraInicio)
            .Select(e => new
            {
                id = e.IdEvento,
                nome = e.Nome,
                data = e.Data != null ? e.Data.Value.ToString("yyyy-MM-dd") : null,
                horaInicio = e.HoraInicio != null ? e.HoraInicio.Value.ToString("HH:mm") : null,
                local = e.Local,
                descricao = e.Descricao,
                capacidadeMax = e.CapMax,
                cancelado = e.IsCancelado,
                categoria = e.IdCategoriaNavigation != null ? new { id = e.IdCategoriaNavigation.IdCategoria, nome = e.IdCategoriaNavigation.Nome } : null,
                imageUrl = imageUrls.ContainsKey(e.IdEvento) ? imageUrls[e.IdEvento] : null
            })
            .ToListAsync(ct);

        return Ok(eventos);
    }

    [Authorize]
    [HttpGet("inscritos")]
    public async Task<IActionResult> ListarInscritos(CancellationToken ct)
    {
        var nomeUtilizador = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(nomeUtilizador))
            return Unauthorized();

        var utilizador = await _context.Utilizadores
            .FirstOrDefaultAsync(u => u.Nome == nomeUtilizador, ct);

        if (utilizador == null)
            return Unauthorized();

        var imageCol = await TryGetEventoImageUrlColumnAsync(ct);
        var imageUrls = await LoadImageUrlsAsync(imageCol, ct);

        var eventos = await _context.Eventos
            .Where(e => !e.IsCancelado && e.RegistoEventos.Any(r => r.IdUti == utilizador.IdUti && !r.IsCancelado))
            .Distinct()
            .OrderBy(e => e.Data)
            .ThenBy(e => e.HoraInicio)
            .Select(e => new
            {
                id = e.IdEvento,
                nome = e.Nome,
                data = e.Data != null ? e.Data.Value.ToString("yyyy-MM-dd") : null,
                horaInicio = e.HoraInicio != null ? e.HoraInicio.Value.ToString("HH:mm") : null,
                local = e.Local,
                descricao = e.Descricao,
                capacidadeMax = e.CapMax,
                cancelado = e.IsCancelado,
                categoria = e.IdCategoriaNavigation != null ? new { id = e.IdCategoriaNavigation.IdCategoria, nome = e.IdCategoriaNavigation.Nome } : null,
                imageUrl = imageUrls.ContainsKey(e.IdEvento) ? imageUrls[e.IdEvento] : null
            })
            .ToListAsync(ct);

        return Ok(eventos);
    }

    [Authorize(Roles = "Organizador")]
    [HttpGet("criados")]
    public async Task<IActionResult> ListarCriados([FromQuery] bool incluirCancelados = false, CancellationToken ct = default)
    {
        var nomeUtilizador = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(nomeUtilizador))
            return Unauthorized();

        var utilizador = await _context.Utilizadores
            .FirstOrDefaultAsync(u => u.Nome == nomeUtilizador, ct);

        if (utilizador == null)
            return Unauthorized();

        var imageCol = await TryGetEventoImageUrlColumnAsync(ct);
        var imageUrls = await LoadImageUrlsAsync(imageCol, ct);

        var query = _context.Eventos
            .Include(e => e.IdCategoriaNavigation)
            .Where(e => e.IdOrganizador == utilizador.IdUti);

        if (!incluirCancelados)
            query = query.Where(e => !e.IsCancelado);

        var eventos = await query
            .OrderBy(e => e.Data)
            .ThenBy(e => e.HoraInicio)
            .Select(e => new
            {
                id = e.IdEvento,
                nome = e.Nome,
                data = e.Data != null ? e.Data.Value.ToString("yyyy-MM-dd") : null,
                horaInicio = e.HoraInicio != null ? e.HoraInicio.Value.ToString("HH:mm") : null,
                local = e.Local,
                descricao = e.Descricao,
                capacidadeMax = e.CapMax,
                cancelado = e.IsCancelado,
                categoria = e.IdCategoriaNavigation != null ? new { id = e.IdCategoriaNavigation.IdCategoria, nome = e.IdCategoriaNavigation.Nome } : null,
                imageUrl = imageUrls.ContainsKey(e.IdEvento) ? imageUrls[e.IdEvento] : null
            })
            .ToListAsync(ct);

        return Ok(eventos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obter(int id, CancellationToken ct)
    {
        var imageCol = await TryGetEventoImageUrlColumnAsync(ct);
        var imageUrls = await LoadImageUrlsAsync(imageCol, ct);

        var evento = await _context.Eventos
            .Include(e => e.IdCategoriaNavigation)
            .Where(e => e.IdEvento == id)
            .Select(e => new
            {
                id = e.IdEvento,
                idOrganizador = e.IdOrganizador,
                nome = e.Nome,
                data = e.Data != null ? e.Data.Value.ToString("yyyy-MM-dd") : null,
                horaInicio = e.HoraInicio != null ? e.HoraInicio.Value.ToString("HH:mm") : null,
                local = e.Local,
                descricao = e.Descricao,
                capacidadeMax = e.CapMax,
                cancelado = e.IsCancelado,
                categoria = e.IdCategoriaNavigation != null ? new { id = e.IdCategoriaNavigation.IdCategoria, nome = e.IdCategoriaNavigation.Nome } : null,
                imageUrl = imageUrls.ContainsKey(e.IdEvento) ? imageUrls[e.IdEvento] : null
            })
            .FirstOrDefaultAsync(ct);

        if (evento == null)
            return NotFound();

        var podeGerirAtividades = false;
        var eOrganizadorDoEvento = false;
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin"))
            {
                podeGerirAtividades = true;
            }
            else if (User.IsInRole("Organizador"))
            {
                var nomeUtilizador = User.Identity?.Name;
                var utilizador = string.IsNullOrWhiteSpace(nomeUtilizador)
                    ? null
                    : await _context.Utilizadores
                        .FirstOrDefaultAsync(u => u.Nome == nomeUtilizador, ct);

                eOrganizadorDoEvento = utilizador != null && evento.idOrganizador == utilizador.IdUti;
                podeGerirAtividades = eOrganizadorDoEvento;
            }
        }

        return Ok(new
        {
            evento.id,
            evento.idOrganizador,
            evento.nome,
            evento.data,
            evento.horaInicio,
            evento.local,
            evento.descricao,
            evento.capacidadeMax,
            evento.cancelado,
            evento.categoria,
            evento.imageUrl,
            podeGerirAtividades,
            eOrganizadorDoEvento
        });
    }

    [HttpGet("{id:int}/bilhetes")]
    public async Task<IActionResult> ListarBilhetes(int id)
    {
        var existeEvento = await _context.Eventos.AnyAsync(e => e.IdEvento == id && !e.IsCancelado);
        if (!existeEvento)
            return NotFound();

        var ofertas = await _configuradorBilhetes.GarantirEObterOfertasAsync(id);
        var idsBilhetesAtivos = await _context.BilhetesEventos
            .Where(be => be.IdEvento == id && !be.IsCancelado)
            .Select(be => be.IdBiEv)
            .ToListAsync();
        var jaInscrito = (await _inscricaoEventoService.ObterEventosInscritosAsync(User.Identity?.Name)).Contains(id);
        var idBilheteAtivo = await _inscricaoEventoService.ObterBilheteAtivoDoEventoAsync(id, User.Identity?.Name);

        return Ok(new
        {
            jaInscrito,
            idBilheteAtivo,
            ofertas = ofertas.Where(o => idsBilhetesAtivos.Contains(o.IdBilheteEvento)).Select(o => new
            {
                idBilheteEvento = o.IdBilheteEvento,
                nomeBilhete = o.NomeBilhete,
                tipoBilhete = o.TipoBilhete,
                descricaoAcesso = o.DescricaoAcesso,
                classeIcone = o.ClasseIcone,
                preco = o.Preco,
                quantidadeDisponivel = o.QuantidadeDisponivel,
                esgotado = o.Esgotado
            })
        });
    }

    // Qualquer utilizador (mesmo nao inscrito) pode ver as avaliacoes do evento.
    [HttpGet("{id:int}/avaliacoes")]
    public async Task<IActionResult> ListarAvaliacoes(int id, CancellationToken ct)
    {
        var evento = await _context.Eventos
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.IdEvento == id, ct);
        if (evento == null)
            return NotFound();

        var feedbacks = await _feedbackEvntRepository.GetByEventoComUtilizadorAsync(id);
        var media = feedbacks.Count > 0
            ? Math.Round(feedbacks.Average(f => f.Classificacao), 1)
            : 0d;

        var nome = User.Identity?.Name;
        var autenticado = !string.IsNullOrWhiteSpace(nome);
        var jaInscrito = autenticado &&
                         (await _inscricaoEventoService.ObterEventosInscritosAsync(nome)).Contains(id);
        var utilizador = autenticado
            ? await _context.Utilizadores.AsNoTracking().FirstOrDefaultAsync(u => u.Nome == nome, ct)
            : null;
        var eOrganizadorDoEvento = utilizador != null && User.IsInRole("Organizador") && evento.IdOrganizador == utilizador.IdUti;

        return Ok(new
        {
            media,
            total = feedbacks.Count,
            autenticado,
            jaInscrito,
            eOrganizadorDoEvento,
            avaliacoes = feedbacks.Select(f => new
            {
                autor = f.IdUtiNavigation != null ? f.IdUtiNavigation.Nome : "Utilizador",
                classificacao = f.Classificacao,
                descricao = f.Descricao
            })
        });
    }

    public sealed class CriarAvaliacaoRequest
    {
        public int Classificacao { get; set; }
        public string? Descricao { get; set; }
    }

    // So utilizadores inscritos no evento podem deixar uma avaliacao (1 a 5 estrelas + comentario).
    [Authorize]
    [HttpPost("{id:int}/avaliacoes")]
    public async Task<IActionResult> AdicionarAvaliacao(int id, [FromBody] CriarAvaliacaoRequest req, CancellationToken ct)
    {
        var evento = await _context.Eventos.AsNoTracking().FirstOrDefaultAsync(e => e.IdEvento == id, ct);
        if (evento == null)
            return NotFound(new { message = "O evento selecionado nao existe." });

        var nome = User.Identity?.Name;
        var utilizador = string.IsNullOrWhiteSpace(nome)
            ? null
            : await _context.Utilizadores.FirstOrDefaultAsync(u => u.Nome == nome, ct);

        if (utilizador == null)
            return BadRequest(new { message = "Nao foi possivel identificar o utilizador autenticado." });

        if (User.IsInRole("Organizador") && evento.IdOrganizador == utilizador.IdUti)
            return BadRequest(new { message = "Organizadores nao podem avaliar os seus proprios eventos." });

        var inscrito = (await _inscricaoEventoService.ObterEventosInscritosAsync(nome)).Contains(id);
        if (!inscrito)
            return BadRequest(new { message = "So podes avaliar eventos em que estas inscrito." });

        if (req.Classificacao is < 1 or > 5)
            return BadRequest(new { message = "A classificacao tem de ser entre 1 e 5 estrelas." });

        var descricao = req.Descricao?.Trim();
        if (string.IsNullOrWhiteSpace(descricao))
            return BadRequest(new { message = "O comentario nao pode estar vazio." });

        await _feedbackEvntRepository.AddAsync(new FeedbackEvnt
        {
            IdEvento = id,
            IdUti = utilizador.IdUti,
            Classificacao = req.Classificacao,
            Descricao = descricao
        });

        return Ok(new { message = "Avaliacao submetida com sucesso. Obrigado pelo teu feedback!" });
    }

    [HttpGet("{id:int}/atividades")]
    public async Task<IActionResult> ListarAtividades(int id, CancellationToken ct)
    {
        var eventoCancelado = await _context.Eventos
            .AnyAsync(e => e.IdEvento == id && e.IsCancelado, ct);
        if (eventoCancelado)
            return Ok(Array.Empty<object>());

        var nomeUtilizador = User.Identity?.Name;
        var utilizador = string.IsNullOrWhiteSpace(nomeUtilizador)
            ? null
            : await _context.Utilizadores.FirstOrDefaultAsync(u => u.Nome == nomeUtilizador, ct);

        var idsInscritos = new HashSet<int>();
        var acessoAutomaticoAtividades = false;

        if (utilizador != null)
        {
            acessoAutomaticoAtividades = await UtilizadorTemBilheteComAcessoAtividadesAsync(utilizador.IdUti, id, ct);

            if (acessoAutomaticoAtividades)
                await GarantirRegistoAtividadesAsync(utilizador.IdUti, id, ct);

            idsInscritos = (await _context.RegistoAtividades
                    .Where(r => r.IdUti == utilizador.IdUti &&
                                !r.IsCancelado &&
                                r.IdAtividadeNavigation.IdEvento == id)
                    .Select(r => r.IdAtividade)
                    .ToListAsync(ct))
                .ToHashSet();
        }

        var atividades = await _context.Atividades
            .Include(a => a.IdCategoriaNavigation)
            .Where(a => a.IdEvento == id && !a.IsCancelado)
            .OrderBy(a => a.Nome)
            .Select((a) => new
            {
                id = a.IdAtividade,
                nome = a.Nome,
                local = a.Local,
                capacidade = a.Capacidade,
                categoria = new { id = a.IdCategoria, nome = a.IdCategoriaNavigation.Nome },
                inscrito = idsInscritos.Contains(a.IdAtividade),
                acessoAutomatico = acessoAutomaticoAtividades
            })
            .ToListAsync(ct);

        return Ok(atividades);
    }

    [Authorize]
    [HttpPost("{id:int}/atividades/{atividadeId:int}/inscricao")]
    public async Task<IActionResult> InscreverAtividade(int id, int atividadeId, CancellationToken ct)
    {
        var resultado = await AlterarInscricaoAtividadeAsync(id, atividadeId, inscrever: true, ct);
        return resultado.Sucesso
            ? Ok(new { message = resultado.Mensagem })
            : BadRequest(new { message = resultado.Mensagem });
    }

    [Authorize]
    [HttpDelete("{id:int}/atividades/{atividadeId:int}/inscricao")]
    public async Task<IActionResult> CancelarInscricaoAtividade(int id, int atividadeId, CancellationToken ct)
    {
        var resultado = await AlterarInscricaoAtividadeAsync(id, atividadeId, inscrever: false, ct);
        return resultado.Sucesso
            ? Ok(new { message = resultado.Mensagem })
            : BadRequest(new { message = resultado.Mensagem });
    }

    [Authorize(Roles = "Admin,Organizador")]
    [HttpDelete("{id:int}/atividades/{atividadeId:int}")]
    public async Task<IActionResult> CancelarAtividade(int id, int atividadeId, CancellationToken ct)
    {
        var atividade = await _context.Atividades
            .Include(a => a.IdEventoNavigation)
            .FirstOrDefaultAsync(a => a.IdAtividade == atividadeId && a.IdEvento == id, ct);

        if (atividade == null)
            return NotFound(new { message = "A atividade selecionada nao existe." });

        if (!await TemPermissaoEditarEventoAsync(atividade.IdEventoNavigation, ct))
            return Forbid("Nao tens permissao para cancelar esta atividade.");

        if (atividade.IsCancelado)
            return Ok(new { message = "A atividade ja estava cancelada." });

        atividade.IsCancelado = true;

        var registos = await _context.RegistoAtividades
            .Where(r => r.IdAtividade == atividadeId && !r.IsCancelado)
            .ToListAsync(ct);

        foreach (var registo in registos)
            registo.IsCancelado = true;

        await _context.SaveChangesAsync(ct);
        return Ok(new { message = "Atividade cancelada com sucesso." });
    }

    public sealed class CriarAtividadeRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string Local { get; set; } = string.Empty;
        public int? Capacidade { get; set; }
        public int? IdCategoria { get; set; }
        public string? NovaCategoriaNome { get; set; }
    }

    [Authorize(Roles = "Admin,Organizador")]
    [HttpPost("{id:int}/atividades")]
    public async Task<IActionResult> CriarAtividade(int id, [FromBody] CriarAtividadeRequest req, CancellationToken ct)
    {
        var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.IdEvento == id, ct);
        if (evento == null)
            return NotFound(new { message = "O evento selecionado nao existe." });

        if (evento.IsCancelado)
            return BadRequest(new { message = "Nao e possivel adicionar atividades a um evento cancelado." });

        if (!await TemPermissaoEditarEventoAsync(evento, ct))
            return Forbid("Nao tens permissao para adicionar atividades a este evento.");

        if (string.IsNullOrWhiteSpace(req.Nome))
            return BadRequest(new { message = "O nome da atividade e obrigatorio." });

        if (string.IsNullOrWhiteSpace(req.Local))
            return BadRequest(new { message = "O local da atividade e obrigatorio." });

        if (req.Capacidade is null or <= 0)
            return BadRequest(new { message = "A capacidade deve ser superior a 0." });

        var novaCategoriaNome = req.NovaCategoriaNome?.Trim();
        if (req.IdCategoria is null && string.IsNullOrWhiteSpace(novaCategoriaNome))
            return BadRequest(new { message = "A categoria e obrigatoria." });

        if (!string.IsNullOrWhiteSpace(novaCategoriaNome) && novaCategoriaNome.Length > 40)
            return BadRequest(new { message = "A nova categoria nao pode ter mais de 40 caracteres." });

        Categoria? categoria;
        if (!string.IsNullOrWhiteSpace(novaCategoriaNome))
        {
            categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Nome.ToLower() == novaCategoriaNome.ToLower(), ct)
                ?? new Categoria { Nome = novaCategoriaNome };

            if (categoria.IdCategoria == 0)
            {
                _context.Categorias.Add(categoria);
                await _context.SaveChangesAsync(ct);
            }
        }
        else
        {
            categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.IdCategoria == req.IdCategoria!.Value, ct);

            if (categoria == null)
                return BadRequest(new { message = "A categoria selecionada nao existe." });
        }

        var temCategoriaEvento = await _context.CategoriaEventos
            .AnyAsync(c => c.IdEvento == id && c.IdCategoria == categoria.IdCategoria, ct);

        if (!temCategoriaEvento)
        {
            _context.CategoriaEventos.Add(new CategoriaEvento
            {
                IdEvento = id,
                IdCategoria = categoria.IdCategoria
            });
        }

        var atividade = new Atividade
        {
            IdEvento = id,
            Nome = req.Nome.Trim(),
            Local = req.Local.Trim(),
            Capacidade = req.Capacidade.Value,
            IdCategoria = categoria.IdCategoria
        };

        _context.Atividades.Add(atividade);
        await _context.SaveChangesAsync(ct);

        return Ok(new
        {
            id = atividade.IdAtividade,
            nome = atividade.Nome,
            local = atividade.Local,
            capacidade = atividade.Capacidade,
            categoria = new { id = categoria.IdCategoria, nome = categoria.Nome },
            inscrito = false,
            acessoAutomatico = false
        });
    }

    public sealed class CriarEventoRequest
    {
        public string Nome { get; set; } = string.Empty;
        public string? Data { get; set; } // yyyy-MM-dd
        public string? HoraInicio { get; set; } // HH:mm
        public string Local { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int? Capacidade { get; set; }
        public decimal? Preco { get; set; }
        public int? QuantidadeStandard { get; set; }
        public int? QuantidadeGold { get; set; }
        public int? QuantidadeVip { get; set; }
        public int? IdCategoria { get; set; }
        public string? NovaCategoria { get; set; }
        public string? ImageUrl { get; set; }
    }

    [Authorize(Roles = "Admin,Organizador")]
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarEventoRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Nome))
            return BadRequest("Nome e obrigatorio.");

        if (string.IsNullOrWhiteSpace(req.Data) || !DateOnly.TryParse(req.Data, out var data))
            return BadRequest("Data invalida.");

        if (string.IsNullOrWhiteSpace(req.HoraInicio) || !TimeOnly.TryParse(req.HoraInicio, out var horaInicio))
            return BadRequest("HoraInicio invalida.");

        if (string.IsNullOrWhiteSpace(req.Local))
            return BadRequest("Local e obrigatorio.");

        if (string.IsNullOrWhiteSpace(req.Descricao))
            return BadRequest("Descricao e obrigatoria.");

        if (req.Capacidade is null or <= 0)
            return BadRequest("Capacidade invalida.");

        if (req.Preco is null or < 0)
            return BadRequest("Preco invalido.");

        var quantidadeStandard = req.QuantidadeStandard ?? req.Capacidade.Value;
        var quantidadeGold = req.QuantidadeGold ?? 0;
        var quantidadeVip = req.QuantidadeVip ?? 0;

        if (quantidadeStandard < 0 ||
            quantidadeGold < 0 ||
            quantidadeVip < 0)
            return BadRequest("As quantidades de bilhetes sao obrigatorias e nao podem ser negativas.");

        if (quantidadeStandard + quantidadeGold + quantidadeVip > req.Capacidade.Value)
            return BadRequest("A soma dos bilhetes Standard, Gold e VIP nao pode ultrapassar a capacidade maxima do evento.");

        if (quantidadeStandard + quantidadeGold + quantidadeVip <= 0)
            return BadRequest("Deves disponibilizar pelo menos um bilhete.");

        var novaCategoria = req.NovaCategoria?.Trim();
        if (!string.IsNullOrWhiteSpace(novaCategoria) && novaCategoria.Length > 40)
            return BadRequest("O nome da categoria nao pode ter mais de 40 caracteres.");

        var nomeUtilizador = User.Identity?.Name;
        var organizador = string.IsNullOrWhiteSpace(nomeUtilizador)
            ? null
            : await _context.Utilizadores
                .FirstOrDefaultAsync(u => u.Nome == nomeUtilizador, ct);

        await using var tx = await _context.Database.BeginTransactionAsync(ct);

        int? idCategoria = req.IdCategoria;
        Categoria? categoriaCriada = null;
        if (!string.IsNullOrWhiteSpace(novaCategoria))
        {
            categoriaCriada = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Nome.ToLower() == novaCategoria.ToLower(), ct);

            if (categoriaCriada == null)
            {
                categoriaCriada = new Categoria { Nome = novaCategoria };
                _context.Categorias.Add(categoriaCriada);
                await _context.SaveChangesAsync(ct);
            }

            idCategoria = categoriaCriada.IdCategoria;
        }
        else if (idCategoria.HasValue)
        {
            categoriaCriada = await _context.Categorias
                .FirstOrDefaultAsync(c => c.IdCategoria == idCategoria.Value, ct);

            if (categoriaCriada == null)
                return BadRequest("Categoria invalida.");
        }

        var evento = new Evento
        {
            Nome = req.Nome.Trim(),
            Data = data,
            HoraInicio = horaInicio,
            Local = req.Local.Trim(),
            Descricao = req.Descricao.Trim(),
            CapMax = req.Capacidade,
            IdCategoria = idCategoria,
            IdOrganizador = organizador?.IdUti
        };

        try
        {
            _context.Eventos.Add(evento);
            await _context.SaveChangesAsync(ct);

            if (idCategoria.HasValue)
            {
                _context.CategoriaEventos.Add(new CategoriaEvento
                {
                    IdEvento = evento.IdEvento,
                    IdCategoria = idCategoria.Value
                });
                await _context.SaveChangesAsync(ct);
            }

            // Bilhete base que depois e expandido para Standard/Gold/VIP.
            var bilheteBase = new Bilhete { Nome = "Entrada Normal" };
            _context.Bilhetes.Add(bilheteBase);
            await _context.SaveChangesAsync(ct);

            var bilheteEvento = new BilhetesEvento
            {
                IdEvento = evento.IdEvento,
                IdBilhete = bilheteBase.IdBilhete,
                Preco = Convert.ToDouble(req.Preco.Value)
            };

            _context.BilhetesEventos.Add(bilheteEvento);
            await _context.SaveChangesAsync(ct);

            // Optional ImageUrl: only write if a matching column exists.
            if (!string.IsNullOrWhiteSpace(req.ImageUrl))
            {
                var imageCol = await TryGetEventoImageUrlColumnAsync(ct);
                if (!string.IsNullOrWhiteSpace(imageCol))
                {
                    var conn = _context.Database.GetDbConnection();
                    if (conn.State != ConnectionState.Open)
                        await conn.OpenAsync(ct);

                    var safeCol = imageCol.Replace("\"", "\"\"");
                    await using var cmd = conn.CreateCommand();
                    cmd.CommandText = $"""
                                       update "ES2"."Evento"
                                       set "{safeCol}" = @p_url
                                       where "ID_Evento" = @p_id;
                                       """;
                    var pUrl = cmd.CreateParameter();
                    pUrl.ParameterName = "p_url";
                    pUrl.Value = req.ImageUrl.Trim();
                    cmd.Parameters.Add(pUrl);

                    var pId = cmd.CreateParameter();
                    pId.ParameterName = "p_id";
                    pId.Value = evento.IdEvento;
                    cmd.Parameters.Add(pId);

                    await cmd.ExecuteNonQueryAsync(ct);
                }
            }

            await _configuradorBilhetes.ConfigurarBilhetesEventoAsync(
                evento.IdEvento,
                req.Preco.Value,
                quantidadeStandard,
                quantidadeGold,
                quantidadeVip);
            await tx.CommitAsync(ct);
            return Ok(new { id = evento.IdEvento });
        }
        catch
        {
            await tx.RollbackAsync(ct);
            return Problem("Nao foi possivel criar o evento.");
        }
    }

    [Authorize(Roles = "Admin,Organizador")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, [FromBody] CriarEventoRequest req, CancellationToken ct)
    {
        var evento = await _context.Eventos
            .Include(e => e.BilhetesEventos)
            .FirstOrDefaultAsync(e => e.IdEvento == id, ct);

        if (evento == null)
            return NotFound("O evento nao existe.");

        if (!await TemPermissaoEditarEventoAsync(evento, ct))
            return Forbid("Nao tens permissao para editar este evento.");

        if (string.IsNullOrWhiteSpace(req.Nome))
            return BadRequest("Nome e obrigatorio.");

        if (string.IsNullOrWhiteSpace(req.Data) || !DateOnly.TryParse(req.Data, out var data))
            return BadRequest("Data invalida.");

        if (string.IsNullOrWhiteSpace(req.HoraInicio) || !TimeOnly.TryParse(req.HoraInicio, out var horaInicio))
            return BadRequest("HoraInicio invalida.");

        if (string.IsNullOrWhiteSpace(req.Local))
            return BadRequest("Local e obrigatorio.");

        if (string.IsNullOrWhiteSpace(req.Descricao))
            return BadRequest("Descricao e obrigatoria.");

        if (req.Capacidade is null or <= 0)
            return BadRequest("Capacidade invalida.");

        var deveAtualizarBilhetes = req.Preco.HasValue ||
                                     req.QuantidadeStandard.HasValue ||
                                     req.QuantidadeGold.HasValue ||
                                     req.QuantidadeVip.HasValue;

        if (deveAtualizarBilhetes)
        {
            if (req.Preco is null or < 0)
                return BadRequest("Preco invalido.");

            if (req.QuantidadeStandard is null or < 0 ||
                req.QuantidadeGold is null or < 0 ||
                req.QuantidadeVip is null or < 0)
                return BadRequest("As quantidades de bilhetes sao obrigatorias e nao podem ser negativas.");

            if (req.QuantidadeStandard + req.QuantidadeGold + req.QuantidadeVip > req.Capacidade)
                return BadRequest("A soma dos bilhetes Standard, Gold e VIP nao pode ultrapassar a capacidade maxima do evento.");

            if (req.QuantidadeStandard + req.QuantidadeGold + req.QuantidadeVip <= 0)
                return BadRequest("Deves disponibilizar pelo menos um bilhete.");
        }

        var novaCategoria = req.NovaCategoria?.Trim();
        int? idCategoria = req.IdCategoria;
        Categoria? categoriaEditada = null;

        if (!string.IsNullOrWhiteSpace(novaCategoria))
        {
            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.Nome.ToLower() == novaCategoria.ToLower(), ct);

            if (categoria == null)
            {
                categoria = new Categoria { Nome = novaCategoria };
                _context.Categorias.Add(categoria);
                await _context.SaveChangesAsync(ct);
            }

            idCategoria = categoria.IdCategoria;
            categoriaEditada = categoria;
        }
        else if (idCategoria.HasValue)
        {
            categoriaEditada = await _context.Categorias
                .FirstOrDefaultAsync(c => c.IdCategoria == idCategoria.Value, ct);

            if (categoriaEditada == null)
                return BadRequest("A categoria selecionada nao existe.");
        }

        evento.Nome = req.Nome.Trim();
        evento.Data = data;
        evento.HoraInicio = horaInicio;
        evento.Local = req.Local.Trim();
        evento.Descricao = req.Descricao.Trim();
        evento.CapMax = req.Capacidade;
        evento.IdCategoria = idCategoria;

        if (idCategoria.HasValue)
        {
            var temCategoriaEvento = await _context.CategoriaEventos
                .AnyAsync(c => c.IdEvento == evento.IdEvento && c.IdCategoria == idCategoria.Value, ct);

            if (!temCategoriaEvento)
            {
                _context.CategoriaEventos.Add(new CategoriaEvento
                {
                    IdEvento = evento.IdEvento,
                    IdCategoria = idCategoria.Value
                });
            }
        }

        await _context.SaveChangesAsync(ct);

        if (!string.IsNullOrWhiteSpace(req.ImageUrl))
        {
            var imageCol = await TryGetEventoImageUrlColumnAsync(ct);
            if (!string.IsNullOrWhiteSpace(imageCol))
            {
                var conn = _context.Database.GetDbConnection();
                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync(ct);

                var safeCol = imageCol.Replace("\"", "\"\"");
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = $"""
                                   update "ES2"."Evento"
                                   set "{safeCol}" = @p_url
                                   where "ID_Evento" = @p_id;
                                   """;
                var pUrl = cmd.CreateParameter();
                pUrl.ParameterName = "p_url";
                pUrl.Value = req.ImageUrl.Trim();
                cmd.Parameters.Add(pUrl);

                var pId = cmd.CreateParameter();
                pId.ParameterName = "p_id";
                pId.Value = evento.IdEvento;
                cmd.Parameters.Add(pId);

                await cmd.ExecuteNonQueryAsync(ct);
            }
        }

        if (deveAtualizarBilhetes)
        {
            await _configuradorBilhetes.ConfigurarBilhetesEventoAsync(
                evento.IdEvento,
                req.Preco!.Value,
                req.QuantidadeStandard!.Value,
                req.QuantidadeGold!.Value,
                req.QuantidadeVip!.Value);
        }

        return Ok(new
        {
            message = "Evento editado com sucesso.",
            categoria = categoriaEditada == null
                ? null
                : new { id = categoriaEditada.IdCategoria, nome = categoriaEditada.Nome }
        });
    }

    [Authorize(Roles = "Admin,Organizador")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        var evento = await _context.Eventos
            .Include(e => e.BilhetesEventos)
            .Include(e => e.Atividades)
            .Include(e => e.RegistoEventos)
            .FirstOrDefaultAsync(e => e.IdEvento == id, ct);

        if (evento == null)
            return NotFound("O evento nao existe.");

        if (!await TemPermissaoEditarEventoAsync(evento, ct))
            return Forbid("Nao tens permissao para eliminar este evento.");

        if (evento.IsCancelado)
            return Ok("Evento ja estava cancelado.");

        await using var tx = await _context.Database.BeginTransactionAsync(ct);

        try
        {
            evento.IsCancelado = true;

            var atividadesIds = evento.Atividades.Select(a => a.IdAtividade).ToList();
            var registoAtividades = await _context.RegistoAtividades
                .Where(r => atividadesIds.Contains(r.IdAtividade))
                .ToListAsync(ct);

            foreach (var registoAtividade in registoAtividades)
                registoAtividade.IsCancelado = true;

            foreach (var atividade in evento.Atividades)
                atividade.IsCancelado = true;

            foreach (var registoEvento in evento.RegistoEventos)
                registoEvento.IsCancelado = true;

            var bilheteUtils = await _context.BilheteUtils
                .Where(bu => evento.BilhetesEventos.Select(be => be.IdBiEv).Contains(bu.IdBiEv.Value))
                .ToListAsync(ct);

            _context.BilheteUtils.RemoveRange(bilheteUtils);

            foreach (var bilheteEvento in evento.BilhetesEventos)
                bilheteEvento.IsCancelado = true;

            await _context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return Ok("Evento cancelado com sucesso.");
        }
        catch
        {
            await tx.RollbackAsync(ct);
            return Problem("Nao foi possivel cancelar o evento.");
        }
    }

    [Authorize(Roles = "Admin,Organizador")]
    [HttpPost("{id:int}/reativar")]
    public async Task<IActionResult> Reativar(int id, CancellationToken ct)
    {
        var evento = await _context.Eventos
            .Include(e => e.BilhetesEventos)
            .Include(e => e.Atividades)
            .FirstOrDefaultAsync(e => e.IdEvento == id, ct);

        if (evento == null)
            return NotFound("O evento nao existe.");

        if (!await TemPermissaoEditarEventoAsync(evento, ct))
            return Forbid("Nao tens permissao para reativar este evento.");

        if (!evento.IsCancelado)
            return Ok("Evento ja estava ativo.");

        evento.IsCancelado = false;

        foreach (var atividade in evento.Atividades)
            atividade.IsCancelado = false;

        foreach (var bilheteEvento in evento.BilhetesEventos)
            bilheteEvento.IsCancelado = false;

        await _context.SaveChangesAsync(ct);

        return Ok("Evento reativado com sucesso.");
    }

    private async Task<bool> TemPermissaoEditarEventoAsync(Evento evento, CancellationToken ct)
    {
        var nomeUtilizador = User.Identity?.Name;
        var utilizador = string.IsNullOrWhiteSpace(nomeUtilizador)
            ? null
            : await _context.Utilizadores
                .FirstOrDefaultAsync(u => u.Nome == nomeUtilizador, ct);

        if (utilizador == null)
            return false;

        if (User.IsInRole("Admin"))
            return true;

        if (User.IsInRole("Organizador"))
            return evento.IdOrganizador == utilizador.IdUti;

        return false;
    }

    private async Task<(bool Sucesso, string Mensagem)> AlterarInscricaoAtividadeAsync(
        int eventoId,
        int atividadeId,
        bool inscrever,
        CancellationToken ct)
    {
        var nomeUtilizador = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(nomeUtilizador))
            return (false, "Nao foi possivel identificar o utilizador autenticado.");

        var utilizador = await _context.Utilizadores
            .FirstOrDefaultAsync(u => u.Nome == nomeUtilizador, ct);

        if (utilizador == null)
            return (false, "Nao foi possivel identificar o utilizador autenticado.");

        if (await UtilizadorTemBilheteComAcessoAtividadesAsync(utilizador.IdUti, eventoId, ct))
            return (false, "As atividades ja estao incluidas automaticamente no teu bilhete Gold/VIP.");

        var atividade = await _context.Atividades
            .Include(a => a.IdEventoNavigation)
            .FirstOrDefaultAsync(a => a.IdAtividade == atividadeId && a.IdEvento == eventoId, ct);

        if (atividade == null || atividade.IsCancelado || atividade.IdEventoNavigation.IsCancelado)
            return (false, "A atividade selecionada nao existe.");

        var registo = await _context.RegistoAtividades
            .FirstOrDefaultAsync(r => r.IdUti == utilizador.IdUti && r.IdAtividade == atividadeId, ct);

        if (inscrever)
        {
            if (registo != null && !registo.IsCancelado)
                return (false, "Ja estas inscrito nesta atividade.");

            var inscritosAtivos = await _context.RegistoAtividades
                .CountAsync(r => r.IdAtividade == atividadeId && !r.IsCancelado, ct);

            if (inscritosAtivos >= atividade.Capacidade)
                return (false, "Esta atividade ja atingiu a capacidade maxima.");

            if (registo == null)
            {
                _context.RegistoAtividades.Add(new RegistoAtividade
                {
                    IdUti = utilizador.IdUti,
                    IdAtividade = atividadeId,
                    IsCancelado = false
                });
            }
            else
            {
                registo.IsCancelado = false;
            }

            await _context.SaveChangesAsync(ct);
            return (true, $"Inscricao na atividade '{atividade.Nome}' efetuada com sucesso.");
        }

        if (registo == null || registo.IsCancelado)
            return (false, "Nao tens uma inscricao ativa nesta atividade.");

        registo.IsCancelado = true;
        await _context.SaveChangesAsync(ct);

        return (true, "Inscricao na atividade cancelada com sucesso.");
    }

    private async Task<bool> UtilizadorTemBilheteComAcessoAtividadesAsync(int utilizadorId, int eventoId, CancellationToken ct)
    {
        var temEventoAtivo = await _context.RegistoEventos.AnyAsync(r =>
            r.IdUti == utilizadorId &&
            r.IdEvento == eventoId &&
            !r.IsCancelado, ct);

        if (!temEventoAtivo)
            return false;

        var tipoBilhete = await _context.BilheteUtils
            .Where(bu => bu.IdUtilizador == utilizadorId &&
                         bu.IdBiEvNavigation != null &&
                         bu.IdBiEvNavigation.IdEvento == eventoId)
            .OrderByDescending(bu => bu.IdBiUti)
            .Select(bu => bu.IdBiEvNavigation.IdBilheteNavigation.IdTipoNavigation.Nome)
            .FirstOrDefaultAsync(ct);

        return string.Equals(tipoBilhete, "Gold", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(tipoBilhete, "VIP", StringComparison.OrdinalIgnoreCase);
    }

    private async Task GarantirRegistoAtividadesAsync(int utilizadorId, int eventoId, CancellationToken ct)
    {
        var atividades = await _context.Atividades
            .Where(a => a.IdEvento == eventoId && !a.IsCancelado && !a.IdEventoNavigation.IsCancelado)
            .Select(a => a.IdAtividade)
            .ToListAsync(ct);

        foreach (var atividadeId in atividades)
        {
            var registo = await _context.RegistoAtividades
                .FirstOrDefaultAsync(r => r.IdUti == utilizadorId && r.IdAtividade == atividadeId, ct);

            if (registo == null)
            {
                _context.RegistoAtividades.Add(new RegistoAtividade
                {
                    IdUti = utilizadorId,
                    IdAtividade = atividadeId,
                    IsCancelado = false
                });
            }
            else
            {
                registo.IsCancelado = false;
            }
        }

        await _context.SaveChangesAsync(ct);
    }
}
