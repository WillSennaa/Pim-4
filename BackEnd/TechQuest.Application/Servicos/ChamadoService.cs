using TechQuest.Application.Dtos;
using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;

namespace TechQuest.Application.Servicos;

/// <summary>
/// Chamados: duvida educacional e suporte tecnico.
///
/// O MODELO TEM UMA TABELA SO, e isso e proposital: Chamado ja carrega
/// remetente, destinatario, tipo, assunto, descricao, data e status. Criar
/// uma tabela para duvida e outra para suporte duplicaria todas essas colunas
/// para mudar apenas QUEM responde. O que separa os dois e o campo Tipo e o
/// roteamento -- nao a estrutura.
///
/// ROTEAMENTO (decidido aqui, nunca pelo cliente):
///   Duvida  -> tutor que criou o curso informado
///   Tecnico -> fila dos administradores (sem destinatario fixo)
/// Deixar o cliente escolher o destinatario permitiria enderecar a qualquer
/// usuario, e obrigaria a tela a saber quem e o tutor de cada curso.
/// </summary>
public class ChamadoService
{
    public const string StatusAberto = "Aberto";
    public const string StatusEmAndamento = "Em andamento";
    public const string StatusResolvido = "Resolvido";

    private readonly IChamadoRepository _chamados;
    private readonly ICursoRepository _cursos;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnidadeDeTrabalho _uow;

    public ChamadoService(
        IChamadoRepository chamados, ICursoRepository cursos,
        IUsuarioRepository usuarios, IUnidadeDeTrabalho uow)
    {
        _chamados = chamados;
        _cursos = cursos;
        _usuarios = usuarios;
        _uow = uow;
    }

    public async Task<Resultado<ChamadoDto>> AbrirAsync(
        int idRemetente, AbrirChamadoRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Assunto))
            return Resultado<ChamadoDto>.Invalido("Informe o assunto.");
        if (string.IsNullOrWhiteSpace(req.Descricao))
            return Resultado<ChamadoDto>.Invalido("Descreva o que aconteceu.");

        var tipo = string.IsNullOrWhiteSpace(req.Tipo) ? TipoChamado.Tecnico : req.Tipo.Trim();
        int? destinatario = req.IdDestinatario;
        int? idCurso = null;

        if (tipo == TipoChamado.Duvida)
        {
            if (req.IdCurso is null)
                return Resultado<ChamadoDto>.Invalido(
                    "Escolha o curso sobre o qual e a duvida.");

            idCurso = req.IdCurso;
            destinatario = await _cursos.ObterUsuarioDoTutorDoCursoAsync(req.IdCurso.Value, ct);

            // Curso sem tutor responsavel nao pode engolir a duvida em
            // silencio: ela cai na fila dos administradores, que resolvem ou
            // encaminham.
            if (destinatario is null) tipo = TipoChamado.Tecnico;
        }
        else if (tipo == TipoChamado.Tecnico)
        {
            // Fila dos administradores: sem destinatario fixo, para qualquer
            // um deles poder assumir. Travar num administrador especifico
            // deixaria o chamado parado se ele estivesse ausente.
            destinatario = null;
        }

        var chamado = new Chamado
        {
            IdRemetente = idRemetente,
            IdDestinatario = destinatario,
            IdCurso = idCurso,
            Tipo = tipo,
            Assunto = req.Assunto.Trim(),
            Descricao = req.Descricao.Trim(),
            DataAbertura = DateTime.UtcNow,
            Status = StatusAberto
        };

        _chamados.Adicionar(chamado);
        await _uow.SalvarAsync(ct);

        var salvo = await _chamados.ObterAsync(chamado.IdChamado, ct);
        return Resultado<ChamadoDto>.Ok(Mapear(salvo!));
    }

    /// <summary>Caixa do usuario: o que ele enviou e o que foi enviado a ele.</summary>
    public async Task<IReadOnlyList<ChamadoDto>> ListarAsync(
        int idUsuario, CancellationToken ct = default)
        => (await _chamados.ListarDoUsuarioAsync(idUsuario, ct)).Select(Mapear).ToList();

    /// <summary>Duvidas enderecadas a este tutor, em qualquer situacao.</summary>
    public async Task<IReadOnlyList<ChamadoDto>> ListarRecebidosAsync(
        int idUsuario, CancellationToken ct = default)
        => (await _chamados.ListarDoDestinatarioAsync(idUsuario, ct))
            .Where(c => c.Tipo == TipoChamado.Duvida)
            .Select(Mapear).ToList();

    /// <summary>Fila de suporte tecnico, para os administradores.</summary>
    public async Task<IReadOnlyList<ChamadoDto>> ListarTecnicosAsync(CancellationToken ct = default)
        => (await _chamados.ListarPorTipoAsync(TipoChamado.Tecnico, ct)).Select(Mapear).ToList();

    /// <summary>
    /// Muda a situacao do chamado. Quem pode: o destinatario, o remetente, ou
    /// qualquer administrador quando o chamado e tecnico (a fila nao tem dono).
    /// </summary>
    public async Task<Resultado<bool>> AtualizarStatusAsync(
        int idChamado, int idUsuario, string novoStatus, CancellationToken ct = default)
    {
        var permitidos = new[] { StatusAberto, StatusEmAndamento, StatusResolvido };
        if (!permitidos.Contains(novoStatus, StringComparer.OrdinalIgnoreCase))
            return Resultado<bool>.Invalido(
                $"Situacao invalida. Use: {string.Join(", ", permitidos)}.");

        var chamado = await _chamados.ObterAsync(idChamado, ct);
        if (chamado is null) return Resultado<bool>.NaoEncontrado("Chamado nao encontrado.");

        var usuario = await _usuarios.ObterPorIdAsync(idUsuario, ct);
        var ehAdmin = usuario?.Adm is not null;

        var podeAgir = chamado.IdDestinatario == idUsuario
                       || chamado.IdRemetente == idUsuario
                       || (ehAdmin && chamado.Tipo == TipoChamado.Tecnico);

        if (!podeAgir) return Resultado<bool>.SemPermissao();

        chamado.Status = novoStatus;
        await _uow.SalvarAsync(ct);
        return Resultado<bool>.Ok(true);
    }

    /// <summary>
    /// Responde a um chamado. A resposta e um chamado novo enderecado a quem
    /// perguntou, e o original passa a Resolvido.
    ///
    /// POR QUE ASSIM: o modelo nao tem tabela de mensagens encadeadas. Reusar
    /// Chamado mantem a resposta na caixa de entrada de quem perguntou, que e
    /// onde ele ja procura.
    /// </summary>
    public async Task<Resultado<bool>> ResponderAsync(
        int idChamado, int idUsuario, string texto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return Resultado<bool>.Invalido("Escreva a resposta.");

        var original = await _chamados.ObterAsync(idChamado, ct);
        if (original is null) return Resultado<bool>.NaoEncontrado("Chamado nao encontrado.");

        var usuario = await _usuarios.ObterPorIdAsync(idUsuario, ct);
        var ehAdmin = usuario?.Adm is not null;

        var podeResponder = original.IdDestinatario == idUsuario
                            || (ehAdmin && original.Tipo == TipoChamado.Tecnico);
        if (!podeResponder) return Resultado<bool>.SemPermissao();

        await _uow.ExecutarEmTransacaoAsync(async () =>
        {
            _chamados.Adicionar(new Chamado
            {
                IdRemetente = idUsuario,
                IdDestinatario = original.IdRemetente,
                IdCurso = original.IdCurso,
                Tipo = TipoChamado.Resposta,
                Assunto = "Re: " + (original.Assunto ?? "chamado"),
                Descricao = texto.Trim(),
                DataAbertura = DateTime.UtcNow,
                Status = StatusResolvido
            });

            original.Status = StatusResolvido;
            await _uow.SalvarAsync(ct);
        }, ct);

        return Resultado<bool>.Ok(true);
    }

    public async Task<bool> FecharAsync(int idChamado, int idUsuario, CancellationToken ct = default)
    {
        var r = await AtualizarStatusAsync(idChamado, idUsuario, StatusResolvido, ct);
        return r.Status == StatusOperacao.Ok;
    }

    private static ChamadoDto Mapear(Chamado c) => new(
        c.IdChamado, c.Tipo, c.Assunto, c.Descricao,
        c.IdRemetente, c.Remetente.Nome,
        c.IdDestinatario, c.Destinatario?.Nome,
        c.DataAbertura, c.Status,
        c.Curso?.Nome);
}
