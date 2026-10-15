/* ==========================================================================
   Contrato da API — espelho dos DTOs de TechQuest.Application/Dtos
   --------------------------------------------------------------------------
   O ASP.NET Core serializa em camelCase por padrão, então "IdEstudante" do C#
   chega aqui como "idEstudante". Cada tipo leva o nome do record C# que o
   origina, para a correspondência ser direta na hora de conferir.

   POR QUE O APP NÃO REFERENCIA O PROJETO C#: é outra linguagem e outro
   processo. A mesma decisão foi tomada no desktop, que tem os próprios
   modelos do JSON em vez de referenciar TechQuest.Application. O cliente
   depende do CONTRATO (o JSON), nunca da implementação do servidor.
   ========================================================================== */

/** PapelUsuario (enum C#). Sem JsonStringEnumConverter, chega como número. */
export type PapelApi = 1 | 2 | 3 | 'Estudante' | 'Tutor' | 'Admin';
export type Papel = 'estudante' | 'tutor' | 'admin';

// ---------------- Autenticação ----------------
export interface UsuarioLogado {           // UsuarioLogadoDto
  id: number;
  nome: string;
  email: string | null;
  iniciais: string;
  papel: PapelApi;
  idEstudante: number | null;
  xp: number;
  nivel: number;
  xpProximoNivel: number;
  progressoNivel: number;
}

export interface LoginResponse {           // LoginResponse
  token: string;
  expiraEm: string;
  usuario: UsuarioLogado;
}

// ---------------- Cursos ----------------
export interface CursoResumo {             // CursoResumoDto
  id: number;
  nome: string;
  descricao: string | null;
  categoria: string | null;
  nivel: string | null;
  duracaoHoras: number | null;
  instrutor: string | null;
  status: string;
  temProva: boolean;
}

export interface Material {                // MaterialDto
  id: number;
  titulo: string | null;
  tipo: string | null;
  conteudo: string | null;
  concluido: boolean;
  porcentagemAssistida: number;
}

export interface CursoDetalhe {            // CursoDetalheDto
  id: number;
  nome: string;
  descricao: string | null;
  categoria: string | null;
  nivel: string | null;
  duracaoHoras: number | null;
  instrutor: string | null;
  instrutorIniciais: string | null;
  status: string;
  idProva: number | null;
  matriculado: boolean;
  materiais: Material[];
}

export interface Matricula {               // MatriculaDto
  idHistorico: number;
  idCurso: number;
  curso: string;
  status: string;
}

export interface ProgressoAtualizado {     // ProgressoAtualizadoDto
  idMaterial: number;
  concluido: boolean;
  porcentagemAssistida: number;
  aulasConcluidas: number;
  totalAulas: number;
  percentualCurso: number;
  cursoConcluido: boolean;
  xpAtual: number;
  nivel: number;
  medalhasNovas: string[];
}

// ---------------- Prova ----------------
export interface Alternativa { id: number; letra: string | null; texto: string | null; }

export interface Questao {                 // QuestaoDto (sem gabarito)
  id: number;
  ordem: number;
  enunciado: string;
  codigoExemplo: string | null;
  alternativas: Alternativa[];
}

export interface Prova {                   // ProvaDto
  id: number;
  titulo: string | null;
  notaMinima: number;
  tempoMinutos: number;
  totalQuestoes: number;
  questoes: Questao[];
}

export interface CorrecaoQuestao {         // CorrecaoQuestaoDto
  idQuestao: number;
  ordem: number;
  letraMarcada: string | null;
  letraCorreta: string | null;
  acertou: boolean;
}

export interface ResultadoTentativa {      // ResultadoTentativaDto
  idProva: number;
  acertos: number;
  totalQuestoes: number;
  nota: number;
  notaMinima: number;
  aprovado: boolean;
  numeroTentativa: number;
  xpGanho: number;
  medalhasNovas: string[];
  correcao: CorrecaoQuestao[];
}

// ---------------- Perfil ----------------
export interface ItemHistorico {           // ItemHistoricoDto
  idHistorico: number;
  idCurso: number;
  curso: string;
  status: string | null;
  dataConclusao: string | null;
  aulasConcluidas: number;
  totalAulas: number;
  percentualCurso: number;
  codigoCertificado: string | null;
}

export interface Perfil {                  // PerfilDto
  id: number;
  nome: string;
  email: string | null;
  iniciais: string;
  papel: string;
  telefone: string | null;
  dataNascimento: string | null;
  cidade: string | null;
  dataCadastro: string;
  xp: number;
  nivel: number;
  xpProximoNivel: number;
  progressoNivel: number;
  cursosConcluidos: number;
  aulasConcluidas: number;
  provasAprovadas: number;
  medalhas: number;
}

export interface Medalha {                 // MedalhaDto
  id: number;
  nome: string;
  descricao: string | null;
  raridade: string | null;
  conquistada: boolean;
  dataConquista: string | null;
}

export interface Certificado {             // CertificadoDto
  id: number;
  codigoAutenticacao: string;
  idCurso: number;
  curso: string;
  estudante: string;
  dataEmissao: string;
  dataConclusao: string | null;
  cargaHoraria: number | null;
  instrutor: string | null;
}

export interface LinhaDesempenho {         // LinhaRelatorioDesempenhoDto (procedure)
  estudante: string;
  prova: string | null;
  nota: number | null;
  tentativas: number;
  dataRealizacao: string;
}

// ---------------- Chamados ----------------
export type TipoChamado = 'Duvida' | 'Tecnico' | 'Resposta' | 'Avaliacao' | 'Mensagem';
export type StatusChamado = 'Aberto' | 'Em andamento' | 'Resolvido';

export interface Chamado {                 // ChamadoDto
  id: number;
  tipo: string | null;
  assunto: string | null;
  descricao: string | null;
  idRemetente: number;
  remetente: string;
  idDestinatario: number | null;
  destinatario: string | null;
  dataAbertura: string;
  status: string;
  curso: string | null;
}

// ---------------- Tutor ----------------
export interface CursoTutor {              // CursoTutorDto
  id: number;
  nome: string;
  descricao: string | null;
  categoria: string | null;
  nivel: string | null;
  duracaoHoras: number | null;
  status: string;
  totalAulas: number;
  totalMatriculados: number;
  idProva: number | null;
  totalQuestoes: number;
  podeEditar: boolean;
}

export interface DadosCurso {              // CriarCursoRequest
  nome: string;
  descricao: string | null;
  categoria: string | null;
  nivel: string | null;
  duracaoHoras: number | null;
}

export interface AlunoDoTutor {            // AlunoDoTutorDto
  idEstudante: number;
  idUsuario: number;
  nome: string;
  email: string | null;
  idCurso: number;
  curso: string;
  status: string | null;
  aulasConcluidas: number;
  totalAulas: number;
  percentualCurso: number;
  melhorNota: number | null;
  tentativas: number;
}

export interface AlternativaCompleta { id: number; letra: string | null; texto: string | null; ehCorreta: boolean; }

export interface QuestaoCompletaTutor {    // QuestaoCompletaTutorDto
  id: number;
  ordem: number;
  enunciado: string;
  codigoExemplo: string | null;
  alternativas: AlternativaCompleta[];
}

export interface ProvaCompletaTutor {      // ProvaCompletaTutorDto
  id: number;
  titulo: string | null;
  notaMinima: number;
  tempoMinutos: number;
  questoes: QuestaoCompletaTutor[];
}

export interface NovaQuestao {             // CriarQuestaoRequest
  enunciado: string;
  codigoExemplo: string | null;
  ordem: number;
  alternativas: { letra: string; texto: string; ehCorreta: boolean }[];
}
