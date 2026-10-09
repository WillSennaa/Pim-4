namespace TechQuest.Desktop.Apresentacao.Comum;

/// <summary>
/// O que TODA tela sabe fazer, do ponto de vista do presenter.
///
/// O QUE E O MVP (Model-View-Presenter): a tela (View) so desenha e repassa
/// cliques; a decisao do que fazer com o clique fica no Presenter, que
/// conversa com a tela por esta interface, sem saber que existe Windows Forms.
///
/// POR QUE: o presenter vira uma classe C# comum. Compila em qualquer
/// sistema, pode ser testado com uma "tela falsa" que implementa a interface,
/// e a logica de cada tela nao fica espalhada em handlers de botao.
///
/// ALTERNATIVA REJEITADA: o Form chamar o servico direto no clique. Mais
/// curto, mas mistura desenho com decisao, e so se testaria abrindo janela.
/// </summary>
public interface IViewBase
{
    /// <summary>Trava a tela e mostra espera durante a chamada a API.</summary>
    void DefinirOcupado(bool ocupado);

    void MostrarErro(string mensagem);
}
