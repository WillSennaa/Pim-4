using TechQuest.Application.Dtos;
using TechQuest.Application.Interfaces;

namespace TechQuest.Application.Servicos;

public class EstudanteService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IHistoricoRepository _historicos;
    private readonly ICertificadoRepository _certificados;
    private readonly IProgressoRepository _progressos;
    private readonly GamificacaoService _gamificacao;
    private readonly IUnidadeDeTrabalho _uow;

    public EstudanteService(
        IUsuarioRepository usuarios, IHistoricoRepository historicos,
        ICertificadoRepository certificados, IProgressoRepository progressos,
        GamificacaoService gamificacao, IUnidadeDeTrabalho uow)
    {
        _usuarios = usuarios; _historicos = historicos; _certificados = certificados;
        _progressos = progressos; _gamificacao = gamificacao; _uow = uow;
    }

    public async Task<PerfilDto?> ObterPerfilAsync(
        int idUsuario, int? idEstudante, CancellationToken ct = default)
    {
        var u = await _usuarios.ObterPorIdAsync(idUsuario, ct);
        if (u is null) return null;

        var papel = u.Adm is not null ? "Admin" : u.Tutor is not null ? "Tutor" : "Estudante";

        var xp = idEstudante is null ? null : await _gamificacao.ObterAsync(idEstudante.Value, ct);
        var concluidos = idEstudante is null
            ? 0
            : (await _historicos.NomesDeCursosConcluidosAsync(idEstudante.Value, ct)).Count;

        return new PerfilDto(
            u.IdUsuario, u.Nome, u.Email, u.Iniciais(), papel,
            u.Telefone, u.DataNascimento, u.Cidade, u.DataCadastro,
            xp?.Xp ?? 0, xp?.Nivel ?? 1, xp?.XpProximoNivel ?? 0, xp?.ProgressoNivel ?? 0,
            concluidos,
            xp?.MateriaisConcluidos ?? 0, xp?.ProvasAprovadas ?? 0, xp?.Medalhas ?? 0);
    }

    public async Task<bool> AtualizarPerfilAsync(
        int idUsuario, AtualizarPerfilRequest req, CancellationToken ct = default)
    {
        var u = await _usuarios.ObterParaEdicaoAsync(idUsuario, ct);
        if (u is null) return false;

        // Nome e e-mail nao sao editaveis aqui: e-mail e a chave de login e
        // alterar exigiria fluxo proprio de confirmacao.
        u.Telefone = req.Telefone;
        u.DataNascimento = req.DataNascimento;
        u.Cidade = req.Cidade;

        await _uow.SalvarAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<ItemHistoricoDto>> ObterHistoricoAsync(
        int idEstudante, CancellationToken ct = default)
    {
        var historicos = await _historicos.ListarPorEstudanteAsync(idEstudante, ct);
        var itens = new List<ItemHistoricoDto>();

        foreach (var h in historicos)
        {
            var (total, concluidas) = await _progressos.ContarAulasDoCursoAsync(
                idEstudante, h.IdCurso, ct);

            itens.Add(new ItemHistoricoDto(
                h.IdHistorico, h.IdCurso, h.Curso.Nome, h.StatusConclusao, h.DataConclusao,
                concluidas, total,
                total == 0 ? 0 : (int)Math.Round(concluidas * 100.0 / total),
                h.Certificado?.CodigoAutenticacao));
        }

        return itens;
    }

    public async Task<IReadOnlyList<CertificadoDto>> ListarCertificadosAsync(
        int idEstudante, CancellationToken ct = default)
        => (await _certificados.ListarPorEstudanteAsync(idEstudante, ct))
            .Select(Mapear).ToList();

    /// <summary>Validacao publica de certificado pelo codigo impresso nele.</summary>
    public async Task<CertificadoDto?> ValidarAsync(Guid codigo, CancellationToken ct = default)
    {
        var c = await _certificados.ObterPorCodigoAsync(codigo, ct);
        return c is null ? null : Mapear(c);
    }

    private static CertificadoDto Mapear(Domain.Entidades.Certificado c) => new(
        c.IdCertificado,
        c.CodigoAutenticacao,
        c.Historico.IdCurso,
        c.Historico.Curso.Nome,
        c.Historico.Estudante.Usuario.Nome,
        c.DataEmissao,
        c.Historico.DataConclusao,
        c.Historico.Curso.DuracaoHoras);
}
