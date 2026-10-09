namespace TechQuest.Application.Dtos;

/// <summary>
/// Abertura de chamado.
///
/// O DESTINATARIO E DECIDIDO PELO SERVIDOR, nao pelo cliente: uma duvida de
/// conteudo vai ao tutor do curso informado em IdCurso, e um chamado tecnico
/// vai para a fila dos administradores. Deixar o cliente escolher para quem
/// manda permitiria enderecar a qualquer usuario -- e obrigaria a tela a
/// saber quem e o tutor de cada curso, informacao que ela nao tem por que ter.
///
/// IdDestinatario permanece para a RESPOSTA do tutor, que enderaca de volta a
/// quem perguntou.
/// </summary>
public record AbrirChamadoRequest(
    string Tipo, string Assunto, string Descricao, int? IdDestinatario, int? IdCurso);

/// <summary>
/// Os IDs de remetente e destinatario acompanham os nomes porque quem recebe
/// o chamado precisa poder RESPONDER a quem o abriu. Sem o ID, a tela teria de
/// adivinhar o destinatario pelo nome — que nao e unico nem estavel.
/// </summary>
public record ChamadoDto(
    int Id,
    string? Tipo,
    string? Assunto,
    string? Descricao,
    int IdRemetente,
    string Remetente,
    int? IdDestinatario,
    string? Destinatario,
    DateTime DataAbertura,
    string Status,
    string? Curso);

public record ResponderChamadoRequest(string Descricao);

/// <summary>Tipos de chamado reconhecidos pelo roteamento.</summary>
public static class TipoChamado
{
    /// <summary>Duvida sobre o conteudo de um curso. Vai ao tutor do curso.</summary>
    public const string Duvida = "Duvida";

    /// <summary>Problema na plataforma. Vai para a fila dos administradores.</summary>
    public const string Tecnico = "Tecnico";

    /// <summary>Resposta de um tutor a uma duvida.</summary>
    public const string Resposta = "Resposta";

    /// <summary>Parecer do administrador sobre um curso submetido.</summary>
    public const string Avaliacao = "Avaliacao";

    /// <summary>Mensagem direta do tutor para um aluno.</summary>
    public const string Mensagem = "Mensagem";
}

/// <summary>Mudanca de situacao pelo responsavel.</summary>
public record AtualizarStatusChamadoRequest(string Status, string? Resolucao);

