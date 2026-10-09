namespace TechQuest.Desktop.Apresentacao.Principal;

/// <summary>
/// Areas da janela principal. Sao as mesmas da barra lateral do admin no
/// web (pages/admin), para quem usa as duas versoes encontrar tudo no mesmo
/// lugar. Medalhas e Logs, que no web ficam dentro de outras paginas, aqui
/// ganham item proprio: no desktop ha espaco lateral de sobra.
/// </summary>
public enum SecaoAdmin
{
    Painel,
    Aprovacoes,
    Cursos,
    Usuarios,
    Chamados,
    Medalhas,
    Logs,
    MinhaConta
}

public static class SecaoAdminExtensoes
{
    /// <summary>Titulo exibido no topo e no menu.</summary>
    public static string Titulo(this SecaoAdmin secao) => secao switch
    {
        SecaoAdmin.Painel => "Painel",
        SecaoAdmin.Aprovacoes => "Aprovações de cursos",
        SecaoAdmin.Cursos => "Cursos",
        SecaoAdmin.Usuarios => "Usuários",
        SecaoAdmin.Chamados => "Chamados técnicos",
        SecaoAdmin.Medalhas => "Medalhas",
        SecaoAdmin.Logs => "Logs de auditoria",
        SecaoAdmin.MinhaConta => "Minha conta",
        _ => secao.ToString()
    };
}
