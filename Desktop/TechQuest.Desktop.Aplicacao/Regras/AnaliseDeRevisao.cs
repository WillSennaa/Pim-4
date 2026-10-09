using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Aplicacao.Regras;

/// <summary>
/// Aponta o que deixaria um curso incompleto se fosse publicado como esta:
/// material sem texto, falta de prova, questao sem alternativa correta.
/// Sao os mesmos "Pontos de atencao" da revisao no web
/// (carregarRevisaoCurso), para o admin ter o mesmo criterio nas duas versoes.
///
/// POR QUE AVISAR E NAO BLOQUEAR: a API aceita aprovar mesmo assim, e quem
/// decide e o avaliador (um curso introdutorio sem prova pode ser
/// intencional). O desktop mostra a lista e pede confirmacao reforcada; nao
/// cria uma regra que o servidor nao tem.
///
/// POR QUE NA CAMADA DE APLICACAO: e criterio de negocio sobre o curso, nao
/// detalhe de tela. Se amanha o mobile ganhasse revisao, a regra ja estaria
/// fora do Windows Forms.
/// </summary>
public static class AnaliseDeRevisao
{
    public static IReadOnlyList<string> PontosDeAtencao(CursoRevisao curso)
    {
        ArgumentNullException.ThrowIfNull(curso);
        var pontos = new List<string>();

        if (curso.Materiais.Count == 0)
        {
            pontos.Add("O curso não tem nenhum material.");
        }
        else
        {
            var vazios = curso.Materiais.Count(m => string.IsNullOrWhiteSpace(m.Conteudo));
            if (vazios == 1) pontos.Add("1 material está sem conteúdo escrito.");
            else if (vazios > 1) pontos.Add($"{vazios} materiais estão sem conteúdo escrito.");
        }

        if (curso.Prova is null)
        {
            pontos.Add("O curso não tem prova: o aluno concluiria apenas lendo os materiais.");
        }
        else if (curso.Prova.Questoes.Count == 0)
        {
            pontos.Add("A prova não tem questões.");
        }
        else
        {
            var semGabarito = curso.Prova.Questoes.Count(q => !q.Alternativas.Any(a => a.EhCorreta));
            if (semGabarito == 1) pontos.Add("1 questão está sem alternativa correta marcada.");
            else if (semGabarito > 1) pontos.Add($"{semGabarito} questões estão sem alternativa correta marcada.");
        }

        return pontos;
    }
}
