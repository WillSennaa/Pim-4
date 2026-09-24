using TechQuest.Application.Dtos;
using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;

namespace TechQuest.Application.Servicos;

public class ChamadoService
{
    private readonly IChamadoRepository _chamados;
    private readonly IUnidadeDeTrabalho _uow;

    public ChamadoService(IChamadoRepository chamados, IUnidadeDeTrabalho uow)
    {
        _chamados = chamados;
        _uow = uow;
    }

    public async Task<ChamadoDto> AbrirAsync(
        int idRemetente, AbrirChamadoRequest req, CancellationToken ct = default)
    {
        var chamado = new Chamado
        {
            IdRemetente = idRemetente,
            IdDestinatario = req.IdDestinatario,
            Tipo = req.Tipo,
            Assunto = req.Assunto,
            Descricao = req.Descricao,
            DataAbertura = DateTime.UtcNow,
            Status = "Aberto"
        };

        _chamados.Adicionar(chamado);
        await _uow.SalvarAsync(ct);

        var salvo = await _chamados.ObterAsync(chamado.IdChamado, ct);
        return Mapear(salvo!);
    }

    public async Task<IReadOnlyList<ChamadoDto>> ListarAsync(
        int idUsuario, CancellationToken ct = default)
        => (await _chamados.ListarDoUsuarioAsync(idUsuario, ct)).Select(Mapear).ToList();

    /// <summary>
    /// Fecha o chamado. So o remetente ou o destinatario podem fechar — a
    /// verificacao fica aqui, nao no controller, porque e regra de negocio.
    /// </summary>
    public async Task<bool> FecharAsync(int idChamado, int idUsuario, CancellationToken ct = default)
    {
        var chamado = await _chamados.ObterAsync(idChamado, ct);
        if (chamado is null) return false;
        if (chamado.IdRemetente != idUsuario && chamado.IdDestinatario != idUsuario) return false;

        chamado.Status = "Fechado";
        await _uow.SalvarAsync(ct);
        return true;
    }

    private static ChamadoDto Mapear(Chamado c) => new(
        c.IdChamado, c.Tipo, c.Assunto, c.Descricao,
        c.Remetente.Nome, c.Destinatario?.Nome,
        c.DataAbertura, c.Status);
}
