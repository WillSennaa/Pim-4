namespace TechQuest.Desktop.UI.Estilo;

/// <summary>
/// Paleta do Tech Quest, copiada das variaveis de css/style.css do web
/// (--color-primary, --color-bg...). Mantem a identidade visual do PIM III
/// tambem no desktop.
///
/// Os arquivos .Designer.cs repetem algumas destas cores como literais
/// (Color.FromArgb), porque o designer do Visual Studio so serializa valores
/// fixos. Esta classe e usada pelo codigo que muda cor em tempo de execucao,
/// como o destaque do item ativo no menu.
/// </summary>
internal static class Tema
{
    public static readonly Color Primaria = Color.FromArgb(37, 99, 235);       // #2563EB
    public static readonly Color PrimariaEscura = Color.FromArgb(30, 64, 175); // #1E40AF
    public static readonly Color PrimariaClara = Color.FromArgb(219, 234, 254);// #DBEAFE

    public static readonly Color Fundo = Color.FromArgb(241, 245, 249);        // #F1F5F9
    public static readonly Color Superficie = Color.White;
    public static readonly Color Borda = Color.FromArgb(226, 232, 240);        // #E2E8F0
    public static readonly Color FundoCodigo = Color.FromArgb(248, 250, 252);  // #F8FAFC (--color-surface-2)

    public static readonly Color Texto = Color.FromArgb(15, 23, 42);           // #0F172A
    public static readonly Color TextoSecundario = Color.FromArgb(71, 85, 105);// #475569
    public static readonly Color TextoSuave = Color.FromArgb(148, 163, 184);   // #94A3B8

    public static readonly Color Perigo = Color.FromArgb(185, 28, 28);         // #B91C1C
    public static readonly Color PerigoClaro = Color.FromArgb(254, 226, 226);  // #FEE2E2
    public static readonly Color Sucesso = Color.FromArgb(21, 128, 61);        // #15803D
    public static readonly Color SucessoClaro = Color.FromArgb(220, 252, 231); // #DCFCE7
    public static readonly Color Alerta = Color.FromArgb(180, 83, 9);          // #B45309
    public static readonly Color AlertaClaro = Color.FromArgb(254, 243, 199);  // #FEF3C7

    public static readonly Color Neutro = Color.FromArgb(100, 116, 139);       // #64748B
    public static readonly Color NeutroClaro = Color.FromArgb(241, 245, 249);  // #F1F5F9

    public static readonly Color MenuFundo = Color.FromArgb(15, 23, 42);       // #0F172A
    public static readonly Color MenuTexto = Color.FromArgb(203, 213, 225);    // #CBD5E1
    public static readonly Color MenuHover = Color.FromArgb(30, 41, 59);       // #1E293B
}
