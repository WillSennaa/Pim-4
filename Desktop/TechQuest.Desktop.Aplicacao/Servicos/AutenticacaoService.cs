using TechQuest.Desktop.Aplicacao.Comum;
using TechQuest.Desktop.Aplicacao.Erros;
using TechQuest.Desktop.Aplicacao.Interfaces;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Aplicacao.Validacao;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Aplicacao.Servicos;

/// <summary>
/// Caso de uso "entrar no desktop" e "sair".
///
/// A REGRA QUE SO EXISTE AQUI: o desktop e exclusivo do administrador. A API
/// autentica qualquer papel em /api/auth/login (o mesmo endpoint serve web e
/// mobile), entao e o cliente que decide quem pode abrir ESTA aplicacao.
///
/// ISSO NAO E A BARREIRA DE SEGURANCA. Mesmo que alguem burlasse esta
/// checagem, toda rota /api/admin exige o papel Admin no token e devolveria
/// 403. A checagem local existe para dar ao tutor ou ao estudante uma
/// mensagem clara na hora, em vez de deixa-lo entrar numa janela em que
/// nenhuma tela carrega.
/// </summary>
public sealed class AutenticacaoService
{
    private readonly IAutenticacaoGateway _gateway;
    private readonly SessaoAdmin _sessao;

    public AutenticacaoService(IAutenticacaoGateway gateway, SessaoAdmin sessao)
    {
        _gateway = gateway;
        _sessao = sessao;
    }

    public async Task<ResultadoOperacao> EntrarAsync(
        string? email, string? senha, CancellationToken ct = default)
    {
        // A senha NAO passa por Trim: espaco pode fazer parte dela.
        var emailLimpo = email?.Trim() ?? string.Empty;

        if (emailLimpo.Length == 0 || string.IsNullOrEmpty(senha))
            return ResultadoOperacao.Falha("Informe e-mail e senha.");

        if (!ValidadorEmail.EhValido(emailLimpo))
            return ResultadoOperacao.Falha("O e-mail informado não está em um formato válido.");

        RespostaLogin resposta;
        try
        {
            resposta = await _gateway.EntrarAsync(new CredenciaisLogin(emailLimpo, senha), ct);
        }
        catch (NaoAutenticadoException)
        {
            // Mensagem generica, como a da API: nao revela se o e-mail existe.
            return ResultadoOperacao.Falha("E-mail ou senha inválidos.");
        }

        if (resposta.Usuario.Papel != PapelUsuario.Admin)
        {
            // O token recebido e simplesmente descartado: nunca chega a sessao.
            return ResultadoOperacao.Falha(
                "Esta aplicação é exclusiva para administradores. " +
                "Tutores e estudantes acessam a plataforma pelo site ou pelo aplicativo.");
        }

        _sessao.Iniciar(resposta);
        return ResultadoOperacao.Ok();
    }

    /// <summary>
    /// Logout local. Nao ha chamada a API porque o JWT e sem estado: o
    /// servidor nao guarda sessoes para invalidar. Descartar o token no
    /// cliente encerra o acesso; ele venceria sozinho no prazo de validade.
    /// </summary>
    public void Sair() => _sessao.Encerrar();
}
