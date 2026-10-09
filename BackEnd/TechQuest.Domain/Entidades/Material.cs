namespace TechQuest.Domain.Entidades;

public class Material
{
    public int IdMaterial { get; set; }
    public int IdCurso { get; set; }
    public Curso Curso { get; set; } = null!;

    public string? Titulo { get; set; }
    public string? Tipo { get; set; }

    /// <summary>
    /// Texto do material. Sem esta coluna a tela de aula exibia o mesmo
    /// conteudo escrito no HTML para qualquer material -- o titulo vinha do
    /// banco e o corpo era sempre o mesmo. NULL e valido: material recem
    /// criado pelo tutor nasce vazio e a tela diz isso.
    /// </summary>
    public string? Conteudo { get; set; }

    public ICollection<Progresso> Progressos { get; set; } = new List<Progresso>();
}
