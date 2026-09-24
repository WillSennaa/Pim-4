using TechQuest.Application.Dtos;
using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;
using TechQuest.Domain.Regras;

namespace TechQuest.Application.Servicos;

public class TutorService
{
    private readonly ICursoRepository _cursos;
    private readonly IProvaRepository _provas;
    private readonly ITutorRepository _tutores;
    private readonly IConquistaRepository _conquistas;
    private readonly IUnidadeDeTrabalho _uow;

    public TutorService(
        ICursoRepository cursos, IProvaRepository provas, ITutorRepository tutores,
        IConquistaRepository conquistas, IUnidadeDeTrabalho uow)
    {
        _cursos = cursos; _provas = provas; _tutores = tutores;
        _conquistas = conquistas; _uow = uow;
    }

    // -----------------------------------------------------------------------
    // CURSOS
    // -----------------------------------------------------------------------
    public async Task<IReadOnlyList<CursoTutorDto>> ListarCursosAsync(
        int idTutor, CancellationToken ct = default)
    {
        var cursos = await _cursos.ListarPorTutorAsync(idTutor, ct);
        var lista = new List<CursoTutorDto>();

        foreach (var c in cursos)
            lista.Add(await MapearAsync(c, ct));

        return lista;
    }

    public async Task<Resultado<CursoTutorDto>> CriarCursoAsync(
        int idTutor, CriarCursoRequest req, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(req.Nome))
            return Resultado<CursoTutorDto>.Invalido("O nome do curso e obrigatorio.");

        var curso = new Curso
        {
            Nome = req.Nome.Trim(),
            Descricao = req.Descricao,
            Categoria = req.Categoria,
            Nivel = req.Nivel,
            DuracaoHoras = req.DuracaoHoras,
            IdTutorCriou = idTutor,
            // Todo curso nasce como rascunho: publicar depende do administrador.
            Status = FluxoPublicacaoCurso.Rascunho
        };

        _cursos.Adicionar(curso);
        await _uow.SalvarAsync(ct);

        return Resultado<CursoTutorDto>.Ok(await MapearAsync(curso, ct));
    }

    public async Task<Resultado<CursoTutorDto>> AtualizarCursoAsync(
        int idTutor, int idCurso, CriarCursoRequest req, CancellationToken ct = default)
    {
        var (curso, erro) = await ObterProprioAsync(idTutor, idCurso, exigirEditavel: true, ct);
        if (erro is not null) return erro;

        curso!.Nome = req.Nome.Trim();
        curso.Descricao = req.Descricao;
        curso.Categoria = req.Categoria;
        curso.Nivel = req.Nivel;
        curso.DuracaoHoras = req.DuracaoHoras;

        await _uow.SalvarAsync(ct);
        return Resultado<CursoTutorDto>.Ok(await MapearAsync(curso, ct));
    }

    /// <summary>Envia o curso para avaliacao do administrador.</summary>
    public async Task<Resultado<CursoTutorDto>> SubmeterAsync(
        int idTutor, int idCurso, CancellationToken ct = default)
    {
        var (curso, erro) = await ObterProprioAsync(idTutor, idCurso, exigirEditavel: false, ct);
        if (erro is not null) return erro;

        if (!FluxoPublicacaoCurso.PodeTransitar(curso!.Status, FluxoPublicacaoCurso.Pendente))
            return Resultado<CursoTutorDto>.Invalido(
                $"Um curso com status '{curso.Status}' nao pode ser submetido.");

        // Submeter curso vazio geraria uma solicitacao que o admin so pode
        // rejeitar: a validacao aqui poupa o ciclo inteiro.
        var comMateriais = await _cursos.ObterComMateriaisAsync(idCurso, ct);
        if (comMateriais is null || comMateriais.Materiais.Count == 0)
            return Resultado<CursoTutorDto>.Invalido("Adicione ao menos uma aula antes de submeter.");

        curso.Status = FluxoPublicacaoCurso.Pendente;
        await _uow.SalvarAsync(ct);

        return Resultado<CursoTutorDto>.Ok(await MapearAsync(curso, ct));
    }

    // -----------------------------------------------------------------------
    // AULAS
    // -----------------------------------------------------------------------
    public async Task<Resultado<MaterialTutorDto>> AdicionarAulaAsync(
        int idTutor, int idCurso, MaterialRequest req, CancellationToken ct = default)
    {
        var (curso, erro) = await ObterProprioAsync<MaterialTutorDto>(idTutor, idCurso, true, ct);
        if (erro is not null) return erro;

        if (string.IsNullOrWhiteSpace(req.Titulo))
            return Resultado<MaterialTutorDto>.Invalido("O titulo da aula e obrigatorio.");

        var material = new Material { IdCurso = curso!.IdCurso, Titulo = req.Titulo.Trim(), Tipo = req.Tipo };
        _cursos.AdicionarMaterial(material);
        await _uow.SalvarAsync(ct);

        return Resultado<MaterialTutorDto>.Ok(
            new MaterialTutorDto(material.IdMaterial, material.Titulo, material.Tipo));
    }

    public async Task<Resultado<bool>> RemoverAulaAsync(
        int idTutor, int idMaterial, CancellationToken ct = default)
    {
        var material = await _cursos.ObterMaterialParaEdicaoAsync(idMaterial, ct);
        if (material is null) return Resultado<bool>.NaoEncontrado("Aula nao encontrada.");

        var (curso, erro) = await ObterProprioAsync<bool>(idTutor, material.IdCurso, true, ct);
        if (erro is not null) return erro;

        // Progresso registrado cria FK: apagar a aula apagaria historico de
        // estudo de alunos. O banco impediria de qualquer forma; aqui a
        // mensagem explica o motivo em vez de estourar erro de constraint.
        if (await _cursos.MaterialTemProgressoAsync(idMaterial, ct))
            return Resultado<bool>.Invalido(
                "Esta aula ja foi assistida por alunos e nao pode ser removida.");

        _cursos.RemoverMaterial(material);
        await _uow.SalvarAsync(ct);
        return Resultado<bool>.Ok(true);
    }

    // -----------------------------------------------------------------------
    // PROVA E QUESTOES
    // -----------------------------------------------------------------------
    public async Task<Resultado<ProvaTutorDto>> CriarProvaAsync(
        int idTutor, int idCurso, CriarProvaRequest req, CancellationToken ct = default)
    {
        var (curso, erro) = await ObterProprioAsync<ProvaTutorDto>(idTutor, idCurso, true, ct);
        if (erro is not null) return erro;

        // O modelo do PIM III guarda a FK da prova no curso: 1 curso, 1 prova.
        if (curso!.IdProva is not null)
            return Resultado<ProvaTutorDto>.Invalido("Este curso ja possui uma prova.");

        if (req.NotaMinima < 0 || req.NotaMinima > 10)
            return Resultado<ProvaTutorDto>.Invalido("A nota minima deve estar entre 0 e 10.");

        var prova = new Prova
        {
            Titulo = req.Titulo,
            NotaMinima = req.NotaMinima,
            TempoMinutos = req.TempoMinutos <= 0 ? 30 : req.TempoMinutos
        };

        await _uow.ExecutarEmTransacaoAsync(async () =>
        {
            _provas.Adicionar(prova);
            await _uow.SalvarAsync(ct);

            // Prova e vinculo ao curso precisam valer juntos: uma prova sem
            // curso ficaria orfa e invisivel.
            curso.IdProva = prova.IdProva;
            await _uow.SalvarAsync(ct);
        }, ct);

        return Resultado<ProvaTutorDto>.Ok(
            new ProvaTutorDto(prova.IdProva, prova.Titulo, prova.NotaMinima, prova.TempoMinutos, 0));
    }

    public async Task<Resultado<QuestaoTutorDto>> AdicionarQuestaoAsync(
        int idTutor, int idProva, CriarQuestaoRequest req, CancellationToken ct = default)
    {
        var prova = await _provas.ObterParaEdicaoAsync(idProva, ct);
        if (prova is null) return Resultado<QuestaoTutorDto>.NaoEncontrado("Prova nao encontrada.");

        var cursos = await _cursos.ListarPorTutorAsync(idTutor, ct);
        var curso = cursos.FirstOrDefault(c => c.IdProva == idProva);
        if (curso is null) return Resultado<QuestaoTutorDto>.SemPermissao();
        if (!FluxoPublicacaoCurso.PermiteEdicao(curso.Status))
            return Resultado<QuestaoTutorDto>.Invalido(
                "Nao e possivel alterar a prova de um curso publicado ou em avaliacao.");

        var validacao = RegrasQuestao.Validar(
            req.Enunciado,
            req.Alternativas.Select(a => new RegrasQuestao.AlternativaParaValidar(a.Letra, a.EhCorreta)).ToList());
        if (validacao is not null) return Resultado<QuestaoTutorDto>.Invalido(validacao);

        var questao = new Questao
        {
            IdProva = idProva,
            Enunciado = req.Enunciado.Trim(),
            CodigoExemplo = req.CodigoExemplo,
            Ordem = req.Ordem
        };

        foreach (var a in req.Alternativas)
            questao.Alternativas.Add(new Alternativa
            {
                Letra = a.Letra.Trim().ToUpperInvariant(),
                Texto = a.Texto,
                EhCorreta = a.EhCorreta
            });

        _provas.AdicionarQuestao(questao);
        await _uow.SalvarAsync(ct);

        return Resultado<QuestaoTutorDto>.Ok(new QuestaoTutorDto(
            questao.IdQuestao, questao.Ordem, questao.Enunciado, questao.CodigoExemplo,
            questao.AlternativaCorreta()?.Letra, questao.Alternativas.Count));
    }

    // -----------------------------------------------------------------------
    // ALUNOS
    // -----------------------------------------------------------------------
    public Task<IReadOnlyList<AlunoDoTutorDto>> ListarAlunosAsync(
        int idTutor, int? idCurso, CancellationToken ct = default)
        => _tutores.ListarAlunosAsync(idTutor, idCurso, ct);

    /// <summary>
    /// Concessao manual de medalha.
    ///
    /// Existe porque quatro medalhas do PIM III ("Mestre em Arrays",
    /// "Maratonista", "POO Master", "Arquiteto SOLID") dependem de criterios
    /// que o modelo de dados nao registra. Em vez de inventar coluna ou fingir
    /// que a regra e automatica, o tutor concede.
    /// </summary>
    public async Task<Resultado<string>> ConcederMedalhaAsync(
        int idEstudante, int idMedalha, CancellationToken ct = default)
    {
        if (!await _tutores.EstudanteExisteAsync(idEstudante, ct))
            return Resultado<string>.NaoEncontrado("Estudante nao encontrado.");

        var medalha = await _conquistas.ObterMedalhaAsync(idMedalha, ct);
        if (medalha is null) return Resultado<string>.NaoEncontrado("Medalha nao encontrada.");

        var jaTem = (await _conquistas.ListarDoEstudanteAsync(idEstudante, ct))
            .Any(c => c.IdMedalha == idMedalha);
        if (jaTem) return Resultado<string>.Invalido("O estudante ja possui esta medalha.");

        _conquistas.Adicionar(new Conquista
        {
            IdEstudante = idEstudante,
            IdMedalha = idMedalha,
            DataConquista = DateTime.UtcNow
        });
        await _uow.SalvarAsync(ct);

        return Resultado<string>.Ok(medalha.Nome);
    }

    // -----------------------------------------------------------------------
    // APOIO
    // -----------------------------------------------------------------------
    private async Task<(Curso?, Resultado<CursoTutorDto>?)> ObterProprioAsync(
        int idTutor, int idCurso, bool exigirEditavel, CancellationToken ct)
    {
        var (curso, erro) = await ObterProprioAsync<CursoTutorDto>(idTutor, idCurso, exigirEditavel, ct);
        return (curso, erro);
    }

    /// <summary>
    /// Verificacao de propriedade: o tutor so alcanca o proprio curso. Fica no
    /// servico, nao no controller, porque e regra de negocio — e porque o
    /// mobile e o desktop usam o mesmo servico.
    /// </summary>
    private async Task<(Curso?, Resultado<T>?)> ObterProprioAsync<T>(
        int idTutor, int idCurso, bool exigirEditavel, CancellationToken ct)
    {
        var curso = await _cursos.ObterParaEdicaoAsync(idCurso, ct);
        if (curso is null) return (null, Resultado<T>.NaoEncontrado("Curso nao encontrado."));
        if (curso.IdTutorCriou != idTutor) return (null, Resultado<T>.SemPermissao());

        if (exigirEditavel && !FluxoPublicacaoCurso.PermiteEdicao(curso.Status))
            return (null, Resultado<T>.Invalido(
                $"Um curso com status '{curso.Status}' nao pode ser alterado."));

        return (curso, null);
    }

    private async Task<CursoTutorDto> MapearAsync(Curso c, CancellationToken ct)
    {
        var comMateriais = await _cursos.ObterComMateriaisAsync(c.IdCurso, ct);
        var questoes = c.IdProva is null ? 0 : await _cursos.ContarQuestoesAsync(c.IdProva.Value, ct);

        return new CursoTutorDto(
            c.IdCurso, c.Nome, c.Descricao, c.Categoria, c.Nivel, c.DuracaoHoras, c.Status,
            comMateriais?.Materiais.Count ?? 0,
            await _cursos.ContarMatriculadosAsync(c.IdCurso, ct),
            c.IdProva, questoes,
            FluxoPublicacaoCurso.PermiteEdicao(c.Status));
    }
}
