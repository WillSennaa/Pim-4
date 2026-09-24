namespace TechQuest.Domain.Enums;

/// <summary>
/// Papel do usuario autenticado. Nao existe como coluna no banco: e derivado
/// da especializacao preenchida (ADM, Tutor ou Estudante), que e exatamente
/// como o modelo do PIM III representa os tres perfis.
/// </summary>
public enum PapelUsuario
{
    Estudante = 1,
    Tutor = 2,
    Admin = 3
}
