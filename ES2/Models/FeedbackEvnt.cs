using System;
using System.Collections.Generic;

namespace ES2.Models;

public partial class FeedbackEvnt
{
    public int IdFbevnt { get; set; }

    public int IdEvento { get; set; }

    public int IdUti { get; set; }

    public string Descricao { get; set; } = null!;

    /// <summary>Classificacao em estrelas atribuida pelo utilizador (1 a 5).</summary>
    public int Classificacao { get; set; }

    public virtual Evento IdEventoNavigation { get; set; } = null!;

    public virtual Utilizador IdUtiNavigation { get; set; } = null!;
}
