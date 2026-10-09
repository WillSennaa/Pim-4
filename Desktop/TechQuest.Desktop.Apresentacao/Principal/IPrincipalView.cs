namespace TechQuest.Desktop.Apresentacao.Principal;

/// <summary>Como o programa deixou a janela principal.</summary>
public enum SaidaPrincipal
{
    /// <summary>Fechou a janela: o programa termina.</summary>
    Encerrar,

    /// <summary>Saiu da conta ou a sessao expirou: volta ao login.</summary>
    VoltarAoLogin
}

/// <summary>
/// Janela principal: menu lateral, barra superior e area central.
/// Nao herda IViewBase porque nao faz chamada a API por conta propria;
/// cada secao carregada no centro e uma view com seu proprio presenter.
/// </summary>
public interface IPrincipalView
{
    void MostrarUsuario(string nome, string iniciais);

    /// <summary>Troca o conteudo central e destaca o item do menu.</summary>
    void ExibirSecao(SecaoAdmin secao, string titulo);

    /// <summary>
    /// Numero ao lado do item do menu (cursos aguardando avaliacao).
    /// null ou zero: sem selo. "Nao sei" e "nenhum" aparecem igual, como no web.
    /// </summary>
    void AtualizarSelo(SecaoAdmin secao, int? quantidade);

    bool ConfirmarSaida();

    void AvisarSessaoExpirada();

    void Fechar(SaidaPrincipal saida);
}
