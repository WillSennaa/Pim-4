namespace TechQuest.Domain.Entidades;

public class Curso
{
    public int IdCurso { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Categoria { get; set; }
    public string? Nivel { get; set; }
    public int? DuracaoHoras { get; set; }
    public string Status { get; set; } = "Rascunho";

    public int? IdTutorCriou { get; set; }
    public Tutor? TutorCriou { get; set; }

    public int? IdAdmAvaliou { get; set; }
    public Adm? AdmAvaliou { get; set; }

    /// <summary>
    /// O modelo do PIM III coloca a FK da prova no curso (1 curso : 1 prova).
    /// Limitacao conhecida e documentada; nao foi alterada no PIM IV.
    /// </summary>
    public int? IdProva { get; set; }
    public Prova? Prova { get; set; }

    public ICollection<Material> Materiais { get; set; } = new List<Material>();
    public ICollection<Historico> Historicos { get; set; } = new List<Historico>();

    public bool EstaPublicado() => Status.Equals("Publicado", StringComparison.OrdinalIgnoreCase);
}
