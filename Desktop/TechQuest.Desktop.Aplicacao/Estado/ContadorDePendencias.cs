namespace TechQuest.Desktop.Aplicacao.Estado;

/// <summary>
/// Quantos cursos aguardam avaliacao, segundo a ultima leitura da API.
/// Alimenta o selo numerico do item "Aprovacoes" no menu lateral.
///
/// PADRAO OBSERVER: quem le o numero (Painel, fila de aprovacoes) o informa
/// aqui; quem o exibe (a janela principal) assina o evento Mudou. As telas
/// nao se conhecem: o Painel nao sabe que existe um menu, e o menu nao sabe
/// de onde o numero veio.
///
/// POR QUE NAO UMA REQUISICAO PROPRIA PARA O SELO: o numero ja chega de
/// graca em duas chamadas que acontecem de qualquer forma (o resumo do
/// Painel, que abre logo apos o login, e a lista de pendentes). Uma terceira
/// chamada so para o selo seria trafego repetido.
///
/// ALTERNATIVA REJEITADA: atualizar o selo por temporizador (consultar a API
/// a cada N segundos). Mostraria cursos submetidos enquanto o admin esta em
/// outra tela, mas gera trafego constante e, no Azure SQL serverless, impede
/// o banco de pausar, o que tem custo. O numero se atualiza a cada visita ao
/// Painel ou as Aprovacoes, como no web.
/// </summary>
public sealed class ContadorDePendencias
{
    /// <summary>null enquanto nenhuma leitura foi feita: "nao sei" nao e zero.</summary>
    public int? CursosPendentes { get; private set; }

    public event EventHandler? Mudou;

    public void Informar(int cursosPendentes)
    {
        if (cursosPendentes < 0) throw new ArgumentOutOfRangeException(nameof(cursosPendentes));
        if (CursosPendentes == cursosPendentes) return; // sem mudanca, sem aviso
        CursosPendentes = cursosPendentes;
        Mudou?.Invoke(this, EventArgs.Empty);
    }
}
