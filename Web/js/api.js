/* ==========================================================================
   Tech Quest — Cliente da API (PIM IV)
   --------------------------------------------------------------------------
   Camada única de comunicação com o back-end. Nenhuma página faz fetch
   direto: toda chamada passa por aqui.

   POR QUE UM ARQUIVO SÓ: a URL da API, o envio do token, o tratamento de
   sessão expirada e o formato de erro ficam definidos em um lugar. Quando a
   API for publicada no Azure, muda UMA linha (API.base) e as 47 páginas
   passam a apontar para a nuvem.
   ========================================================================== */

const API = {
  // Porta do perfil "https" do Visual Studio (Properties/launchSettings.json).
  // Ao publicar no Azure, troque por https://techquest-api.azurewebsites.net
  base: 'https://localhost:7160',

  // sessionStorage e não localStorage: o token morre quando a aba fecha.
  // localStorage mantém o token indefinidamente, inclusive em computador
  // compartilhado — laboratório da faculdade é exatamente esse caso.
  CHAVE_TOKEN: 'tq_token',
  CHAVE_USUARIO: 'tq_usuario',

  // ---------------- sessão ----------------
  get token() { return sessionStorage.getItem(this.CHAVE_TOKEN); },

  get usuario() {
    const bruto = sessionStorage.getItem(this.CHAVE_USUARIO);
    return bruto ? JSON.parse(bruto) : null;
  },

  get autenticado() { return !!this.token; },

  /** Papel em minúsculas, no vocabulário que o front-end já usa. */
  get papel() {
    const u = this.usuario;
    if (!u) return null;
    const mapa = { 1: 'estudante', 2: 'tutor', 3: 'admin', Estudante: 'estudante', Tutor: 'tutor', Admin: 'admin' };
    return mapa[u.papel] || String(u.papel || '').toLowerCase();
  },

  salvarSessao(resposta) {
    sessionStorage.setItem(this.CHAVE_TOKEN, resposta.token);
    sessionStorage.setItem(this.CHAVE_USUARIO, JSON.stringify(resposta.usuario));
  },

  limparSessao() {
    sessionStorage.removeItem(this.CHAVE_TOKEN);
    sessionStorage.removeItem(this.CHAVE_USUARIO);
  },

  // ---------------- requisição ----------------
  /**
   * Faz a chamada HTTP. Lança um objeto { status, mensagem } em caso de erro,
   * para a página poder exibir a mensagem que o servidor devolveu em vez de
   * um "erro desconhecido".
   */
  async request(metodo, rota, corpo) {
    const cabecalhos = { 'Accept': 'application/json' };
    if (corpo !== undefined) cabecalhos['Content-Type'] = 'application/json';
    if (this.token) cabecalhos['Authorization'] = 'Bearer ' + this.token;

    let resposta;
    try {
      resposta = await fetch(this.base + rota, {
        method: metodo,
        headers: cabecalhos,
        body: corpo !== undefined ? JSON.stringify(corpo) : undefined
      });
    } catch (e) {
      // Falha de rede: API desligada, certificado não aceito ou CORS.
      throw { status: 0, mensagem: 'Não foi possível falar com o servidor. Verifique se a API está rodando.' };
    }

    // 401 = token ausente, inválido ou expirado. Sessão encerrada e volta
    // para o login — tratado aqui uma vez, e não em cada página.
    if (resposta.status === 401) {
      const eraLogin = rota.startsWith('/api/auth/');
      if (!eraLogin) {
        this.limparSessao();
        if (typeof TQ !== 'undefined') window.location.href = TQ.getPath('pages/login.html?expirou=1');
        throw { status: 401, mensagem: 'Sessão expirada.' };
      }
    }

    if (resposta.status === 204) return null;

    let dados = null;
    const texto = await resposta.text();
    if (texto) { try { dados = JSON.parse(texto); } catch { dados = texto; } }

    if (!resposta.ok) {
      throw {
        status: resposta.status,
        mensagem: (dados && dados.mensagem) || 'Erro ' + resposta.status + '.',
        identificador: dados && dados.identificador
      };
    }

    return dados;
  },

  get(rota) { return this.request('GET', rota); },
  post(rota, corpo) { return this.request('POST', rota, corpo); },
  put(rota, corpo) { return this.request('PUT', rota, corpo); },
  patch(rota, corpo) { return this.request('PATCH', rota, corpo); },
  del(rota) { return this.request('DELETE', rota); },

  // ---------------- endpoints ----------------
  saude: () => API.get('/api/health'),

  login: (email, senha) => API.post('/api/auth/login', { email, senha }),
  registrar: (nome, email, senha) => API.post('/api/auth/registrar', { nome, email, senha }),

  // estudante
  cursos: () => API.get('/api/cursos'),
  curso: (id) => API.get('/api/cursos/' + id),
  matricular: (id) => API.post('/api/cursos/' + id + '/matricula'),
  progressoAula: (idMaterial, concluido, porcentagem) =>
    API.put('/api/materiais/' + idMaterial + '/progresso',
            { concluido, porcentagemAssistida: porcentagem }),
  prova: (id) => API.get('/api/provas/' + id),
  submeterProva: (id, respostas) => API.post('/api/provas/' + id + '/tentativas', { respostas }),
  perfil: () => API.get('/api/perfil'),
  salvarPerfil: (dados) => API.put('/api/perfil', dados),
  historico: () => API.get('/api/perfil/historico'),
  conquistas: () => API.get('/api/perfil/conquistas'),
  certificados: () => API.get('/api/perfil/certificados'),
  desempenho: () => API.get('/api/perfil/desempenho'),
  validarCertificado: (codigo) => API.get('/api/certificados/' + codigo),

  // chamados (dúvidas)
  chamados: () => API.get('/api/chamados'),
  abrirChamado: (tipo, assunto, descricao, idDestinatario) =>
    API.post('/api/chamados', { tipo, assunto, descricao, idDestinatario }),
  fecharChamado: (id) => API.post('/api/chamados/' + id + '/fechar'),

  // tutor
  tutorCursos: () => API.get('/api/tutor/cursos'),
  tutorCriarCurso: (dados) => API.post('/api/tutor/cursos', dados),
  tutorAtualizarCurso: (id, dados) => API.put('/api/tutor/cursos/' + id, dados),
  tutorSubmeterCurso: (id) => API.post('/api/tutor/cursos/' + id + '/submeter'),
  tutorAdicionarAula: (idCurso, titulo, tipo) =>
    API.post('/api/tutor/cursos/' + idCurso + '/aulas', { titulo, tipo }),
  tutorRemoverAula: (idMaterial) => API.del('/api/tutor/aulas/' + idMaterial),
  tutorCriarProva: (idCurso, dados) => API.post('/api/tutor/cursos/' + idCurso + '/prova', dados),
  tutorAdicionarQuestao: (idProva, dados) => API.post('/api/tutor/provas/' + idProva + '/questoes', dados),
  tutorAlunos: (idCurso) => API.get('/api/tutor/alunos' + (idCurso ? '?idCurso=' + idCurso : '')),
  tutorConcederMedalha: (idEstudante, idMedalha) =>
    API.post('/api/tutor/estudantes/' + idEstudante + '/medalhas/' + idMedalha),

  // admin
  adminResumo: () => API.get('/api/admin/resumo'),
  adminCursos: (status) => API.get('/api/admin/cursos' + (status ? '?status=' + status : '')),
  adminAprovar: (id) => API.post('/api/admin/cursos/' + id + '/aprovar'),
  adminRejeitar: (id, motivo) => API.post('/api/admin/cursos/' + id + '/rejeitar', { motivo }),
  adminUsuarios: () => API.get('/api/admin/usuarios'),
  adminCriarUsuario: (dados) => API.post('/api/admin/usuarios', dados),
  adminAlterarStatus: (id, ativo) => API.patch('/api/admin/usuarios/' + id + '/status', { ativo }),
  adminLogs: (limite) => API.get('/api/admin/logs?limite=' + (limite || 100))
};
