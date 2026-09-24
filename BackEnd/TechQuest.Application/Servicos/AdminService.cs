using TechQuest.Application.Dtos;
using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;
using TechQuest.Domain.Enums;
using TechQuest.Domain.Regras;

namespace TechQuest.Application.Servicos;

public class AdminService
{
    private readonly ICursoRepository _cursos;
    private readonly IUsuarioRepository _usuarios;
    private readonly IAdminRepository _admin;
    private readonly IChamadoRepository _chamados;
    private readonly IHashSenhaService _hash;
    private readonly IUnidadeDeTrabalho _uow;

    public AdminService(
        ICursoRepository cursos, IUsuarioRepository usuarios, IAdminRepository admin,
        IChamadoRepository chamados, IHashSenhaService hash, IUnidadeDeTrabalho uow)
    {
        _cursos = cursos; _usuarios = usuarios; _admin = admin;
        _chamados = chamados; _hash = hash; _uow = uow;
    }

    public Task<ResumoAdminDto> ResumoAsync(CancellationToken ct = default)
        => _admin.ResumoAsync(ct);

    // -----------------------------------------------------------------------
    // AVALIACAO DE CURSOS
    // -----------------------------------------------------------------------
    public async Task<IReadOnlyList<SolicitacaoCursoDto>> ListarSolicitacoesAsync(
        string? status, CancellationToken ct = default)
    {
        var cursos = await _cursos.ListarPorStatusAsync(
            string.IsNullOrWhiteSpace(status) ? FluxoPublicacaoCurso.Pendente : status, ct);

        var lista = new List<SolicitacaoCursoDto>();
        foreach (var c in cursos)
        {
            var completo = await _cursos.ObterComMateriaisAsync(c.IdCurso, ct);
            var questoes = c.IdProva is null ? 0 : await _cursos.ContarQuestoesAsync(c.IdProva.Value, ct);

            lista.Add(new SolicitacaoCursoDto(
                c.IdCurso, c.Nome, c.Descricao, c.Categoria, c.Nivel,
                c.TutorCriou?.Usuario.Nome, c.Status,
                completo?.Materiais.Count ?? 0, c.IdProva is not null, questoes));
        }
        return lista;
    }

    public Task<Resultado<string>> AprovarAsync(
        int idAdm, int idCurso, CancellationToken ct = default)
        => AvaliarAsync(idAdm, idCurso, FluxoPublicacaoCurso.Publicado, null, ct);

    public Task<Resultado<string>> RejeitarAsync(
        int idAdm, int idCurso, string? motivo, CancellationToken ct = default)
        => AvaliarAsync(idAdm, idCurso, FluxoPublicacaoCurso.Rejeitado, motivo, ct);

    private async Task<Resultado<string>> AvaliarAsync(
        int idAdm, int idCurso, string novoStatus, string? motivo, CancellationToken ct)
    {
        var curso = await _cursos.ObterParaEdicaoAsync(idCurso, ct);
        if (curso is null) return Resultado<string>.NaoEncontrado("Curso nao encontrado.");

        if (!FluxoPublicacaoCurso.PodeTransitar(curso.Status, novoStatus))
            return Resultado<string>.Invalido(
                $"Um curso com status '{curso.Status}' nao pode ir para '{novoStatus}'.");

        await _uow.ExecutarEmTransacaoAsync(async () =>
        {
            curso.Status = novoStatus;
            curso.IdAdmAvaliou = idAdm;

            // A trigger do banco so audita mudanca de status de USUARIO.
            // A avaliacao de curso e registrada aqui, com o ID real de quem
            // avaliou — nao com o valor fixo que a trigger usa.
            _admin.RegistrarLog(new LogAuditoria
            {
                IdAdm = idAdm,
                AcaoRealizada = $"Curso {curso.IdCurso} ({curso.Nome}) alterado para {novoStatus}",
                DataAcao = DateTime.UtcNow
            });

            // A rejeicao vira uma mensagem para o tutor: a tabela Curso nao tem
            // campo de parecer, e Chamado ja e o canal de comunicacao do modelo.
            if (novoStatus == FluxoPublicacaoCurso.Rejeitado && curso.IdTutorCriou is not null)
            {
                var tutor = await ObterUsuarioDoTutorAsync(curso.IdTutorCriou.Value, ct);
                if (tutor is not null)
                {
                    _chamados.Adicionar(new Chamado
                    {
                        IdRemetente = await ObterUsuarioDoAdmAsync(idAdm, ct) ?? tutor.Value,
                        IdDestinatario = tutor,
                        Tipo = "Avaliacao",
                        Assunto = $"Curso rejeitado: {curso.Nome}",
                        Descricao = string.IsNullOrWhiteSpace(motivo)
                            ? "O curso foi rejeitado. Revise o conteudo e submeta novamente."
                            : motivo,
                        DataAbertura = DateTime.UtcNow,
                        Status = "Aberto"
                    });
                }
            }

            await _uow.SalvarAsync(ct);
        }, ct);

        return Resultado<string>.Ok(novoStatus);
    }

    // -----------------------------------------------------------------------
    // USUARIOS
    // -----------------------------------------------------------------------
    public async Task<IReadOnlyList<UsuarioAdminDto>> ListarUsuariosAsync(CancellationToken ct = default)
        => (await _usuarios.ListarAsync(ct)).Select(u => new UsuarioAdminDto(
            u.IdUsuario, u.Nome, u.Email,
            u.Adm is not null ? "Admin" : u.Tutor is not null ? "Tutor" : "Estudante",
            u.Ativo, u.DataCadastro)).ToList();

    public async Task<Resultado<UsuarioAdminDto>> CriarUsuarioAsync(
        CriarUsuarioRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Nome) || string.IsNullOrWhiteSpace(req.Email))
            return Resultado<UsuarioAdminDto>.Invalido("Nome e e-mail sao obrigatorios.");

        if (req.Senha.Length < 6)
            return Resultado<UsuarioAdminDto>.Invalido("A senha deve ter ao menos 6 caracteres.");

        if (!Enum.TryParse<PapelUsuario>(req.Papel, true, out var papel))
            return Resultado<UsuarioAdminDto>.Invalido("Papel invalido. Use Estudante, Tutor ou Admin.");

        if (await _usuarios.EmailExisteAsync(req.Email, ct))
            return Resultado<UsuarioAdminDto>.Invalido("Ja existe um usuario com este e-mail.");

        var usuario = new Usuario
        {
            Nome = req.Nome.Trim(),
            Email = req.Email.Trim().ToLowerInvariant(),
            // A senha em texto puro morre aqui: so o hash vai para o banco.
            SenhaHash = _hash.GerarHash(req.Senha),
            DataCadastro = DateTime.UtcNow,
            Ativo = true
        };

        await _uow.ExecutarEmTransacaoAsync(async () =>
        {
            _usuarios.Adicionar(usuario);
            await _uow.SalvarAsync(ct);

            // Usuario e especializacao nascem juntos: um usuario sem
            // especializacao nao teria papel nenhum e nao conseguiria usar nada.
            switch (papel)
            {
                case PapelUsuario.Admin:
                    _usuarios.Adicionar(new Adm { IdUsuario = usuario.IdUsuario }); break;
                case PapelUsuario.Tutor:
                    _usuarios.Adicionar(new Tutor { IdUsuario = usuario.IdUsuario }); break;
                default:
                    _usuarios.Adicionar(new Estudante { IdUsuario = usuario.IdUsuario }); break;
            }
            await _uow.SalvarAsync(ct);
        }, ct);

        return Resultado<UsuarioAdminDto>.Ok(new UsuarioAdminDto(
            usuario.IdUsuario, usuario.Nome, usuario.Email, papel.ToString(),
            usuario.Ativo, usuario.DataCadastro));
    }

    /// <summary>
    /// Ativa ou desativa um usuario.
    ///
    /// E ESTE endpoint que dispara a trigger TRG_Auditoria_StatusUsuario: o
    /// UPDATE em Status_Usuario gera sozinho a linha em Log_Auditoria. Por
    /// isso a API nao registra log aqui — duplicaria o registro.
    /// </summary>
    public async Task<Resultado<bool>> AlterarStatusAsync(
        int idAdm, int idUsuario, bool ativo, CancellationToken ct = default)
    {
        var usuario = await _usuarios.ObterParaEdicaoAsync(idUsuario, ct);
        if (usuario is null) return Resultado<bool>.NaoEncontrado("Usuario nao encontrado.");

        // Um administrador desativar a si mesmo deixaria a plataforma sem
        // ninguem para reativa-lo.
        var idUsuarioDoAdm = await ObterUsuarioDoAdmAsync(idAdm, ct);
        if (!ativo && idUsuarioDoAdm == idUsuario)
            return Resultado<bool>.Invalido("Um administrador nao pode desativar a propria conta.");

        if (usuario.Ativo == ativo) return Resultado<bool>.Ok(true);

        usuario.Ativo = ativo;
        await _uow.SalvarAsync(ct);
        return Resultado<bool>.Ok(true);
    }

    public async Task<IReadOnlyList<LogAuditoriaDto>> ListarLogsAsync(
        int limite = 100, CancellationToken ct = default)
        => (await _admin.ListarLogsAsync(limite, ct)).Select(l => new LogAuditoriaDto(
            l.IdLog, l.IdAdm, l.Adm?.Usuario?.Nome, l.AcaoRealizada, l.DataAcao)).ToList();

    // -----------------------------------------------------------------------
    private async Task<int?> ObterUsuarioDoTutorAsync(int idTutor, CancellationToken ct)
    {
        var usuarios = await _usuarios.ListarAsync(ct);
        return usuarios.FirstOrDefault(u => u.Tutor?.IdTutor == idTutor)?.IdUsuario;
    }

    private async Task<int?> ObterUsuarioDoAdmAsync(int idAdm, CancellationToken ct)
    {
        var usuarios = await _usuarios.ListarAsync(ct);
        return usuarios.FirstOrDefault(u => u.Adm?.IdAdm == idAdm)?.IdUsuario;
    }
}
