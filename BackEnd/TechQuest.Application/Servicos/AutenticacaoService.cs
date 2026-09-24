using System.Text.RegularExpressions;
using TechQuest.Application.Dtos;
using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;
using TechQuest.Domain.Enums;

namespace TechQuest.Application.Servicos;

public class AutenticacaoService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly GamificacaoService _gamificacao;
    private readonly IHashSenhaService _hash;
    private readonly ITokenService _token;
    private readonly IUnidadeDeTrabalho _uow;

    public AutenticacaoService(
        IUsuarioRepository usuarios,
        GamificacaoService gamificacao,
        IHashSenhaService hash,
        ITokenService token,
        IUnidadeDeTrabalho uow)
    {
        _usuarios = usuarios;
        _gamificacao = gamificacao;
        _hash = hash;
        _token = token;
        _uow = uow;
    }

    public async Task<LoginResponse?> AutenticarAsync(LoginRequest req, CancellationToken ct = default)
    {
        var usuario = await _usuarios.ObterPorEmailAsync(Normalizar(req.Email), ct);

        // Mesma resposta para email inexistente e senha errada: nao revela
        // quais emails estao cadastrados (evita enumeracao de usuarios).
        if (usuario is null || string.IsNullOrEmpty(usuario.SenhaHash)) return null;
        if (!usuario.Ativo) return null;
        if (!_hash.Verificar(req.Senha, usuario.SenhaHash)) return null;

        return await MontarRespostaAsync(usuario, ct);
    }

    /// <summary>
    /// Cadastro publico pela tela de login. O papel e fixado como Estudante no
    /// servidor: o cliente nao escolhe. Contas de Tutor e Admin continuam
    /// nascendo apenas pela area administrativa.
    /// </summary>
    public async Task<Resultado<LoginResponse>> RegistrarAsync(
        RegistrarRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Nome))
            return Resultado<LoginResponse>.Invalido("Informe seu nome.");

        var email = Normalizar(req.Email);
        if (!EmailValido(email))
            return Resultado<LoginResponse>.Invalido("Informe um e-mail valido.");

        if (string.IsNullOrWhiteSpace(req.Senha) || req.Senha.Length < 6)
            return Resultado<LoginResponse>.Invalido("A senha deve ter ao menos 6 caracteres.");

        if (await _usuarios.EmailExisteAsync(email, ct))
            return Resultado<LoginResponse>.Invalido("Ja existe uma conta com este e-mail.");

        var usuario = new Usuario
        {
            Nome = req.Nome.Trim(),
            Email = email,
            // A senha em texto puro morre aqui: so o hash vai para o banco.
            SenhaHash = _hash.GerarHash(req.Senha),
            DataCadastro = DateTime.UtcNow,
            Ativo = true
        };

        await _uow.ExecutarEmTransacaoAsync(async () =>
        {
            _usuarios.Adicionar(usuario);
            await _uow.SalvarAsync(ct);

            // Usuario e especializacao nascem juntos: sem a linha em Estudante
            // a conta ficaria sem papel e sem acesso a nada.
            _usuarios.Adicionar(new Estudante { IdUsuario = usuario.IdUsuario });
            await _uow.SalvarAsync(ct);
        }, ct);

        // Relê para trazer a especializacao recem-criada junto do usuario.
        var completo = await _usuarios.ObterPorIdAsync(usuario.IdUsuario, ct);

        // Ja devolve o token: quem acabou de se cadastrar entra direto, sem
        // precisar digitar a senha de novo na tela seguinte.
        return Resultado<LoginResponse>.Ok(await MontarRespostaAsync(completo ?? usuario, ct));
    }

    private async Task<LoginResponse> MontarRespostaAsync(Usuario usuario, CancellationToken ct)
    {
        var papel = DeterminarPapel(usuario);
        var (token, expira) = _token.Gerar(usuario, papel);

        var idEstudante = usuario.Estudante?.IdEstudante;
        var xp = idEstudante is null ? null : await _gamificacao.ObterAsync(idEstudante.Value, ct);

        var dto = new UsuarioLogadoDto(
            usuario.IdUsuario, usuario.Nome, usuario.Email, usuario.Iniciais(),
            papel, idEstudante,
            xp?.Xp ?? 0, xp?.Nivel ?? 1, xp?.XpProximoNivel ?? 0, xp?.ProgressoNivel ?? 0);

        return new LoginResponse(token, expira, dto);
    }

    /// <summary>
    /// O papel vem da especializacao preenchida. Se um usuario tiver mais de uma,
    /// vale a de maior privilegio — decisao explicita, e nao acidente de ordem.
    /// </summary>
    private static PapelUsuario DeterminarPapel(Usuario u)
    {
        if (u.Adm is not null) return PapelUsuario.Admin;
        if (u.Tutor is not null) return PapelUsuario.Tutor;
        return PapelUsuario.Estudante;
    }

    private static string Normalizar(string? email) => (email ?? string.Empty).Trim().ToLowerInvariant();

    private static bool EmailValido(string email)
        => !string.IsNullOrWhiteSpace(email)
           && Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
}
