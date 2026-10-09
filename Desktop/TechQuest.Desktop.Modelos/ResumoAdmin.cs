namespace TechQuest.Desktop.Modelos;

/// <summary>
/// Resposta de GET /api/admin/resumo: contadores da plataforma inteira.
///
/// O QUE CADA NUMERO CONTA, conforme AdminRepository.ResumoAsync no servidor
/// (importante para nao rotular errado na tela):
///   TotalMatriculas  = linhas de Historico (cada matricula de cada aluno);
///   ChamadosAbertos  = chamados com status "Aberto" de QUALQUER tipo,
///                      duvidas enderecadas a tutores inclusive. Nao e o
///                      tamanho da fila tecnica do administrador.
/// </summary>
public record ResumoAdmin(
    int TotalUsuarios,
    int UsuariosAtivos,
    int Estudantes,
    int Tutores,
    int CursosPublicados,
    int CursosPendentes,
    int TotalMatriculas,
    int CertificadosEmitidos,
    int ChamadosAbertos);
