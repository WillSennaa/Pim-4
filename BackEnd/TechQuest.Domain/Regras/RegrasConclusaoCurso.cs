namespace TechQuest.Domain.Regras;

/// <summary>
/// Quando um curso esta concluido.
///
/// POR QUE A REGRA SAIU DO SERVICO E VEIO PARA O DOMINIO
/// Ela estava embutida dentro de um metodo que tambem carregava dados,
/// gravava historico e emitia certificado. Misturada com isso, so podia ser
/// testada subindo repositorios -- e por isso nao era testada. Separada, e
/// uma funcao de quatro entradas booleanas e inteiras, sem dependencia
/// nenhuma: entra na suite do dominio, que roda em milissegundos.
///
/// O QUE ESTA SEPARACAO NAO RESOLVE, e vale ser explicito: ter a regra
/// correta nao garante que alguem a CHAME na hora certa. O defeito real do
/// certificado nao estava no criterio -- estava em so haver um gatilho
/// (concluir material) quando existem dois (concluir material, passar na
/// prova). Isso e orquestracao, e so um teste do servico pega. Ver
/// ConclusaoCursoServiceTests.
/// </summary>
public static class RegrasConclusaoCurso
{
    /// <summary>
    /// Criterio de conclusao: TODOS os materiais concluidos E, se o curso
    /// tiver prova, a prova aprovada.
    /// </summary>
    /// <param name="totalMateriais">Materiais publicados no curso.</param>
    /// <param name="materiaisConcluidos">Quantos este aluno concluiu.</param>
    /// <param name="cursoTemProva">Se existe prova vinculada ao curso.</param>
    /// <param name="provaAprovada">Se este aluno ja foi aprovado nela.</param>
    public static bool CursoConcluido(
        int totalMateriais, int materiaisConcluidos, bool cursoTemProva, bool provaAprovada)
    {
        // Curso sem material nenhum nao se conclui. Sem esta guarda,
        // 0 >= 0 seria verdadeiro e um curso vazio emitiria certificado --
        // o que e pior que nao emitir, porque certifica o que nao houve.
        if (totalMateriais <= 0) return false;

        if (materiaisConcluidos < totalMateriais) return false;

        // Curso sem prova se conclui so com os materiais. E uma decisao de
        // modelagem: ID_Prova e opcional em Curso.
        return !cursoTemProva || provaAprovada;
    }

    /// <summary>
    /// Percentual de materiais concluidos. Curso sem material e 0%, nao 100%:
    /// dividir por zero nao e a unica forma de errar aqui -- dizer que um
    /// curso vazio esta completo tambem e.
    /// </summary>
    public static int PercentualConcluido(int totalMateriais, int materiaisConcluidos)
    {
        if (totalMateriais <= 0) return 0;

        var feitos = Math.Clamp(materiaisConcluidos, 0, totalMateriais);
        return (int)Math.Round(feitos * 100.0 / totalMateriais);
    }
}
