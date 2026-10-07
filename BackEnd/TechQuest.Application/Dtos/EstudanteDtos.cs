namespace TechQuest.Application.Dtos;

// ----- Progresso em aula -----
public record AtualizarProgressoRequest(bool Concluido, int PorcentagemAssistida);

public record ProgressoAtualizadoDto(
    int IdMaterial,
    bool Concluido,
    int PorcentagemAssistida,
    int AulasConcluidas,
    int TotalAulas,
    int PercentualCurso,
    bool CursoConcluido,
    int XpAtual,
    int Nivel,
    IReadOnlyList<string> MedalhasNovas);

// ----- Matricula / historico -----
public record MatriculaDto(int IdHistorico, int IdCurso, string Curso, string Status);

public record ItemHistoricoDto(
    int IdHistorico,
    int IdCurso,
    string Curso,
    string? Status,
    DateOnly? DataConclusao,
    int AulasConcluidas,
    int TotalAulas,
    int PercentualCurso,
    Guid? CodigoCertificado);

// ----- Perfil -----
public record PerfilDto(
    int Id,
    string Nome,
    string? Email,
    string Iniciais,
    string Papel,
    string? Telefone,
    DateOnly? DataNascimento,
    string? Cidade,
    DateTime DataCadastro,
    int Xp,
    int Nivel,
    int XpProximoNivel,
    int ProgressoNivel,
    int CursosConcluidos,
    int AulasConcluidas,
    int ProvasAprovadas,
    int Medalhas);

public record AtualizarPerfilRequest(string? Telefone, DateOnly? DataNascimento, string? Cidade);

// ----- Conquistas -----
public record MedalhaDto(
    int Id,
    string Nome,
    string? Descricao,
    string? Raridade,
    bool Conquistada,
    DateTime? DataConquista);

// ----- Certificados -----
public record CertificadoDto(
    int Id,
    Guid CodigoAutenticacao,
    int IdCurso,
    string Curso,
    string Estudante,
    DateTime DataEmissao,
    DateOnly? DataConclusao,
    int? CargaHoraria);

// ----- Conta do proprio usuario -----
/// <summary>
/// Troca de senha. A senha ATUAL e exigida mesmo com o usuario ja autenticado:
/// um token roubado ou uma sessao esquecida aberta nao podem servir para
/// tomar a conta. E a mesma razao pela qual bancos pedem a senha de novo em
/// operacoes sensiveis.
/// </summary>
public record TrocarSenhaRequest(string SenhaAtual, string SenhaNova);

/// <summary>
/// Troca de e-mail. Tambem exige a senha: o e-mail e a chave de login, entao
/// alterar o e-mail equivale a transferir a conta.
/// </summary>
public record TrocarEmailRequest(string Senha, string EmailNovo);

/// <summary>
/// Encerramento da propria conta. Exige a senha pelo mesmo motivo.
/// </summary>
public record DesativarContaRequest(string Senha);
