using TechQuest.Application.Dtos;
using TechQuest.Application.Interfaces;
using TechQuest.Domain.Entidades;
using TechQuest.Domain.Regras;

namespace TechQuest.Application.Servicos;

public class ConquistaService
{
    private readonly IConquistaRepository _conquistas;
    private readonly IGamificacaoRepository _gamificacao;
    private readonly IHistoricoRepository _historicos;
    private readonly ICursoRepository _cursos;

    public ConquistaService(
        IConquistaRepository conquistas,
        IGamificacaoRepository gamificacao,
        IHistoricoRepository historicos,
        ICursoRepository cursos)
    {
        _conquistas = conquistas;
        _gamificacao = gamificacao;
        _historicos = historicos;
        _cursos = cursos;
    }

    /// <summary>
    /// Avalia todas as regras e REGISTRA as medalhas novas (sem gravar: quem
    /// grava e a unidade de trabalho do servico chamador). Devolve os nomes
    /// concedidos agora, para o cliente exibir a animacao de conquista.
    /// </summary>
    public async Task<IReadOnlyList<string>> AvaliarAsync(int idEstudante, CancellationToken ct = default)
    {
        var contagens = await _gamificacao.ObterContagensAsync(idEstudante, ct);
        var concluidos = await _historicos.NomesDeCursosConcluidosAsync(idEstudante, ct);
        var totalCursos = await _cursos.ContarPublicadosAsync(ct);

        var merecidas = RegrasMedalhas.Merecidas(new RegrasMedalhas.Situacao(
            contagens.MateriaisConcluidos,
            contagens.ProvasAprovadas,
            contagens.NotasMaximas,
            concluidos,
            totalCursos));

        if (merecidas.Count == 0) return Array.Empty<string>();

        var jaTem = (await _conquistas.ListarDoEstudanteAsync(idEstudante, ct))
            .Select(c => c.Medalha.Nome)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var faltando = merecidas.Where(n => !jaTem.Contains(n)).ToList();
        if (faltando.Count == 0) return Array.Empty<string>();

        var medalhas = await _conquistas.ObterMedalhasPorNomeAsync(faltando, ct);
        foreach (var m in medalhas)
        {
            _conquistas.Adicionar(new Conquista
            {
                IdEstudante = idEstudante,
                IdMedalha = m.IdMedalha,
                DataConquista = DateTime.UtcNow
            });
        }

        return medalhas.Select(m => m.Nome).ToList();
    }

    public async Task<IReadOnlyList<MedalhaDto>> ListarAsync(int idEstudante, CancellationToken ct = default)
    {
        var todas = await _conquistas.ListarMedalhasAsync(ct);
        var minhas = (await _conquistas.ListarDoEstudanteAsync(idEstudante, ct))
            .ToDictionary(c => c.IdMedalha, c => c.DataConquista);

        // Devolve TODAS as medalhas, marcando quais ja foram conquistadas:
        // a tela do PIM III mostra as bloqueadas em cinza como incentivo.
        return todas.Select(m => new MedalhaDto(
            m.IdMedalha, m.Nome, m.Descricao, m.Raridade,
            minhas.ContainsKey(m.IdMedalha),
            minhas.TryGetValue(m.IdMedalha, out var data) ? data : null)).ToList();
    }
}
