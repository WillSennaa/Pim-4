using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Aplicacao.Interfaces;

/// <summary>
/// Fila de suporte tecnico (RF10). A listagem vem de /api/admin/chamados; as
/// acoes usam /api/chamados/{id}/..., as mesmas rotas do tutor, porque o
/// servidor ja autoriza qualquer admin em chamado do tipo Tecnico.
/// </summary>
public interface IChamadosGateway
{
    Task<IReadOnlyList<Chamado>> ListarTecnicosAsync(CancellationToken ct = default);
    Task AlterarStatusAsync(int idChamado, string status, CancellationToken ct = default);

    /// <summary>Envia a resposta ao remetente e marca o chamado como Resolvido.</summary>
    Task ResponderAsync(int idChamado, string texto, CancellationToken ct = default);
}

/// <summary>Catalogo de medalhas (/api/admin/medalhas). Sem exclusao: a API nao tem a rota.</summary>
public interface IMedalhasGateway
{
    Task<IReadOnlyList<MedalhaAdmin>> ListarAsync(CancellationToken ct = default);
    Task<MedalhaAdmin> CriarAsync(DadosMedalha dados, CancellationToken ct = default);
    Task<MedalhaAdmin> AtualizarAsync(int idMedalha, DadosMedalha dados, CancellationToken ct = default);
}

/// <summary>Conta do proprio usuario logado (/api/perfil). O ID sai do token.</summary>
public interface IContaGateway
{
    Task<PerfilUsuario> ObterPerfilAsync(CancellationToken ct = default);
    Task TrocarSenhaAsync(TrocaSenha troca, CancellationToken ct = default);

    /// <returns>O e-mail como ficou gravado (o servidor normaliza para minusculas).</returns>
    Task<string> TrocarEmailAsync(TrocaEmail troca, CancellationToken ct = default);
}
