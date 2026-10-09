namespace TechQuest.Desktop.UI.Estilo;

/// <summary>
/// Fontes usadas em texto montado por codigo (as revisoes em RichTextBox).
///
/// Instancias unicas e estaticas: Font e um recurso do Windows (GDI), e criar
/// uma nova a cada paragrafo renderizado vazaria handles. Vivem o programa
/// inteiro, entao nao precisam de Dispose.
/// </summary>
internal static class Fontes
{
    public static readonly Font Normal = new("Segoe UI", 10F);
    public static readonly Font Negrito = new("Segoe UI", 10F, FontStyle.Bold);
    public static readonly Font Pequena = new("Segoe UI", 9F);
    public static readonly Font PequenaNegrito = new("Segoe UI", 9F, FontStyle.Bold);
    public static readonly Font Titulo = new("Segoe UI", 12F, FontStyle.Bold);

    /// <summary>Monoespacada, como o .code-block do web (JetBrains Mono / Courier).</summary>
    public static readonly Font Codigo = new("Consolas", 10F);
}
