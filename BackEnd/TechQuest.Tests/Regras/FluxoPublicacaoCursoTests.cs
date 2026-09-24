using TechQuest.Domain.Regras;
using Xunit;

namespace TechQuest.Tests.Regras;

public class FluxoPublicacaoCursoTests
{
    [Theory]
    [InlineData(FluxoPublicacaoCurso.Rascunho, FluxoPublicacaoCurso.Pendente)]
    [InlineData(FluxoPublicacaoCurso.Pendente, FluxoPublicacaoCurso.Publicado)]
    [InlineData(FluxoPublicacaoCurso.Pendente, FluxoPublicacaoCurso.Rejeitado)]
    [InlineData(FluxoPublicacaoCurso.Rejeitado, FluxoPublicacaoCurso.Pendente)]
    public void Transicoes_do_fluxo_sao_permitidas(string de, string para)
        => Assert.True(FluxoPublicacaoCurso.PodeTransitar(de, para));

    [Fact]
    public void Rascunho_nao_pula_direto_para_publicado()
        => Assert.False(FluxoPublicacaoCurso.PodeTransitar(
            FluxoPublicacaoCurso.Rascunho, FluxoPublicacaoCurso.Publicado));

    [Theory]
    [InlineData(FluxoPublicacaoCurso.Pendente)]
    [InlineData(FluxoPublicacaoCurso.Rejeitado)]
    [InlineData(FluxoPublicacaoCurso.Rascunho)]
    public void Publicado_e_estado_final(string destino)
        => Assert.False(FluxoPublicacaoCurso.PodeTransitar(FluxoPublicacaoCurso.Publicado, destino));

    [Fact]
    public void Status_desconhecido_ou_nulo_nao_transita()
    {
        Assert.False(FluxoPublicacaoCurso.PodeTransitar(null, FluxoPublicacaoCurso.Pendente));
        Assert.False(FluxoPublicacaoCurso.PodeTransitar("Publicaldo", FluxoPublicacaoCurso.Publicado));
    }

    [Fact]
    public void Comparacao_de_status_ignora_caixa()
    {
        // A coluna do banco e VARCHAR livre: dados antigos podem ter outra caixa.
        Assert.True(FluxoPublicacaoCurso.PodeTransitar("rascunho", FluxoPublicacaoCurso.Pendente));
        Assert.True(FluxoPublicacaoCurso.PermiteEdicao("RASCUNHO"));
    }

    [Theory]
    [InlineData(FluxoPublicacaoCurso.Rascunho, true)]
    [InlineData(FluxoPublicacaoCurso.Rejeitado, true)]
    [InlineData(FluxoPublicacaoCurso.Pendente, false)]
    [InlineData(FluxoPublicacaoCurso.Publicado, false)]
    public void Conteudo_so_e_editavel_antes_da_avaliacao(string status, bool esperado)
        => Assert.Equal(esperado, FluxoPublicacaoCurso.PermiteEdicao(status));
}
