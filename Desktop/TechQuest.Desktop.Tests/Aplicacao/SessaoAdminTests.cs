using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Modelos;
using TechQuest.Desktop.Tests.Dubles;
using Xunit;

namespace TechQuest.Desktop.Tests.Aplicacao;

public class SessaoAdminTests
{
    [Fact]
    public void Token_vale_ate_o_vencimento_e_some_depois()
    {
        var relogio = new RelogioFalso();
        var sessao = new SessaoAdmin(relogio);
        sessao.Iniciar(Fabrica.Resposta(PapelUsuario.Admin, relogio.GetUtcNow().UtcDateTime.AddHours(8)));

        Assert.Equal("token-teste", sessao.Token);

        relogio.Avancar(TimeSpan.FromHours(9));

        Assert.Null(sessao.Token);
        Assert.False(sessao.Ativa);
    }

    [Theory]
    [InlineData(PapelUsuario.Estudante)]
    [InlineData(PapelUsuario.Tutor)]
    public void Sessao_nao_aceita_quem_nao_e_admin(PapelUsuario papel)
    {
        var sessao = new SessaoAdmin();
        Assert.Throws<InvalidOperationException>(() => sessao.Iniciar(Fabrica.Resposta(papel)));
    }

    [Fact]
    public void Expirar_avisa_uma_unica_vez()
    {
        var sessao = new SessaoAdmin();
        sessao.Iniciar(Fabrica.Resposta(PapelUsuario.Admin));
        var avisos = 0;
        sessao.Expirou += (_, _) => avisos++;

        sessao.Expirar();
        sessao.Expirar(); // duas telas recebendo 401 ao mesmo tempo

        Assert.Equal(1, avisos);
        Assert.Null(sessao.Usuario);
    }
}
