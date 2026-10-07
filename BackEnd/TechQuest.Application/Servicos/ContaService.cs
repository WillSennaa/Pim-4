using System.Text.RegularExpressions;
using TechQuest.Application.Dtos;
using TechQuest.Application.Interfaces;

namespace TechQuest.Application.Servicos;

/// <summary>
/// Operacoes sobre as CREDENCIAIS da propria conta: senha, e-mail e
/// encerramento.
///
/// POR QUE UM SERVICO SEPARADO: o EstudanteService cuida de dados academicos
/// (perfil, historico, certificados). Credencial e outra responsabilidade,
/// com outra regra de seguranca -- todas as operacoes daqui exigem a senha
/// atual, nenhuma das de la exige. Separar deixa essa fronteira visivel no
/// codigo, em vez de escondida dentro de uma classe que faz as duas coisas.
///
/// REGRA COMUM A TODAS: o usuario ja esta autenticado, e mesmo assim precisa
/// informar a senha. Um token roubado, um computador de laboratorio com a
/// sessao aberta ou alguem que sentou na cadeira do outro nao podem servir
/// para trocar a senha, transferir a conta pelo e-mail ou encerra-la.
/// </summary>
public class ContaService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IHashSenhaService _hash;
    private readonly IUnidadeDeTrabalho _uow;

    public ContaService(IUsuarioRepository usuarios, IHashSenhaService hash, IUnidadeDeTrabalho uow)
    {
        _usuarios = usuarios;
        _hash = hash;
        _uow = uow;
    }

    public async Task<Resultado<bool>> TrocarSenhaAsync(
        int idUsuario, TrocarSenhaRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.SenhaNova) || req.SenhaNova.Length < 6)
            return Resultado<bool>.Invalido("A nova senha deve ter ao menos 6 caracteres.");

        if (req.SenhaAtual == req.SenhaNova)
            return Resultado<bool>.Invalido("A nova senha deve ser diferente da atual.");

        var usuario = await _usuarios.ObterParaEdicaoAsync(idUsuario, ct);
        if (usuario is null) return Resultado<bool>.NaoEncontrado("Usuario nao encontrado.");

        if (string.IsNullOrEmpty(usuario.SenhaHash) ||
            !_hash.Verificar(req.SenhaAtual, usuario.SenhaHash))
            return Resultado<bool>.Invalido("A senha atual esta incorreta.");

        // So o hash da nova senha chega ao banco; o texto puro morre aqui.
        usuario.SenhaHash = _hash.GerarHash(req.SenhaNova);
        await _uow.SalvarAsync(ct);

        return Resultado<bool>.Ok(true);
    }

    public async Task<Resultado<string>> TrocarEmailAsync(
        int idUsuario, TrocarEmailRequest req, CancellationToken ct = default)
    {
        var novo = (req.EmailNovo ?? string.Empty).Trim().ToLowerInvariant();

        if (!Regex.IsMatch(novo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return Resultado<string>.Invalido("Informe um e-mail valido.");

        var usuario = await _usuarios.ObterParaEdicaoAsync(idUsuario, ct);
        if (usuario is null) return Resultado<string>.NaoEncontrado("Usuario nao encontrado.");

        if (string.IsNullOrEmpty(usuario.SenhaHash) ||
            !_hash.Verificar(req.Senha, usuario.SenhaHash))
            return Resultado<string>.Invalido("Senha incorreta.");

        if (string.Equals(usuario.Email, novo, StringComparison.OrdinalIgnoreCase))
            return Resultado<string>.Invalido("O novo e-mail e igual ao atual.");

        // A coluna tem indice unico: sem esta checagem o erro viria do banco
        // como violacao de constraint, sem mensagem util para o usuario.
        if (await _usuarios.EmailExisteAsync(novo, ct))
            return Resultado<string>.Invalido("Ja existe uma conta com este e-mail.");

        usuario.Email = novo;
        await _uow.SalvarAsync(ct);

        return Resultado<string>.Ok(novo);
    }

    /// <summary>
    /// Encerra a propria conta.
    ///
    /// DESATIVA, NAO APAGA. Historico, desempenho, conquistas e certificados
    /// apontam para o usuario por chave estrangeira: apagar exigiria remover
    /// em cascata o registro academico de quem concluiu cursos -- inclusive os
    /// certificados ja emitidos, que terceiros podem estar validando pelo
    /// codigo publico. Desativar encerra o acesso e preserva o que aconteceu.
    ///
    /// EFEITO COLATERAL DELIBERADO: e um UPDATE em Status_Usuario, entao a
    /// trigger TRG_Auditoria_StatusUsuario grava a linha de auditoria sozinha.
    ///
    /// LIMITE CONHECIDO: o JWT ja emitido continua tecnicamente valido ate
    /// expirar, porque o token e autocontido e o servidor nao guarda sessao.
    /// O cliente encerra a sessao na hora; invalidar o token no servidor
    /// exigiria uma lista de revogacao, que troca a simplicidade do modelo
    /// sem estado por um estado que ele nao tem hoje.
    /// </summary>
    public async Task<Resultado<bool>> DesativarContaAsync(
        int idUsuario, DesativarContaRequest req, CancellationToken ct = default)
    {
        var usuario = await _usuarios.ObterParaEdicaoAsync(idUsuario, ct);
        if (usuario is null) return Resultado<bool>.NaoEncontrado("Usuario nao encontrado.");

        if (string.IsNullOrEmpty(usuario.SenhaHash) ||
            !_hash.Verificar(req.Senha, usuario.SenhaHash))
            return Resultado<bool>.Invalido("Senha incorreta.");

        if (!usuario.Ativo) return Resultado<bool>.Ok(true);

        // Um administrador nao encerra a propria conta por aqui: a plataforma
        // pode ficar sem ninguem para reativar usuarios. A mesma regra ja
        // vale no AdminService.
        if (usuario.Adm is not null)
            return Resultado<bool>.Invalido(
                "Contas de administrador devem ser desativadas por outro administrador.");

        usuario.Ativo = false;
        await _uow.SalvarAsync(ct);

        return Resultado<bool>.Ok(true);
    }
}
