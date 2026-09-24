using TechQuest.Domain.Enums;

namespace TechQuest.Application.Dtos;

public record LoginRequest(string Email, string Senha);

/// <summary>
/// Cadastro publico. NAO tem campo de papel de proposito: quem se cadastra
/// pela tela de login e sempre Estudante. Se o papel viesse no corpo da
/// requisicao, qualquer pessoa se cadastraria como Admin.
/// </summary>
public record RegistrarRequest(string Nome, string Email, string Senha);

public record LoginResponse(
    string Token,
    DateTime ExpiraEm,
    UsuarioLogadoDto Usuario);

/// <summary>
/// Espelha o que o front-end do PIM III le de mock.json > personas.
/// Xp, Nivel e ProgressoNivel sao calculados, nao lidos do banco.
/// </summary>
public record UsuarioLogadoDto(
    int Id,
    string Nome,
    string? Email,
    string Iniciais,
    PapelUsuario Papel,
    int? IdEstudante,
    int Xp,
    int Nivel,
    int XpProximoNivel,
    int ProgressoNivel);
