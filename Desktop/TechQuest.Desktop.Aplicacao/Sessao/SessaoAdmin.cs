using TechQuest.Desktop.Aplicacao.Interfaces;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Aplicacao.Sessao;

/// <summary>
/// Sessao do administrador logado: token, validade e dados de exibicao.
///
/// O TOKEN FICA SO NA MEMORIA. Fechou o programa, acabou a sessao. E a mesma
/// decisao do web, que trocou localStorage por sessionStorage pensando no
/// laboratorio da faculdade: computador compartilhado nao deve guardar uma
/// credencial de administrador em disco.
///
/// ALTERNATIVA REJEITADA: gravar o token (Properties.Settings, arquivo ou
/// Credential Manager) para o admin nao ter de logar de novo. Ganha conforto,
/// mas e justamente o perfil com mais poder no sistema; a troca nao compensa.
///
/// UMA INSTANCIA PARA O PROGRAMA INTEIRO, criada em Program.cs e repassada
/// por construtor. Nao e um singleton estatico de proposito: estatico seria
/// acessivel de qualquer lugar e impossivel de substituir num teste.
/// </summary>
public sealed class SessaoAdmin : IProvedorDeToken
{
    private readonly TimeProvider _relogio;
    private string? _token;
    private DateTime _expiraEmUtc;

    /// <param name="relogio">
    /// Fonte da hora atual. Em producao, TimeProvider.System; num teste, um
    /// relogio controlado, para verificar a expiracao sem esperar 8 horas.
    /// </param>
    public SessaoAdmin(TimeProvider? relogio = null)
        => _relogio = relogio ?? TimeProvider.System;

    public UsuarioLogado? Usuario { get; private set; }

    public bool Ativa => _token is not null && _relogio.GetUtcNow().UtcDateTime < _expiraEmUtc;

    /// <summary>
    /// Devolve null depois do vencimento. Assim a requisicao sai sem token, a
    /// API responde 401, e o fluxo de "sessao expirada" e um so, venha a
    /// expiracao do relogio local ou de uma decisao do servidor.
    /// </summary>
    public string? Token => Ativa ? _token : null;

    /// <summary>
    /// Disparado quando uma requisicao descobre que a sessao nao vale mais.
    /// A janela principal escuta e volta para o login.
    /// </summary>
    public event EventHandler? Expirou;

    public void Iniciar(RespostaLogin resposta)
    {
        ArgumentNullException.ThrowIfNull(resposta);
        if (resposta.Usuario.Papel != PapelUsuario.Admin)
            throw new InvalidOperationException("Sessao do desktop e exclusiva de administrador.");

        _token = resposta.Token;
        _expiraEmUtc = resposta.ExpiraEm.ToUniversalTime();
        Usuario = resposta.Usuario;
    }

    /// <summary>Acompanha a troca de e-mail feita em Minha conta.</summary>
    public void AtualizarEmail(string email)
    {
        if (Usuario is not null) Usuario = Usuario with { Email = email };
    }

    public void Encerrar()
    {
        _token = null;
        _expiraEmUtc = default;
        Usuario = null;
    }

    /// <summary>Encerra e avisa quem estiver ouvindo.</summary>
    public void Expirar()
    {
        if (_token is null && Usuario is null) return; // ja encerrada: nao avisa duas vezes
        Encerrar();
        Expirou?.Invoke(this, EventArgs.Empty);
    }
}
