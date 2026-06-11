using ES2.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ES2.Controllers.Api;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/utilizadores")]
public class UtilizadoresApiController : ControllerBase
{
    private readonly IUtilizadorRepository _utilizadorRepository;

    public UtilizadoresApiController(IUtilizadorRepository utilizadorRepository)
    {
        _utilizadorRepository = utilizadorRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var utilizadores = await _utilizadorRepository.GetAllAsync();

        var resultado = utilizadores
            .OrderBy(u => u.Nome)
            .Select(u => new
            {
                id = u.IdUti,
                nome = u.Nome,
                email = u.Email,
                telemovel = u.Telemovel,
                tipoUti = u.TipoUti,
                perfil = u.TipoUti switch
                {
                    1 => "Admin",
                    3 => "Organizador",
                    _ => "Utilizador"
                }
            });

        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var utilizador = await _utilizadorRepository.GetByIdAsync(id);
        if (utilizador == null)
            return NotFound();

        return Ok(ToDto(utilizador));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AtualizarUtilizadorAdminRequest request)
    {
        var utilizador = await _utilizadorRepository.GetByIdAsync(id);
        if (utilizador == null)
            return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Email) && await _utilizadorRepository.EmailJaExisteAsync(request.Email, id))
            return BadRequest(new { message = "Este email já está a ser utilizado." });

        utilizador.Nome = request.Nome?.Trim() ?? utilizador.Nome;
        utilizador.Email = request.Email?.Trim();
        utilizador.Telemovel = request.Telemovel?.Trim();

        var adminIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var isOwnAccount = adminIdStr == id.ToString();
        if (!isOwnAccount)
            utilizador.TipoUti = PerfilParaTipo(request.Perfil, request.TipoUti);

        await _utilizadorRepository.UpdateAsync(utilizador);

        return Ok(ToDto(utilizador));
    }

    private static object ToDto(ES2.Models.Utilizador u)
    {
        return new
        {
            id = u.IdUti,
            nome = u.Nome,
            email = u.Email,
            telemovel = u.Telemovel,
            tipoUti = u.TipoUti,
            perfil = u.TipoUti switch
            {
                1 => "Admin",
                3 => "Organizador",
                _ => "Utilizador"
            }
        };
    }

    private static int PerfilParaTipo(string? perfil, int? tipoUti)
    {
        if (tipoUti is 1 or 2 or 3)
            return tipoUti.Value;

        return perfil switch
        {
            "Admin" => 1,
            "Organizador" => 3,
            _ => 2
        };
    }
}

public class AtualizarUtilizadorAdminRequest
{
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public string? Telemovel { get; set; }
    public string? Perfil { get; set; }
    public int? TipoUti { get; set; }
}
