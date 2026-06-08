using System.ComponentModel.DataAnnotations;

namespace ES2.DTOs;

public class GerirUtilizadorAdminDto
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(100, ErrorMessage = "O nome não pode ter mais de 100 caracteres")]
    public string Nome { get; set; } = null!;

    [EmailAddress(ErrorMessage = "Email inválido")]
    public string? Email { get; set; }

    [StringLength(20, ErrorMessage = "Telemóvel não pode ter mais de 20 caracteres")]
    public string? Telemovel { get; set; }

    [Required(ErrorMessage = "O tipo de utilizador é obrigatório")]
    public int TipoUti { get; set; }
}
