namespace TechQuest.Desktop.Modelos;

/// <summary>
/// Papel do usuario, com os MESMOS valores numericos do enum do servidor
/// (TechQuest.Domain.Enums.PapelUsuario).
///
/// POR QUE OS VALORES IMPORTAM: a API serializa o enum como numero
/// ("papel": 3), sem conversor de texto. Se a ordem aqui fosse diferente,
/// um Tutor seria lido como Admin. Os valores sao explicitos de proposito,
/// para que reordenar as linhas nao mude o significado.
///
/// ALTERNATIVA REJEITADA: referenciar o enum do Domain do servidor. Ver
/// README da pasta Desktop: o contrato entre cliente e servidor e o JSON, e
/// o desktop nao depende de assembly interno da API.
/// </summary>
public enum PapelUsuario
{
    Estudante = 1,
    Tutor = 2,
    Admin = 3
}
