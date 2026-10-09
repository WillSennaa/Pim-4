using TechQuest.Desktop.Apresentacao.Comum;

namespace TechQuest.Desktop.Apresentacao.Login;

public interface ILoginView : IViewBase
{
    string Email { get; }
    string Senha { get; }

    /// <summary>Apaga so a senha depois de uma tentativa recusada.</summary>
    void LimparSenha();

    /// <summary>Login aceito: a tela se fecha e o programa abre a principal.</summary>
    void ConcluirLogin();
}
