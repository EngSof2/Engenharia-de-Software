using System.ComponentModel.DataAnnotations;

namespace ES2.DTOs;

public class CriarBilheteDto
{
    [Required(ErrorMessage = "Seleciona um evento.")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleciona um evento.")]
    [Display(Name = "Evento")]
    public int IdEvento { get; set; }

    [Required(ErrorMessage = "O nome do bilhete é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome do bilhete não pode ter mais de 100 caracteres.")]
    [Display(Name = "Nome do bilhete")]
    public string Nome { get; set; } = string.Empty;

    public int? IdTipo { get; set; }

    [StringLength(100, ErrorMessage = "O nome do tipo não pode ter mais de 100 caracteres.")]
    [Display(Name = "Novo tipo de bilhete")]
    public string? NovoTipo { get; set; }

    [Required(ErrorMessage = "O preço é obrigatório.")]
    [Range(0, 999999.99, ErrorMessage = "O preço deve ser igual ou superior a 0.")]
    [Display(Name = "Preço")]
    public decimal? Preco { get; set; }
}
