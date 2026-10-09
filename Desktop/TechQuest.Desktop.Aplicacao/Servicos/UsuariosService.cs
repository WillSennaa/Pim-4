using TechQuest.Desktop.Aplicacao.Comum;
using TechQuest.Desktop.Aplicacao.Interfaces;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Aplicacao.Validacao;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Aplicacao.Servicos;

/// <summary>
/// Gestao de contas (RF01): listar, criar com papel definido e ativar ou
/// desativar. Conta nunca e apagada: Usuario e referenciado por Historico,
/// Progresso, Chamado e Log_Auditoria, e apagar quebraria esse rastro.
/// </summary>
public sealed class UsuariosService
{
    // Limites das colunas Nome_Usuario e Email_Usuario (VARCHAR(100)) e da
    // regra de senha do servidor (AdminService.CriarUsuarioAsync: >= 6).
    public const int TamanhoMaximoNome = 100;
    public const int TamanhoMaximoEmail = 100;
    public const int TamanhoMinimoSenha = 6;
    public const int TamanhoMaximoSenha = 100;

    private readonly IUsuariosGateway _gateway;
    private readonly SessaoAdmin _sessao;

    public UsuariosService(IUsuariosGateway gateway, SessaoAdmin sessao)
    {
        _gateway = gateway;
        _sessao = sessao;
    }

    public Task<IReadOnlyList<UsuarioAdmin>> ListarAsync(CancellationToken ct = default)
        => _gateway.ListarAsync(ct);

    /// <summary>
    /// Valida no cliente o que da para validar sem ir ao banco (formato,
    /// tamanho); o que depende do banco (e-mail repetido) volta do servidor
    /// como OperacaoRecusadaException.
    /// </summary>
    public async Task<ResultadoOperacao<UsuarioAdmin>> CriarAsync(
        string? nome, string? email, string? senha, PapelUsuario papel, CancellationToken ct = default)
    {
        var n = nome?.Trim() ?? "";
        var e = email?.Trim() ?? "";
        var s = senha ?? "";

        if (n.Length < 3)
            return ResultadoOperacao<UsuarioAdmin>.Falha("Informe o nome completo (mínimo de 3 caracteres).");
        if (n.Length > TamanhoMaximoNome)
            return ResultadoOperacao<UsuarioAdmin>.Falha($"O nome pode ter no máximo {TamanhoMaximoNome} caracteres.");
        if (!ValidadorEmail.EhValido(e))
            return ResultadoOperacao<UsuarioAdmin>.Falha("Informe um e-mail válido.");
        if (e.Length > TamanhoMaximoEmail)
            return ResultadoOperacao<UsuarioAdmin>.Falha($"O e-mail pode ter no máximo {TamanhoMaximoEmail} caracteres.");
        if (s.Length < TamanhoMinimoSenha)
            return ResultadoOperacao<UsuarioAdmin>.Falha($"A senha deve ter ao menos {TamanhoMinimoSenha} caracteres.");
        if (s.Length > TamanhoMaximoSenha)
            return ResultadoOperacao<UsuarioAdmin>.Falha($"A senha pode ter no máximo {TamanhoMaximoSenha} caracteres.");

        var criado = await _gateway.CriarAsync(new NovoUsuario(n, e, s, papel.ToString()), ct);
        return ResultadoOperacao<UsuarioAdmin>.Ok(criado);
    }

    /// <summary>
    /// Ativa ou desativa. A trigger TRG_Auditoria_StatusUsuario registra a
    /// mudanca no Log_Auditoria sozinha; o desktop nao grava log nenhum.
    /// </summary>
    public async Task<ResultadoOperacao> AlterarStatusAsync(
        UsuarioAdmin alvo, bool ativo, CancellationToken ct = default)
    {
        if (!ativo && EhOProprioAdmin(alvo))
            return ResultadoOperacao.Falha(
                "Você não pode desativar a própria conta: a plataforma poderia ficar sem administrador ativo.");

        await _gateway.AlterarStatusAsync(alvo.Id, ativo, ct);
        return ResultadoOperacao.Ok();
    }

    /// <summary>
    /// O servidor tambem recusa a autodesativacao. A checagem aqui serve para
    /// a tela desabilitar o botao antes do clique, em vez de deixar clicar e
    /// mostrar erro.
    /// </summary>
    public bool EhOProprioAdmin(UsuarioAdmin alvo) => _sessao.Usuario?.Id == alvo.Id;
}
