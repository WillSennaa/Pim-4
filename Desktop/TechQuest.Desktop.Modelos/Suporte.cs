namespace TechQuest.Desktop.Modelos;

/// <summary>Situacoes de chamado aceitas pela API (ChamadoService.AtualizarStatusAsync).</summary>
public static class StatusChamado
{
    public const string Aberto = "Aberto";
    public const string EmAndamento = "Em andamento";
    public const string Resolvido = "Resolvido";
}

/// <summary>Linha de GET /api/admin/chamados (fila tecnica).</summary>
public record Chamado(
    int Id,
    string? Tipo,
    string? Assunto,
    string? Descricao,
    int IdRemetente,
    string Remetente,
    int? IdDestinatario,
    string? Destinatario,
    DateTime DataAbertura,
    string Status,
    string? Curso);

/// <summary>Corpo de POST /api/chamados/{id}/responder.</summary>
public record RespostaChamado(string Descricao);

/// <summary>Corpo de PATCH /api/chamados/{id}/status.</summary>
public record AlteracaoStatusChamado(string Status);

/// <summary>
/// Raridades do catalogo, na grafia em minusculas do script de carga
/// (02_dados_demonstracao.sql). Sao as que o web reconhece para colorir.
///
/// O servidor grava "Comum" (maiusculo) quando a raridade chega vazia; por
/// isso o desktop SEMPRE envia uma das quatro, e compara sem diferenciar
/// maiusculas ao ler.
/// </summary>
public static class Raridades
{
    public const string Comum = "comum";
    public const string Raro = "raro";
    public const string Epico = "epico";
    public const string Lendario = "lendario";

    public static readonly IReadOnlyList<string> Todas = new[] { Comum, Raro, Epico, Lendario };

    /// <summary>Valor canonico, ou null se nao for uma das quatro.</summary>
    public static string? Normalizar(string? raridade)
        => Todas.FirstOrDefault(r => string.Equals(r, raridade?.Trim(), StringComparison.OrdinalIgnoreCase));
}

/// <summary>Linha de GET /api/admin/medalhas.</summary>
public record MedalhaAdmin(int Id, string Nome, string? Raridade, string? Descricao, int TotalConquistas);

/// <summary>Corpo de POST e PUT /api/admin/medalhas.</summary>
public record DadosMedalha(string Nome, string? Raridade, string? Descricao);

/// <summary>
/// GET /api/perfil, so com o que a tela Minha conta usa. Os campos de
/// gamificacao (Xp, Nivel...) fazem sentido para estudante e sao ignorados.
/// </summary>
public record PerfilUsuario(
    int Id,
    string Nome,
    string? Email,
    string? Iniciais,
    string? Papel,
    string? Telefone,
    string? Cidade,
    DateTime DataCadastro);

/// <summary>Corpo de PUT /api/perfil/senha.</summary>
public record TrocaSenha(string SenhaAtual, string SenhaNova);

/// <summary>Corpo de PUT /api/perfil/email.</summary>
public record TrocaEmail(string Senha, string EmailNovo);

/// <summary>Resposta de PUT /api/perfil/email: o e-mail ja normalizado pelo servidor.</summary>
public record EmailAtualizado(string Email);
