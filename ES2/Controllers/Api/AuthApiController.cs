using ES2.Data;
using ES2.Models;
using ES2.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ES2.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IRegistoService _registoService;

    public AuthApiController(AppDbContext context, IRegistoService registoService)
    {
        _context = context;
        _registoService = registoService;
    }

    [HttpGet("status")]
    public IActionResult Status()
    {
        var isAuthenticated = User.Identity?.IsAuthenticated == true;
        return Ok(new
        {
            isAuthenticated,
            userName = isAuthenticated ? User.Identity!.Name : null
        });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            userName = User.Identity?.Name
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var user = await _context.Utilizadores
            .FirstOrDefaultAsync(u => u.Email == model.Email);

        var hasher = new PasswordHasher<Utilizador>();
        if (user == null || hasher.VerifyHashedPassword(user, user.Password, model.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Email ou Password incorretos." });

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Nome),
            new(ClaimTypes.Role, user.TipoUti == 1 ? "Admin" : "Utilizador")
        };

        var identity = new ClaimsIdentity(claims, "CookieAuth");
        await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(identity));

        return Ok(new { userName = user.Nome });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegistoModel model)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var emailExiste = await _context.Utilizadores.AnyAsync(u => u.Email == model.Email);
        if (emailExiste)
            return Conflict(new { message = "Ja existe uma conta com esse email." });

        await _registoService.RegistarAsync(model);

        var user = await _context.Utilizadores
            .FirstAsync(u => u.Email == model.Email);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Nome),
            new(ClaimTypes.Role, user.TipoUti == 1 ? "Admin" : "Utilizador")
        };

        var identity = new ClaimsIdentity(claims, "CookieAuth");
        await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(identity));

        return Ok(new { userName = user.Nome });
    }
}
