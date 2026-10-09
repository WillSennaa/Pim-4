namespace TechQuest.Desktop.Apresentacao.Principal;

/// <summary>
/// Permite que uma secao leve o usuario a outra (o atalho "Revisar cursos"
/// do Painel abre Aprovacoes, por exemplo).
///
/// POR QUE UMA INTERFACE: o presenter do Painel precisa pedir a navegacao,
/// mas nao deve conhecer a janela principal nem o PrincipalPresenter
/// inteiro (que tambem sabe fazer logout). Recebe so a capacidade de que
/// precisa. Num teste, um navegador falso registra o pedido.
/// </summary>
public interface INavegador
{
    void Navegar(SecaoAdmin secao);
}
