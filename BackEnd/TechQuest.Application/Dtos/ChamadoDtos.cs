namespace TechQuest.Application.Dtos;

public record AbrirChamadoRequest(string Tipo, string Assunto, string Descricao, int? IdDestinatario);

/// <summary>
/// Os IDs de remetente e destinatario acompanham os nomes porque quem recebe
/// o chamado precisa poder RESPONDER a quem o abriu. Sem o ID, a tela teria de
/// adivinhar o destinatario pelo nome — que nao e unico nem estavel.
/// </summary>
public record ChamadoDto(
    int Id,
    string? Tipo,
    string? Assunto,
    string? Descricao,
    int IdRemetente,
    string Remetente,
    int? IdDestinatario,
    string? Destinatario,
    DateTime DataAbertura,
    string Status);

public record ResponderChamadoRequest(string Descricao);
