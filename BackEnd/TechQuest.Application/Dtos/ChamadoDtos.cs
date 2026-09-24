namespace TechQuest.Application.Dtos;

public record AbrirChamadoRequest(string Tipo, string Assunto, string Descricao, int? IdDestinatario);

public record ChamadoDto(
    int Id,
    string? Tipo,
    string? Assunto,
    string? Descricao,
    string Remetente,
    string? Destinatario,
    DateTime DataAbertura,
    string Status);

public record ResponderChamadoRequest(string Descricao);
