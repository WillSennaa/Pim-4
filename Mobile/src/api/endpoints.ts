import { http } from './cliente';
import type {
  AlunoDoTutor, Certificado, Chamado, CursoDetalhe, CursoResumo, CursoTutor,
  DadosCurso, ItemHistorico, LinhaDesempenho, LoginResponse, Matricula, Medalha,
  NovaQuestao, Perfil, ProgressoAtualizado, Prova, ProvaCompletaTutor,
  ResultadoTentativa, StatusChamado
} from './tipos';

/* ==========================================================================
   Endpoints — a mesma lista de Web/js/api.js, com tipos
   --------------------------------------------------------------------------
   Os nomes seguem os do web (cursos, curso, matricular, progressoAula...) para
   quem conhece um cliente reconhecer o outro. Nenhum endpoint de /api/admin
   aparece aqui: o perfil Administrador é atendido pelo desktop.
   ========================================================================== */

const id = (n: number | string) => encodeURIComponent(String(n));

export const API = {
  saude: () => http.get<{ status: string; banco: string }>('/api/health'),

  // ---------- autenticação ----------
  login: (email: string, senha: string) =>
    http.post<LoginResponse>('/api/auth/login', { email, senha }),
  registrar: (nome: string, email: string, senha: string) =>
    http.post<LoginResponse>('/api/auth/registrar', { nome, email, senha }),

  // ---------- estudante ----------
  cursos: () => http.get<CursoResumo[]>('/api/cursos'),
  curso: (idCurso: number) => http.get<CursoDetalhe>('/api/cursos/' + id(idCurso)),
  matricular: (idCurso: number) => http.post<Matricula>('/api/cursos/' + id(idCurso) + '/matricula'),
  progressoAula: (idMaterial: number, concluido: boolean, porcentagemAssistida: number) =>
    http.put<ProgressoAtualizado>('/api/materiais/' + id(idMaterial) + '/progresso',
      { concluido, porcentagemAssistida }),
  prova: (idProva: number) => http.get<Prova>('/api/provas/' + id(idProva)),
  submeterProva: (idProva: number, respostas: { idQuestao: number; letra: string }[]) =>
    http.post<ResultadoTentativa>('/api/provas/' + id(idProva) + '/tentativas', { respostas }),

  // ---------- perfil e conta (qualquer papel) ----------
  perfil: () => http.get<Perfil>('/api/perfil'),
  salvarPerfil: (dados: { telefone: string | null; dataNascimento: string | null; cidade: string | null }) =>
    http.put<null>('/api/perfil', dados),
  historico: () => http.get<ItemHistorico[]>('/api/perfil/historico'),
  conquistas: () => http.get<Medalha[]>('/api/perfil/conquistas'),
  certificados: () => http.get<Certificado[]>('/api/perfil/certificados'),
  desempenho: () => http.get<LinhaDesempenho[]>('/api/perfil/desempenho'),
  validarCertificado: (codigo: string) => http.get<Certificado>('/api/certificados/' + id(codigo)),
  trocarSenha: (senhaAtual: string, senhaNova: string) =>
    http.put<null>('/api/perfil/senha', { senhaAtual, senhaNova }),
  trocarEmail: (senha: string, emailNovo: string) =>
    http.put<{ email: string }>('/api/perfil/email', { senha, emailNovo }),
  desativarConta: (senha: string) => http.post<null>('/api/perfil/desativar', { senha }),

  // ---------- chamados (dúvidas, suporte, mensagens) ----------
  chamados: () => http.get<Chamado[]>('/api/chamados'),
  abrirChamado: (tipo: string, assunto: string, descricao: string,
                 idDestinatario: number | null, idCurso: number | null) =>
    http.post<Chamado>('/api/chamados', { tipo, assunto, descricao, idDestinatario, idCurso }),
  chamadosRecebidos: () => http.get<Chamado[]>('/api/chamados/recebidos'),
  responderChamado: (idChamado: number, descricao: string) =>
    http.post<null>('/api/chamados/' + id(idChamado) + '/responder', { descricao }),
  statusChamado: (idChamado: number, status: StatusChamado) =>
    http.patch<null>('/api/chamados/' + id(idChamado) + '/status', { status }),

  // ---------- tutor ----------
  tutorCursos: () => http.get<CursoTutor[]>('/api/tutor/cursos'),
  tutorCriarCurso: (dados: DadosCurso) => http.post<CursoTutor>('/api/tutor/cursos', dados),
  tutorAtualizarCurso: (idCurso: number, dados: DadosCurso) =>
    http.put<CursoTutor>('/api/tutor/cursos/' + id(idCurso), dados),
  tutorSubmeterCurso: (idCurso: number) =>
    http.post<CursoTutor>('/api/tutor/cursos/' + id(idCurso) + '/submeter'),
  tutorAdicionarAula: (idCurso: number, titulo: string, tipo: string, conteudo: string | null) =>
    http.post('/api/tutor/cursos/' + id(idCurso) + '/aulas', { titulo, tipo, conteudo }),
  tutorAtualizarAula: (idMaterial: number, titulo: string, tipo: string, conteudo: string | null) =>
    http.put('/api/tutor/aulas/' + id(idMaterial), { titulo, tipo, conteudo }),
  tutorRemoverAula: (idMaterial: number) => http.del<null>('/api/tutor/aulas/' + id(idMaterial)),
  tutorCriarProva: (idCurso: number, dados: { titulo: string; notaMinima: number; tempoMinutos: number }) =>
    http.post('/api/tutor/cursos/' + id(idCurso) + '/prova', dados),
  tutorAdicionarQuestao: (idProva: number, dados: NovaQuestao) =>
    http.post('/api/tutor/provas/' + id(idProva) + '/questoes', dados),
  tutorProvaCompleta: (idProva: number) =>
    http.get<ProvaCompletaTutor>('/api/tutor/provas/' + id(idProva)),
  tutorRemoverQuestao: (idQuestao: number) => http.del<null>('/api/tutor/questoes/' + id(idQuestao)),
  tutorAlunos: (idCurso?: number) =>
    http.get<AlunoDoTutor[]>('/api/tutor/alunos' + (idCurso ? '?idCurso=' + id(idCurso) : ''))
};
