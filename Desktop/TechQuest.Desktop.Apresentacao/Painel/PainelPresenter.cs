using System.Globalization;
using TechQuest.Desktop.Aplicacao.Servicos;
using TechQuest.Desktop.Aplicacao.Sessao;
using TechQuest.Desktop.Apresentacao.Comum;
using TechQuest.Desktop.Apresentacao.Principal;
using TechQuest.Desktop.Modelos;

namespace TechQuest.Desktop.Apresentacao.Painel;

/// <summary>
/// Painel do administrador: os nove contadores de /api/admin/resumo, os
/// mesmos da home do admin no web, e o atalho para a fila de aprovacoes.
/// </summary>
public sealed class PainelPresenter : PresenterBase
{
    /// <summary>
    /// Cultura fixa, e nao a do Windows da maquina: o painel sai igual em
    /// qualquer computador do laboratorio, e o teste nao depende do idioma
    /// de quem o roda.
    /// </summary>
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IPainelView _view;
    private readonly PainelService _servico;
    private readonly INavegador _navegador;
    private readonly TimeProvider _relogio;

    public PainelPresenter(
        IPainelView view, PainelService servico, INavegador navegador,
        SessaoAdmin sessao, TimeProvider? relogio = null)
        : base(view, sessao)
    {
        _view = view;
        _servico = servico;
        _navegador = navegador;
        _relogio = relogio ?? TimeProvider.System;
    }

    public async Task CarregarAsync()
    {
        _view.OcultarErro();

        ResumoAdmin? resumo = null;
        var concluiu = await ExecutarAsync(async () => resumo = await _servico.ObterResumoAsync());

        // Em falha a mensagem ja foi exibida pela classe base. Os numeros
        // anteriores ficam na tela: melhor que zera-los, porque zero seria
        // lido como dado ("nenhum usuario") e nao como ausencia de dado.
        if (!concluiu || resumo is null) return;

        _view.Exibir(Montar(resumo, _relogio.GetLocalNow()));
    }

    public void IrParaAprovacoes() => _navegador.Navegar(SecaoAdmin.Aprovacoes);

    internal static PainelExibicao Montar(ResumoAdmin r, DateTimeOffset agora)
    {
        var pendentes = r.CursosPendentes;

        return new PainelExibicao(
            TotalUsuarios: Numero(r.TotalUsuarios),
            UsuariosAtivos: Numero(r.UsuariosAtivos),
            DetalheAtivos: DetalheAtivos(r.UsuariosAtivos, r.TotalUsuarios),
            Estudantes: Numero(r.Estudantes),
            Tutores: Numero(r.Tutores),
            CursosPublicados: Numero(r.CursosPublicados),
            CursosPendentes: Numero(pendentes),
            Matriculas: Numero(r.TotalMatriculas),
            Certificados: Numero(r.CertificadosEmitidos),
            ChamadosAbertos: Numero(r.ChamadosAbertos),
            HaAprovacoesPendentes: pendentes > 0,
            TituloAprovacoes: pendentes switch
            {
                0 => "Nenhuma aprovação pendente",
                1 => "1 curso aguardando avaliação",
                _ => $"{Numero(pendentes)} cursos aguardando avaliação"
            },
            DetalheAprovacoes: pendentes > 0
                ? "Enviados por tutores. Revise o conteúdo e a prova antes de publicar."
                : "A fila de cursos submetidos está vazia.",
            AtualizadoEm: "Atualizado às " + agora.ToString("HH:mm", PtBr));
    }

    /// <summary>
    /// Mesma conta do web (Math.round): arredondamento comercial, .5 para
    /// cima. O padrao do .NET seria o "do banqueiro" (.5 para o par), e o
    /// mesmo dado mostraria percentuais diferentes no web e no desktop.
    /// </summary>
    internal static string DetalheAtivos(int ativos, int total)
    {
        if (total <= 0) return "sem usuários cadastrados";
        var pct = (int)Math.Round(ativos * 100.0 / total, MidpointRounding.AwayFromZero);
        return $"{pct}% da base";
    }

    private static string Numero(int n) => n.ToString("N0", PtBr);
}
