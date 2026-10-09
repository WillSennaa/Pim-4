using TechQuest.Desktop.Aplicacao.Comum;
using TechQuest.Desktop.Aplicacao.Interfaces;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Aplicacao.Validacao;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Aplicacao.Servicos;

/// <summary>
/// Conta do proprio administrador: ver os dados, trocar senha e trocar e-mail.
///
/// AMBAS AS TROCAS EXIGEM A SENHA ATUAL, mesmo com a sessao aberta. E regra do
/// servidor (PerfilController): um computador esquecido logado nao pode
/// permitir que outra pessoa tome a conta trocando o e-mail de login.
///
/// NAO HA "DESATIVAR MINHA CONTA": a API recusa para administrador
/// (ContaService.DesativarContaAsync), para a plataforma nunca ficar sem
/// alguem que reative usuarios. Outro admin desativa pela tela Usuarios.
/// </summary>
public sealed class ContaService
{
    private readonly IContaGateway _gateway;
    private readonly SessaoAdmin _sessao;

    public ContaService(IContaGateway gateway, SessaoAdmin sessao)
    {
        _gateway = gateway;
        _sessao = sessao;
    }

    public Task<PerfilUsuario> ObterPerfilAsync(CancellationToken ct = default)
        => _gateway.ObterPerfilAsync(ct);

    public async Task<ResultadoOperacao> TrocarSenhaAsync(
        string? atual, string? nova, string? confirmacao, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(atual))
            return ResultadoOperacao.Falha("Informe a senha atual.");
        if (string.IsNullOrEmpty(nova) || nova.Length < UsuariosService.TamanhoMinimoSenha)
            return ResultadoOperacao.Falha($"A nova senha deve ter ao menos {UsuariosService.TamanhoMinimoSenha} caracteres.");
        if (nova.Length > UsuariosService.TamanhoMaximoSenha)
            return ResultadoOperacao.Falha($"A nova senha pode ter no máximo {UsuariosService.TamanhoMaximoSenha} caracteres.");
        // A confirmacao so existe no cliente: o servidor recebe a senha uma
        // vez. Ela pega o erro de digitacao que trancaria o admin fora.
        if (nova != confirmacao)
            return ResultadoOperacao.Falha("A confirmação não confere com a nova senha.");
        if (nova == atual)
            return ResultadoOperacao.Falha("A nova senha deve ser diferente da atual.");

        await _gateway.TrocarSenhaAsync(new TrocaSenha(atual, nova), ct);
        return ResultadoOperacao.Ok();
    }

    public async Task<ResultadoOperacao<string>> TrocarEmailAsync(
        string? emailNovo, string? senha, CancellationToken ct = default)
    {
        var novo = emailNovo?.Trim() ?? "";

        if (!ValidadorEmail.EhValido(novo))
            return ResultadoOperacao<string>.Falha("Informe um e-mail válido.");
        if (novo.Length > UsuariosService.TamanhoMaximoEmail)
            return ResultadoOperacao<string>.Falha($"O e-mail pode ter no máximo {UsuariosService.TamanhoMaximoEmail} caracteres.");
        if (string.Equals(novo, _sessao.Usuario?.Email, StringComparison.OrdinalIgnoreCase))
            return ResultadoOperacao<string>.Falha("O novo e-mail é igual ao atual.");
        if (string.IsNullOrEmpty(senha))
            return ResultadoOperacao<string>.Falha("Confirme com a sua senha atual.");

        var gravado = await _gateway.TrocarEmailAsync(new TrocaEmail(senha, novo), ct);

        // O token continua valido (ele carrega o ID, nao o e-mail), entao nao
        // ha novo login; so a copia local do e-mail precisa acompanhar.
        _sessao.AtualizarEmail(gravado);
        return ResultadoOperacao<string>.Ok(gravado);
    }
}
