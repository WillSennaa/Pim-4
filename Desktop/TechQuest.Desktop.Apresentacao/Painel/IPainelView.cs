using TechQuest.Desktop.Apresentacao.Comum;

namespace TechQuest.Desktop.Apresentacao.Painel;

public interface IPainelView : IViewBase
{
    /// <summary>Preenche os cartoes. Tudo chega pronto para exibir.</summary>
    void Exibir(PainelExibicao dados);

    /// <summary>Esconde o aviso de falha antes de uma nova tentativa.</summary>
    void OcultarErro();
}

/// <summary>
/// O que a tela mostra, ja formatado.
///
/// POR QUE O PRESENTER FORMATA E NAO A TELA: "75% da base", "1 curso" x
/// "3 cursos", milhar com ponto. Sao decisoes de apresentacao com regra
/// (arredondamento, divisao por zero, plural), e regra testavel mora no
/// presenter. A tela so copia texto para os rotulos.
/// </summary>
public sealed record PainelExibicao(
    string TotalUsuarios,
    string UsuariosAtivos,
    string DetalheAtivos,
    string Estudantes,
    string Tutores,
    string CursosPublicados,
    string CursosPendentes,
    string Matriculas,
    string Certificados,
    string ChamadosAbertos,
    bool HaAprovacoesPendentes,
    string TituloAprovacoes,
    string DetalheAprovacoes,
    string AtualizadoEm);
