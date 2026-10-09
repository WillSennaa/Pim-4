using TechQuest.Desktop.Aplicacao.Estado;
using TechQuest.Desktop.Aplicacao.Regras;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Modelos;
using TechQuest.Desktop.Tests.Dubles;
using Xunit;

namespace TechQuest.Desktop.Tests.Aplicacao;

public class AvaliacaoCursosServiceTests
{
    private static (AvaliacaoCursosService servico, CursosAdminGatewayFalso api, ContadorDePendencias contador) Montar()
    {
        var api = new CursosAdminGatewayFalso();
        var contador = new ContadorDePendencias();
        return (new AvaliacaoCursosService(api, contador), api, contador);
    }

    [Fact]
    public async Task Listar_a_fila_informa_o_contador_do_selo()
    {
        var (servico, api, contador) = Montar();
        api.Fila = new() { CursosDeTeste.Solicitacao(1), CursosDeTeste.Solicitacao(2) };

        await servico.ListarPendentesAsync();

        Assert.Equal(2, contador.CursosPendentes);
    }

    [Theory]
    [InlineData((string?)null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("nao")]
    [InlineData("  curto   ")] // 5 caracteres depois do Trim
    public async Task Motivo_curto_ou_vazio_e_recusado_sem_chamar_a_API(string? motivo)
    {
        var (servico, api, _) = Montar();

        var r = await servico.RejeitarAsync(7, motivo);

        Assert.False(r.Sucesso);
        Assert.Empty(api.Rejeitados);
    }

    [Fact]
    public async Task Motivo_acima_do_limite_e_recusado()
    {
        var (servico, api, _) = Montar();

        var r = await servico.RejeitarAsync(7, new string('x', AvaliacaoCursosService.TamanhoMaximoMotivo + 1));

        Assert.False(r.Sucesso);
        Assert.Empty(api.Rejeitados);
    }

    [Fact]
    public async Task Motivo_valido_e_enviado_sem_espacos_nas_pontas()
    {
        var (servico, api, _) = Montar();

        var r = await servico.RejeitarAsync(7, "   Falta a prova final.  ");

        Assert.True(r.Sucesso);
        Assert.Equal((7, "Falta a prova final."), api.Rejeitados.Single());
    }
}

public class ContadorDePendenciasTests
{
    [Fact]
    public void Comeca_desconhecido_e_avisa_so_quando_o_numero_muda()
    {
        var contador = new ContadorDePendencias();
        var avisos = 0;
        contador.Mudou += (_, _) => avisos++;

        Assert.Null(contador.CursosPendentes);

        contador.Informar(2);
        contador.Informar(2); // mesmo numero: tela nao precisa redesenhar
        contador.Informar(0);

        Assert.Equal(2, avisos);
        Assert.Equal(0, contador.CursosPendentes);
    }

    [Fact]
    public void Numero_negativo_e_erro_de_programa()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ContadorDePendencias().Informar(-1));
    }
}

public class AnaliseDeRevisaoTests
{
    [Fact]
    public void Curso_completo_nao_tem_pontos_de_atencao()
    {
        Assert.Empty(AnaliseDeRevisao.PontosDeAtencao(CursosDeTeste.Completo()));
    }

    [Fact]
    public void Sem_material_e_sem_prova_aponta_os_dois()
    {
        var curso = CursosDeTeste.Completo() with { Materiais = Array.Empty<MaterialRevisao>(), Prova = null };

        var pontos = AnaliseDeRevisao.PontosDeAtencao(curso);

        Assert.Equal(2, pontos.Count);
        Assert.Contains("nenhum material", pontos[0]);
        Assert.Contains("não tem prova", pontos[1]);
    }

    [Fact]
    public void Conta_materiais_vazios_e_questoes_sem_gabarito()
    {
        var c = CursosDeTeste.Completo();
        var curso = c with
        {
            Materiais = new[]
            {
                new MaterialRevisao(1, "A", "texto", ""),
                new MaterialRevisao(2, "B", "texto", "   "),
                new MaterialRevisao(3, "C", "texto", "ok")
            },
            Prova = c.Prova! with
            {
                Questoes = new[]
                {
                    new QuestaoRevisao(1, 1, "?", null, new[] { new AlternativaRevisao(1, "A", "x", false) })
                }
            }
        };

        var pontos = AnaliseDeRevisao.PontosDeAtencao(curso);

        Assert.Equal(new[]
        {
            "2 materiais estão sem conteúdo escrito.",
            "1 questão está sem alternativa correta marcada."
        }, pontos);
    }

    [Fact]
    public void Prova_sem_questoes_e_apontada()
    {
        var c = CursosDeTeste.Completo();
        var curso = c with { Prova = c.Prova! with { Questoes = Array.Empty<QuestaoRevisao>() } };

        Assert.Equal(new[] { "A prova não tem questões." }, AnaliseDeRevisao.PontosDeAtencao(curso));
    }
}
