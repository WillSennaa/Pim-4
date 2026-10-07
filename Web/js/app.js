/* ==========================================================================
   Tech Quest - JavaScript principal
   INTEGRADO AO BACK-END (PIM IV)

   O que mudou em relação ao PIM III:
     - TQ.init() continua enchendo TQ.data e TQ.questoes, mas agora os dados
       de negócio vêm da API. O mock.json passou a ser apenas CATÁLOGO DE
       APRESENTAÇÃO (ícones, cores, FAQ) e fallback.
     - login, cadastro e logout deixaram de ser simulados.
     - o QUIZ não corrige mais no navegador: envia as respostas e recebe a
       nota do servidor.
     - páginas internas exigem token.

   POR QUE O FORMATO DE TQ.data FOI PRESERVADO: são 47 páginas lendo
   TQ.data.personas.estudante, TQ.data.cursos, TQ.data.admin_usuarios e
   afins. Mantendo o mesmo formato, a integração não exige reescrever página
   por página — o adaptador traduz a resposta da API para o formato que o
   front-end já consome.
   ========================================================================== */

// -------------- 1. Estado global --------------
const TQ = {
  data: null,
  questoes: null,
  personaAtual: 'estudante',
  online: false,          // true quando os dados vieram da API
  erroApi: null,

  init: async function () {
    // 1) Catálogo de apresentação + fallback: garante que nenhuma página
    //    quebre por campo ausente enquanto a API não cobre tudo.
    try {
      const mockRes = await fetch(this.getPath('data/mock.json'));
      this.data = await mockRes.json();
    } catch (e) {
      console.warn('mock.json indisponível:', e);
      this.data = {};
    }

    this.questoes = {};

    // 2) Dados reais. Sem token (index, login, cadastro) fica só o catálogo.
    if (typeof API === 'undefined' || !API.autenticado) return;

    this.personaAtual = API.papel || 'estudante';

    try {
      await ADAPTADOR.carregar(this.personaAtual);
      this.online = true;
    } catch (e) {
      this.erroApi = e;
      console.error('Falha ao carregar dados da API:', e);
    }
  },

  getPath: function (p) {
    const path = window.location.pathname;
    if (path.includes('/pages/estudante/') || path.includes('/pages/tutor/') || path.includes('/pages/admin/')) {
      return '../../' + p;
    }
    if (path.includes('/pages/')) {
      return '../' + p;
    }
    return p;
  }
};

// -------------- 1.1 Adaptador API -> formato do PIM III --------------
/**
 * Traduz as respostas da API para as chaves que as páginas já leem.
 *
 * Usa Promise.allSettled e não Promise.all de propósito: se UM endpoint
 * falhar, os outros continuam e a página mostra o que deu para carregar,
 * em vez de ficar inteira em branco.
 */
const ADAPTADOR = {
  async carregar(papel) {
    if (papel === 'tutor') return this.carregarTutor();
    if (papel === 'admin') return this.carregarAdmin();
    return this.carregarEstudante();
  },

  // ---------------- ESTUDANTE ----------------
  async carregarEstudante() {
    const [perfil, cursos, historico, conquistas, certificados] = await Promise.allSettled([
      API.perfil(), API.cursos(), API.historico(), API.conquistas(), API.certificados()
    ]);

    const d = TQ.data;

    if (perfil.status === 'fulfilled') {
      const p = perfil.value;
      d.personas = d.personas || {};
      d.personas.estudante = Object.assign({}, d.personas.estudante, {
        id: p.id,
        nome: p.nome,
        email: p.email,
        tipo: 'estudante',
        iniciais: p.iniciais,
        telefone: p.telefone || '',
        nascimento: FMT.data(p.dataNascimento),
        cidade: p.cidade || '',
        nivel: p.nivel,
        xp: p.xp,
        xp_proximo_nivel: p.xpProximoNivel,
        progresso_nivel: p.progressoNivel,
        provas_realizadas: p.provasAprovadas,
        medalhas: p.medalhas,
        certificados: p.cursosConcluidos,
        // Não existe no banco: é texto de interface, calculado do XP real.
        frase_motivacional: 'Faltam ' + Math.max(0, p.xpProximoNivel - p.xp) +
                            ' XP para você chegar ao nível ' + (p.nivel + 1) + '!'
      });
    }

    // Progresso por curso sai do histórico, para a listagem não precisar de
    // uma chamada por curso.
    const progressoPorCurso = {};
    if (historico.status === 'fulfilled') {
      historico.value.forEach(h => { progressoPorCurso[h.idCurso] = h.percentualCurso; });

      d.historico = historico.value.map(h => ({
        data: h.dataConclusao ? FMT.data(h.dataConclusao) : 'em andamento',
        tempo: h.status === 'Concluido' ? 'concluído' : h.percentualCurso + '% concluído',
        tipo: h.status === 'Concluido' ? 'concluido' : 'info',
        titulo: h.curso,
        subtitulo: h.aulasConcluidas + ' de ' + h.totalAulas + ' aulas'
      }));
    }

    if (cursos.status === 'fulfilled') {
      d.cursos = cursos.value.map(c => Object.assign(
        // Preserva ícone e cor do catálogo, casando pelo título.
        DECORACAO.doCurso(c.nome),
        {
          id: c.id,
          slug: FMT.slug(c.nome),
          titulo: c.nome,
          descricao: c.descricao,
          categoria: c.categoria,
          nivel: c.nivel,
          duracao_horas: c.duracaoHoras,
          instrutor: c.instrutor,
          instrutor_iniciais: FMT.iniciais(c.instrutor),
          progresso_aluno: progressoPorCurso[c.id] || 0,
          id_prova: c.temProva ? null : null   // preenchido em TQ.carregarProva
        }
      ));
    }

    if (conquistas.status === 'fulfilled') {
      const todas = conquistas.value;

      d.medalhas = todas.map(m => ({
        id: m.id,
        titulo: m.nome,
        descricao: m.descricao,
        status: m.conquistada ? 'conquistada' : 'bloqueada',
        data: m.dataConquista ? FMT.relativo(m.dataConquista) : null,
        icone: DECORACAO.daMedalha(m.nome)
      }));

      d.conquistas_recentes = todas
        .filter(m => m.conquistada)
        .sort((a, b) => new Date(b.dataConquista) - new Date(a.dataConquista))
        .slice(0, 3)
        .map(m => ({
          id: m.id,
          titulo: m.nome,
          xp: 75,                                  // XP por medalha (RegrasGamificacao)
          tempo: FMT.relativo(m.dataConquista),
          icone: DECORACAO.daMedalha(m.nome)
        }));
    }

    if (certificados.status === 'fulfilled') {
      d.certificados = certificados.value.map(c => ({
        id: c.id,
        curso_id: c.idCurso,
        curso_titulo: c.curso,
        status: 'emitido',
        data_emissao: FMT.data(c.dataEmissao),
        carga_horaria: c.cargaHoraria,
        codigo_validacao: c.codigoAutenticacao,
        instrutor: null
      }));
    }
  },

  // ---------------- TUTOR ----------------
  async carregarTutor() {
    const [cursos, alunos, chamados] = await Promise.allSettled([
      API.tutorCursos(), API.tutorAlunos(), API.chamados()
    ]);

    const d = TQ.data;
    const u = API.usuario;

    d.personas = d.personas || {};
    d.personas.tutor = Object.assign({}, d.personas.tutor, {
      id: u.id, nome: u.nome, email: u.email, tipo: 'tutor', iniciais: u.iniciais
    });

    if (cursos.status === 'fulfilled') {
      d.tutor_meus_cursos = cursos.value.map(c => ({
        id: c.id,
        titulo: c.nome,
        alunos: c.totalMatriculados,
        modulos: c.totalAulas,          // o modelo não tem módulo: são aulas
        rating: null,                   // não existe no banco
        status: (c.status || '').toLowerCase(),
        ultima_atualizacao: '—',
        pode_editar: c.podeEditar,
        id_prova: c.idProva,
        total_questoes: c.totalQuestoes
      }));

      d.personas.tutor.cursos_publicados =
        cursos.value.filter(c => (c.status || '').toLowerCase() === 'publicado').length;
    }

    if (alunos.status === 'fulfilled') {
      d.tutor_alunos = alunos.value.map(a => ({
        id_estudante: a.idEstudante,
        id_usuario: a.idUsuario,
        nome: a.nome,
        email: a.email,
        curso: a.curso,
        progresso: a.percentualCurso,
        melhor_nota: a.melhorNota,
        tentativas: a.tentativas,
        ultima_atividade: a.status || '—'
      }));

      d.personas.tutor.alunos_ativos = new Set(alunos.value.map(a => a.idEstudante)).size;
    }

    if (chamados.status === 'fulfilled') {
      const abertos = chamados.value.filter(c => c.status === 'Aberto');
      d.tutor_duvidas = abertos.map(c => ({
        id: c.id,
        id_remetente: c.idRemetente,
        aluno: c.remetente,
        iniciais: FMT.iniciais(c.remetente),
        pergunta: c.descricao,
        assunto: c.assunto,
        curso: c.tipo,
        tempo: FMT.relativo(c.dataAbertura),
        status: (c.status || '').toLowerCase()
      }));
      d.personas.tutor.duvidas_pendentes = abertos.length;
    }
  },

  // ---------------- ADMIN ----------------
  async carregarAdmin() {
    const [resumo, pendentes, publicados, usuarios, logs] = await Promise.allSettled([
      API.adminResumo(), API.adminCursos('Pendente'), API.adminCursos('Publicado'),
      API.adminUsuarios(), API.adminLogs(50)
    ]);

    const d = TQ.data;
    const u = API.usuario;

    d.personas = d.personas || {};
    d.personas.admin = Object.assign({}, d.personas.admin, {
      id: u.id, nome: u.nome, email: u.email, tipo: 'admin', iniciais: u.iniciais
    });

    if (resumo.status === 'fulfilled') {
      const r = resumo.value;
      d.admin_kpis = Object.assign({}, d.admin_kpis, {
        usuarios_total: r.totalUsuarios,
        usuarios_ativos_30d: r.usuariosAtivos,
        cursos_publicados: r.cursosPublicados,
        certificados_emitidos: r.certificadosEmitidos,
        matriculas_total: r.totalMatriculas,
        cursos_pendentes: r.cursosPendentes,
        chamados_abertos: r.chamadosAbertos
      });
    }

    if (pendentes.status === 'fulfilled') {
      d.solicitacoes_pendentes = pendentes.value.map(c => ({
        id: c.idCurso,
        tipo: 'curso',
        solicitante: c.tutor,
        iniciais: FMT.iniciais(c.tutor),
        papel: 'tutor',
        titulo: c.nome,
        descricao: c.descricao,
        data: '—',
        prioridade: c.totalAulas === 0 ? 'alta' : 'normal',
        total_aulas: c.totalAulas,
        tem_prova: c.temProva,
        total_questoes: c.totalQuestoes
      }));
    }

    const listaCursos = [];
    [publicados, pendentes].forEach(r => {
      if (r.status === 'fulfilled') {
        r.value.forEach(c => listaCursos.push({
          id: c.idCurso,
          titulo: c.nome,
          tutor: c.tutor,
          alunos: null,
          rating: null,
          status: (c.status || '').toLowerCase(),
          categoria: c.categoria,
          criado_em: '—',
          modulos: c.totalAulas,
          duracao_h: null,
          descricao: c.descricao
        }));
      }
    });
    if (listaCursos.length) d.admin_cursos = listaCursos;

    if (usuarios.status === 'fulfilled') {
      d.admin_usuarios = usuarios.value.map(x => ({
        id: x.id,
        nome: x.nome,
        email: x.email,
        tipo: (x.papel || '').toLowerCase(),
        status: x.ativo ? 'ativo' : 'inativo',
        data_cadastro: FMT.data(x.dataCadastro),
        ultimo_acesso: '—',
        cursos_concluidos: null,
        xp: null
      }));
    }

    if (logs.status === 'fulfilled') {
      d.admin_logs = logs.value.map(l => ({
        id: l.id,
        administrador: l.administrador || ('ADM ' + l.idAdm),
        acao: l.acao,
        data: FMT.dataHora(l.data)
      }));
    }
  }
};

// -------------- 1.2 Formatação --------------
/**
 * O mock.json guardava texto já formatado ("Ontem às 18:30", "15/05/1998").
 * A API devolve data ISO, como deve ser: formatar é trabalho do cliente,
 * que conhece o fuso e o idioma de quem está olhando.
 */
const FMT = {
  data(iso) {
    if (!iso) return '';
    const d = new Date(iso);
    return isNaN(d) ? '' : d.toLocaleDateString('pt-BR');
  },

  dataHora(iso) {
    if (!iso) return '';
    const d = new Date(iso);
    return isNaN(d) ? '' : d.toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' });
  },

  relativo(iso) {
    if (!iso) return '';
    const d = new Date(iso);
    if (isNaN(d)) return '';
    const seg = Math.floor((Date.now() - d.getTime()) / 1000);
    if (seg < 60) return 'agora mesmo';
    const min = Math.floor(seg / 60);
    if (min < 60) return 'há ' + min + (min === 1 ? ' minuto' : ' minutos');
    const h = Math.floor(min / 60);
    if (h < 24) return 'há ' + h + (h === 1 ? ' hora' : ' horas');
    const dias = Math.floor(h / 24);
    if (dias === 1) return 'ontem';
    if (dias < 30) return 'há ' + dias + ' dias';
    const meses = Math.floor(dias / 30);
    if (meses < 12) return 'há ' + meses + (meses === 1 ? ' mês' : ' meses');
    return this.data(iso);
  },

  iniciais(nome) {
    if (!nome) return '??';
    const p = String(nome).trim().split(/\s+/);
    if (p.length === 1) return p[0].charAt(0).toUpperCase();
    return (p[0].charAt(0) + p[p.length - 1].charAt(0)).toUpperCase();
  },

  slug(texto) {
    return String(texto || '')
      .toLowerCase()
      .normalize('NFD').replace(/[\u0300-\u036f]/g, '')
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-|-$/g, '');
  }
};

// -------------- 1.3 Decoração (ícones e cores) --------------
/**
 * Ícone e cor nunca foram dados de negócio: são apresentação. Ficam no
 * mock.json, casados por título, e não no banco. Evita coluna de ícone em
 * uma tabela de curso.
 */
const DECORACAO = {
  doCurso(titulo) {
    const base = (TQ.data && TQ.data.cursos) || [];
    const achado = base.find(c => c.titulo === titulo);
    return achado
      ? { cor_tema: achado.cor_tema, icone: achado.icone, instrutor_cargo: achado.instrutor_cargo }
      : { cor_tema: 'primary', icone: 'ti-book-2', instrutor_cargo: null };
  },

  daMedalha(nome) {
    const base = (TQ.data && TQ.data.medalhas) || [];
    const achado = base.find(m => m.titulo === nome);
    return achado ? achado.icone : 'ti-medal';
  }
};

// -------------- 1.4 Guarda de rota --------------
/**
 * Bloqueia página interna sem token e manda cada papel para a própria área.
 *
 * ATENÇÃO (resposta pronta para a banca): esta verificação é CONVENIÊNCIA DE
 * INTERFACE, não segurança. Quem edita o sessionStorage no DevTools passa por
 * ela. A segurança real está no servidor: sem token válido a API devolve 401,
 * e com o papel errado devolve 403 — como o roteiro de testes demonstra.
 */
function protegerPagina() {
  const path = window.location.pathname;
  const areas = { '/pages/estudante/': 'estudante', '/pages/tutor/': 'tutor', '/pages/admin/': 'admin' };

  const area = Object.keys(areas).find(a => path.includes(a));
  if (!area) return true;

  if (typeof API === 'undefined' || !API.autenticado) {
    window.location.href = TQ.getPath('pages/login.html?exigeLogin=1');
    return false;
  }

  const papel = API.papel;
  if (papel !== areas[area]) {
    window.location.href = TQ.getPath('pages/' + papel + '/home.html');
    return false;
  }

  return true;
}

// -------------- 2. Sidebar do estudante (injeção automática) --------------
function injetarSidebarEstudante() {
  const path = window.location.pathname;
  if (!path.includes('/pages/estudante/')) return;

  const container = document.querySelector('.app-container');
  if (!container) return;
  if (container.querySelector('.sidebar-nav')) return;

  const page = path.split('/').pop();
  const navMap = {
    'home.html': 'home',
    'cursos.html': 'cursos', 'curso.html': 'cursos', 'curso-bd.html': 'cursos',
    'aula.html': 'cursos', 'aula-bd.html': 'cursos',
    'prova.html': 'cursos', 'prova-bd.html': 'cursos',
    'resultado-aprovado.html': 'cursos', 'resultado-reprovado.html': 'cursos',
    'resultado-aprovado-bd.html': 'cursos',
    'conquistas.html': 'conquistas',
    'certificados.html': 'certificados', 'certificado-detalhe.html': 'certificados',
    'perfil.html': 'perfil', 'editar-perfil.html': 'perfil',
    'configuracoes.html': 'perfil', 'alterar-email.html': 'perfil',
    'alterar-senha.html': 'perfil', 'historico.html': 'perfil',
    'notificacoes.html': 'home',
    'ajuda.html': 'perfil', 'suporte.html': 'perfil',
    'termos.html': 'perfil', 'privacidade.html': 'perfil'
  };
  const active = navMap[page] || '';

  const sidebar = document.createElement('aside');
  sidebar.className = 'sidebar-nav';
  sidebar.innerHTML = `
    <div class="sidebar-nav__brand"><i class="ti ti-terminal-2"></i> Tech Quest</div>
    <a href="home.html" class="sidebar-nav__item ${active === 'home' ? 'active' : ''}">
      <i class="ti ti-home"></i> Início
    </a>
    <a href="cursos.html" class="sidebar-nav__item ${active === 'cursos' ? 'active' : ''}">
      <i class="ti ti-book-2"></i> Meus Cursos
    </a>
    <a href="conquistas.html" class="sidebar-nav__item ${active === 'conquistas' ? 'active' : ''}">
      <i class="ti ti-trophy"></i> Conquistas
    </a>
    <a href="certificados.html" class="sidebar-nav__item ${active === 'certificados' ? 'active' : ''}">
      <i class="ti ti-certificate"></i> Certificados
    </a>
    <a href="perfil.html" class="sidebar-nav__item ${active === 'perfil' ? 'active' : ''}">
      <i class="ti ti-user"></i> Perfil
    </a>
    <div style="flex:1;"></div>
    <a href="ajuda.html" class="sidebar-nav__item"><i class="ti ti-help-circle"></i> Central de Ajuda</a>
    <a href="../../index.html" class="sidebar-nav__item" onclick="return confirmarSaida(event)"><i class="ti ti-logout"></i> Sair</a>
  `;

  const wrapper = document.createElement('div');
  wrapper.style.cssText = 'flex:1; display:flex; flex-direction:column; min-width:0;';
  while (container.firstChild) {
    wrapper.appendChild(container.firstChild);
  }
  container.appendChild(sidebar);
  container.appendChild(wrapper);
}

// -------------- 3. Persona switcher --------------
/**
 * No PIM III trocava de perfil livremente, porque o perfil era só um valor no
 * localStorage. Agora o papel vem do token: trocar de perfil exige outro
 * login. O botão continua existindo, mas leva para a tela de login.
 */
function trocarPersona(persona) {
  localStorage.setItem('tq_persona', persona);
  if (typeof API !== 'undefined' && API.autenticado && API.papel === persona) {
    window.location.href = TQ.getPath('pages/' + persona + '/home.html');
    return;
  }
  if (typeof API !== 'undefined') API.limparSessao();
  window.location.href = TQ.getPath('pages/login.html');
}

// -------------- 4. Modais --------------
function abrirModal(id) {
  const m = document.getElementById(id);
  if (m) m.classList.add('active');
}
function fecharModal(id) {
  const m = document.getElementById(id);
  if (m) m.classList.remove('active');
}
document.addEventListener('click', function (e) {
  if (e.target.classList && e.target.classList.contains('modal-overlay')) {
    e.target.classList.remove('active');
  }
});

// -------------- 4.1 Confirmar saída (logout) --------------
function confirmarSaida(e) {
  if (e) e.preventDefault();
  if (!document.getElementById('modalSairGlobal')) {
    const modal = document.createElement('div');
    modal.className = 'modal-overlay';
    modal.id = 'modalSairGlobal';
    modal.innerHTML = `
      <div class="modal">
        <div class="modal__icon modal__icon--warning"><i class="ti ti-logout"></i></div>
        <h3 class="modal__title">Sair da conta?</h3>
        <p class="modal__text">Você precisará fazer login novamente para acessar a plataforma.</p>
        <div class="modal__actions">
          <button class="btn btn--danger btn--full" onclick="fazerLogout()">Sim, sair</button>
          <button class="btn btn--secondary btn--full" onclick="fecharModal('modalSairGlobal')">Cancelar</button>
        </div>
      </div>
    `;
    document.body.appendChild(modal);
  }
  abrirModal('modalSairGlobal');
  return false;
}

function fazerLogout() {
  if (typeof API !== 'undefined') API.limparSessao();
  sessionStorage.clear();
  window.location.href = TQ.getPath('index.html');
}

// -------------- 5. Toggle (switches) --------------
function toggleSwitch(el) {
  el.classList.toggle('active');
}

// -------------- 6. Tabs --------------
function trocarTab(grupo, valor) {
  document.querySelectorAll('[data-tab-group="' + grupo + '"]').forEach(t => t.classList.remove('active'));
  document.querySelectorAll('[data-tab-content="' + grupo + '"]').forEach(c => c.style.display = 'none');
  const tab = document.querySelector('[data-tab-group="' + grupo + '"][data-tab-value="' + valor + '"]');
  const content = document.querySelector('[data-tab-content="' + grupo + '"][data-tab-value="' + valor + '"]');
  if (tab) tab.classList.add('active');
  if (content) content.style.display = '';
}

// -------------- 7. Login e cadastro (REAIS) --------------
/**
 * Antes: as credenciais estavam escritas no JavaScript e o PERFIL era
 * escolhido pelo próprio usuário no localStorage — qualquer pessoa entrava
 * como Admin editando um valor no navegador.
 * Agora: a senha é verificada no servidor contra o hash BCrypt, e o papel vem
 * dentro do token assinado. O cliente não escolhe mais o que ele é.
 */
async function fazerLogin(event) {
  if (event) event.preventDefault();

  const email = (document.getElementById('email') || {}).value || '';
  const senha = (document.getElementById('senha') || {}).value || '';
  const erroBox = document.getElementById('erroLogin');
  const botao = document.querySelector('button[type="submit"]');

  if (erroBox) erroBox.style.display = 'none';
  if (botao) { botao.disabled = true; botao.textContent = 'Entrando...'; }

  try {
    const resposta = await API.login(email.trim(), senha);
    API.salvarSessao(resposta);

    const papel = API.papel || 'estudante';
    localStorage.setItem('tq_persona', papel);
    window.location.href = papel + '/home.html';
  } catch (e) {
    if (erroBox) {
      const texto = erroBox.querySelector('p');
      if (texto) texto.innerHTML = '<strong>' + (e.mensagem || 'Não foi possível entrar.') + '</strong>';
      erroBox.style.display = '';
    } else {
      alert(e.mensagem || 'Não foi possível entrar.');
    }
  } finally {
    if (botao) { botao.disabled = false; botao.textContent = 'Entrar'; }
  }

  return false;
}

/**
 * Cadastro real. Cria sempre conta de Estudante — o papel não é enviado pelo
 * cliente, é fixado no servidor.
 *
 * LEITURA DOS CAMPOS POR TIPO E POSIÇÃO: os inputs do cadastro.html não têm
 * atributo id (só type, class e placeholder), então getElementById devolvia
 * null para todos e o formulário seguia vazio. A leitura usa o que o HTML
 * realmente oferece; se os ids existirem, eles têm prioridade.
 */
async function fazerCadastro(event) {
  if (event) event.preventDefault();

  // O formulário que disparou o submit, ou o primeiro da página.
  const form = (event && event.target && event.target.tagName === 'FORM')
    ? event.target
    : document.querySelector('form');

  if (!form) {
    alert('Formulário de cadastro não encontrado na página.');
    return false;
  }

  // Prioridade 1: id. Prioridade 2: posição por tipo dentro do formulário.
  const porId = id => {
    const el = document.getElementById(id);
    return el ? el.value : null;
  };

  const textos = form.querySelectorAll('input[type="text"]');
  const emails = form.querySelectorAll('input[type="email"]');
  const senhas = form.querySelectorAll('input[type="password"]');
  const aceite = form.querySelector('input[type="checkbox"]');

  const nome = porId('nome') ?? porId('nomeCompleto') ?? (textos[0] ? textos[0].value : '');
  const email = porId('email') ?? (emails[0] ? emails[0].value : '');
  const senha = porId('senha') ?? (senhas[0] ? senhas[0].value : '');
  const confirmar = porId('confirmarSenha') ?? porId('senhaConfirmacao')
                    ?? (senhas[1] ? senhas[1].value : null);

  // Validações no cliente: respondem na hora, sem ida ao servidor.
  // Elas NÃO substituem as do servidor, que valida tudo de novo.
  if (!nome || !nome.trim()) { alert('Informe seu nome.'); return false; }
  if (!email || !email.trim()) { alert('Informe seu e-mail.'); return false; }
  if (!senha || senha.length < 6) {
    alert('A senha deve ter ao menos 6 caracteres.');
    return false;
  }
  if (confirmar !== null && senha !== confirmar) {
    alert('As senhas não coincidem.');
    return false;
  }
  if (aceite && !aceite.checked) {
    alert('É preciso aceitar os termos de uso para continuar.');
    return false;
  }

  const botao = form.querySelector('button[type="submit"]');
  const textoOriginal = botao ? botao.innerHTML : null;
  if (botao) { botao.disabled = true; botao.textContent = 'Criando conta...'; }

  try {
    const resposta = await API.registrar(nome.trim(), email.trim(), senha);
    API.salvarSessao(resposta);
    localStorage.setItem('tq_persona', 'estudante');
    window.location.href = 'estudante/home.html';
  } catch (e) {
    // Mensagem real do servidor: "Já existe uma conta com este e-mail",
    // "A senha deve ter ao menos 6 caracteres", etc.
    alert(e.mensagem || 'Não foi possível criar a conta.');
    if (botao) { botao.disabled = false; botao.innerHTML = textoOriginal; }
  }

  return false;
}

// -------------- 8. Quiz (correção no servidor) --------------
/**
 * No PIM III o questoes.json era baixado pelo navegador COM o campo
 * resposta_correta: o gabarito estava a um DevTools de distância, e a nota era
 * calculada no cliente. Agora a prova chega sem gabarito e a nota é calculada
 * e gravada pelo servidor.
 *
 * A assinatura de QUIZ.iniciar foi preservada — inclusive as chaves antigas
 * 'prova_csharp' e 'prova_bd' — para as páginas de prova não mudarem.
 */
const QUIZ = {
  questaoAtual: 0,
  respostas: {},
  questoes: [],
  idProva: null,
  prova: null,
  rotaAprovado: 'resultado-aprovado.html',
  rotaReprovado: 'resultado-reprovado.html',
  rotaVoltar: 'curso.html',
  enviando: false,

  // Compatibilidade: chave antiga -> trecho do nome do curso na API.
  ALIAS: {
    'prova_csharp': 'C#',
    'prova_bd': 'Banco de Dados'
  },

  iniciar: async function (provaKey, rotaApr, rotaRep, rotaVoltar) {
    if (!TQ.data) await TQ.init();
    if (rotaApr) this.rotaAprovado = rotaApr;
    if (rotaRep) this.rotaReprovado = rotaRep;
    if (rotaVoltar) this.rotaVoltar = rotaVoltar;

    const id = await this.resolverIdProva(provaKey);
    if (!id) {
      this.mostrarErro('Prova não encontrada para este curso.');
      return;
    }

    try {
      this.prova = await API.prova(id);
    } catch (e) {
      this.mostrarErro(e.mensagem || 'Não foi possível carregar a prova.');
      return;
    }

    this.idProva = id;
    this.questoes = (this.prova.questoes || []).map(q => ({
      id: q.id,
      enunciado: q.enunciado,
      codigo: q.codigoExemplo,
      // A API devolve lista; o renderizador espera objeto { A: 'texto' }.
      alternativas: (q.alternativas || []).reduce((acc, a) => {
        acc[a.letra] = a.texto; return acc;
      }, {})
    }));

    this.questaoAtual = 0;
    this.respostas = {};

    const titEl = document.getElementById('quizTitulo');
    const subEl = document.getElementById('quizSubtitulo');
    if (titEl) titEl.textContent = this.prova.titulo;
    if (subEl) subEl.textContent = this.prova.totalQuestoes + ' questões • nota mínima ' + this.prova.notaMinima;

    this.renderizar();
  },

  /** Aceita id numérico, chave antiga ('prova_csharp') ou nome do curso. */
  resolverIdProva: async function (chave) {
    if (typeof chave === 'number') return chave;
    if (/^\d+$/.test(String(chave))) return parseInt(chave, 10);

    const trecho = this.ALIAS[chave] || String(chave || '');
    let cursos = (TQ.data && TQ.data.cursos) || [];
    if (!cursos.length) {
      try { cursos = await API.cursos(); } catch { return null; }
    }

    const curso = cursos.find(c =>
      String(c.titulo || c.nome || '').toLowerCase().includes(trecho.toLowerCase()));
    if (!curso) return null;

    // A lista de cursos não traz o id da prova: busca o detalhe do curso.
    try {
      const detalhe = await API.curso(curso.id);
      return detalhe.idProva || null;
    } catch { return null; }
  },

  mostrarErro: function (mensagem) {
    const box = document.getElementById('quizContent');
    if (box) {
      box.innerHTML = '<div class="card" style="border-color:var(--color-danger);">' +
        '<p class="text-danger"><strong>' + mensagem + '</strong></p></div>';
    } else {
      alert(mensagem);
    }
  },

  renderizar: function () {
    const q = this.questoes[this.questaoAtual];
    if (!q) return;
    const total = this.questoes.length;
    const numero = this.questaoAtual + 1;
    const percentual = Math.round((numero / total) * 100);

    const progress = document.getElementById('quizProgress');
    if (progress) {
      progress.innerHTML =
        '<div class="quiz-header__progress"><span>Questão <strong>' + numero + ' de ' + total + '</strong></span><span class="text-primary"><strong>' + percentual + '%</strong> concluído</span></div>' +
        '<div class="progress"><div class="progress__fill" style="width:' + percentual + '%"></div></div>';
    }

    const box = document.getElementById('quizContent');
    if (!box) return;
    let html = '<div class="quiz-question animate-in">';
    html += '<p class="quiz-question__text">' + q.enunciado + '</p>';
    if (q.codigo) {
      html += '<div class="code-block"><div class="code-block__header"><span>EXEMPLO • C#</span></div>';
      html += '<pre>' + escapeCode(q.codigo) + '</pre></div>';
    }
    html += '<div class="quiz-options">';
    Object.keys(q.alternativas).sort().forEach(letra => {
      const sel = QUIZ.respostas[QUIZ.questaoAtual] === letra ? ' selected' : '';
      html += '<div class="quiz-option' + sel + '" onclick="QUIZ.selecionar(\'' + letra + '\')">';
      html += '<div class="quiz-option__letter">' + letra + '</div>';
      html += '<div class="quiz-option__text">' + q.alternativas[letra] + '</div>';
      html += '<div class="quiz-option__check"><i class="ti ti-check"></i></div>';
      html += '</div>';
    });
    html += '</div></div>';
    box.innerHTML = html;

    const btnAnt = document.getElementById('btnAnterior');
    const btnProx = document.getElementById('btnProxima');
    if (btnAnt) btnAnt.disabled = this.questaoAtual === 0;
    if (btnProx) {
      btnProx.disabled = this.respostas[this.questaoAtual] === undefined;
      btnProx.innerHTML = this.questaoAtual === this.questoes.length - 1
        ? 'Finalizar prova <i class="ti ti-check"></i>'
        : 'Próxima <i class="ti ti-arrow-right"></i>';
    }
  },

  selecionar: function (letra) {
    this.respostas[this.questaoAtual] = letra;
    this.renderizar();
  },

  anterior: function () {
    if (this.questaoAtual > 0) { this.questaoAtual--; this.renderizar(); }
  },

  proxima: function () {
    if (this.respostas[this.questaoAtual] === undefined) return;
    if (this.questaoAtual === this.questoes.length - 1) this.finalizar();
    else { this.questaoAtual++; this.renderizar(); }
  },

  finalizar: async function () {
    if (this.enviando) return;      // evita duas tentativas por clique duplo
    this.enviando = true;

    const btnProx = document.getElementById('btnProxima');
    if (btnProx) { btnProx.disabled = true; btnProx.innerHTML = 'Corrigindo...'; }

    const respostas = this.questoes.map((q, i) => ({
      idQuestao: q.id,
      letra: this.respostas[i]
    })).filter(r => r.letra);

    try {
      const r = await API.submeterProva(this.idProva, respostas);

      // Mesma chave e mesmos campos do PIM III: as telas de resultado
      // continuam funcionando sem alteração.
      sessionStorage.setItem('tq_resultado', JSON.stringify({
        acertos: r.acertos,
        total: r.totalQuestoes,
        nota: r.nota.toFixed(1),
        aprovado: r.aprovado,
        prova: this.idProva,
        nota_minima: r.notaMinima,
        tentativa: r.numeroTentativa,
        xp_ganho: r.xpGanho,
        medalhas_novas: r.medalhasNovas || [],
        correcao: r.correcao
      }));

      window.location.href = r.aprovado ? this.rotaAprovado : this.rotaReprovado;
    } catch (e) {
      this.enviando = false;
      if (btnProx) { btnProx.disabled = false; btnProx.innerHTML = 'Finalizar prova <i class="ti ti-check"></i>'; }
      alert(e.mensagem || 'Não foi possível enviar a prova. Tente novamente.');
    }
  }
};

function escapeCode(code) {
  let h = code.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
  h = h.replace(/("[^"]*")/g, '<span class="string">$1</span>');
  h = h.replace(/(\/\/[^\n]*)/g, '<span class="comment">$1</span>');
  h = h.replace(/\b(\d+)\b/g, '<span class="number">$1</span>');
  const kw = ['List', 'int', 'string', 'foreach', 'in', 'if', 'else', 'new', 'var', 'bool', 'true', 'false', 'public', 'class', 'static', 'void', 'return', 'Console', 'WriteLine', 'SELECT', 'FROM', 'WHERE', 'JOIN', 'INSERT', 'UPDATE', 'DELETE'];
  kw.forEach(k => {
    const re = new RegExp('\\b' + k + '\\b', 'g');
    h = h.replace(re, '<span class="keyword">' + k + '</span>');
  });
  return h;
}

// -------------- 9. Renderizadores específicos --------------
function renderEstudanteHome() {
  if (!TQ.data || !TQ.data.personas) return;
  const e = TQ.data.personas.estudante;
  if (!e) return;
  const setText = (id, val) => { const el = document.getElementById(id); if (el) el.textContent = val; };

  setText('userName', (e.nome || '').split(' ')[0]);
  setText('userInitials', e.iniciais);
  setText('userLevel', e.nivel);
  setText('userXP', (e.xp || 0).toLocaleString('pt-BR'));
  setText('userXPNext', (e.xp_proximo_nivel || 0).toLocaleString('pt-BR'));
  setText('userQuote', e.frase_motivacional);
  const bar = document.getElementById('userProgressBar');
  if (bar) bar.style.width = (e.progresso_nivel || 0) + '%';
  const barLabel = document.getElementById('userProgressLabel');
  if (barLabel) barLabel.textContent = (e.progresso_nivel || 0) + '%';

  const conqList = document.getElementById('conquistasList');
  if (conqList) {
    const lista = TQ.data.conquistas_recentes || [];
    conqList.innerHTML = lista.length
      ? lista.map(c =>
          '<div class="list-item">' +
            '<div class="list-item__icon"><i class="ti ' + c.icone + '"></i></div>' +
            '<div class="list-item__body">' +
              '<div class="list-item__title">' + c.titulo + '</div>' +
              '<div class="list-item__subtitle">' + c.tempo + '</div>' +
            '</div>' +
            '<div class="text-warning font-semibold text-sm">+' + c.xp + ' XP</div>' +
          '</div>'
        ).join('')
      : '<p class="text-sm text-muted">Conclua sua primeira aula para desbloquear conquistas.</p>';
  }
}

// -------------- 9.1 Aviso de API fora do ar --------------
function avisarApiIndisponivel() {
  if (!TQ.erroApi) return;
  const alvo = document.querySelector('.main-content') || document.body;
  const aviso = document.createElement('div');
  aviso.className = 'card mb-md';
  aviso.style.cssText = 'background:var(--color-warning-light); border-color:var(--color-warning);';
  aviso.innerHTML = '<div class="flex items-start gap-sm">' +
    '<i class="ti ti-cloud-off text-warning" style="font-size:20px;"></i>' +
    '<p class="text-sm" style="margin:0;"><strong>Dados podem estar desatualizados.</strong> ' +
    (TQ.erroApi.mensagem || 'Falha ao consultar o servidor.') + '</p></div>';
  alvo.insertBefore(aviso, alvo.firstChild);
}

// -------------- 9.2 Contexto da página --------------
/**
 * No PIM III cada curso tinha a sua própria página: curso.html era "Fundamentos
 * de C#" e curso-bd.html era "Banco de Dados", com as aulas escritas à mão no
 * HTML. Isso não escala — um curso novo exigiria criar arquivos novos.
 *
 * Na integração as páginas passaram a ser PARAMETRIZADAS: o curso vem de
 * ?id= na URL. O nome do arquivo continua funcionando como padrão, para que
 * os links antigos e os favoritos não quebrem.
 */
const CONTEXTO = {
  // Nome do arquivo -> trecho do nome do curso, para quando não há ?id=.
  PADRAO: {
    'curso.html': 'C#', 'aula.html': 'C#', 'prova.html': 'C#',
    'resultado-aprovado.html': 'C#', 'resultado-reprovado.html': 'C#',
    'curso-bd.html': 'Banco de Dados', 'aula-bd.html': 'Banco de Dados',
    'prova-bd.html': 'Banco de Dados', 'resultado-aprovado-bd.html': 'Banco de Dados'
  },

  parametro(nome) {
    const valor = new URLSearchParams(window.location.search).get(nome);
    return valor && /^\d+$/.test(valor) ? parseInt(valor, 10) : null;
  },

  pagina() { return window.location.pathname.split('/').pop(); },

  /** Curso completo (com materiais) da página atual, ou null. */
  async curso() {
    const id = this.parametro('id');
    if (id) return API.curso(id);

    const trecho = this.PADRAO[this.pagina()];
    if (!trecho) return null;

    const lista = await API.cursos();
    const achado = lista.find(c => (c.nome || '').toLowerCase().includes(trecho.toLowerCase()));
    return achado ? API.curso(achado.id) : null;
  }
};

// -------------- 9.3 Avisos na tela --------------
/**
 * Substitui os alert() que o PIM III usava para simular ações. Um aviso que
 * aparece na própria página não interrompe o fluxo e permite distinguir
 * sucesso de erro pela cor.
 */
function avisar(mensagem, tipo) {
  document.querySelectorAll('.tq-aviso').forEach(a => a.remove());

  const cores = {
    sucesso: ['--color-success-light', '--color-success', 'ti-circle-check'],
    erro: ['--color-danger-light', '--color-danger', 'ti-alert-triangle'],
    info: ['--color-warning-light', '--color-warning', 'ti-info-circle']
  };
  const [fundo, borda, icone] = cores[tipo || 'info'];

  const caixa = document.createElement('div');
  caixa.className = 'card tq-aviso animate-in';
  caixa.style.cssText = `position:fixed; left:50%; transform:translateX(-50%); top:16px;
    z-index:9999; max-width:460px; width:calc(100% - 32px);
    background:var(${fundo}); border-color:var(${borda}); box-shadow:0 8px 24px rgba(0,0,0,.15);`;
  caixa.innerHTML = `<div class="flex items-start gap-sm">
      <i class="ti ${icone}" style="font-size:20px; color:var(${borda});"></i>
      <p class="text-sm" style="margin:0; flex:1;">${mensagem}</p>
    </div>`;

  document.body.appendChild(caixa);
  setTimeout(() => caixa.remove(), 5000);
}

/** Desabilita o botão durante a chamada, evitando duplo envio. */
async function comBotaoOcupado(botao, textoOcupado, acao) {
  if (!botao) return acao();
  const original = botao.innerHTML;
  botao.disabled = true;
  botao.innerHTML = textoOcupado;
  try {
    return await acao();
  } finally {
    botao.disabled = false;
    botao.innerHTML = original;
  }
}

// -------------- 9.4 Ações do estudante --------------

/**
 * Renderiza a página de um curso: cabeçalho, progresso e a lista real de
 * aulas vinda do banco, com o estado de cada uma para este aluno.
 */
async function renderCursoDetalhe() {
  const alvo = document.getElementById('listaAulas');
  if (!alvo) return;

  let curso;
  try {
    curso = await CONTEXTO.curso();
  } catch (e) {
    avisar(e.mensagem || 'Não foi possível carregar o curso.', 'erro');
    return;
  }
  if (!curso) { avisar('Curso não encontrado.', 'erro'); return; }

  TQ.cursoAtual = curso;

  const texto = (id, valor) => { const el = document.getElementById(id); if (el) el.textContent = valor; };
  texto('cursoTitulo', curso.nome);
  texto('cursoDescricao', curso.descricao || '');
  texto('cursoCategoria', curso.categoria || '');
  texto('cursoNivel', curso.nivel || '');
  texto('cursoInstrutor', curso.instrutor || 'A definir');
  texto('cursoDuracao', (curso.duracaoHoras || 0) + ' horas');
  texto('cursoDificuldade', curso.nivel || '');
  const iniciais = document.getElementById('cursoInstrutorIniciais');
  if (iniciais) iniciais.textContent = curso.instrutorIniciais || FMT.iniciais(curso.instrutor);

  const aulas = curso.materiais || [];
  const concluidas = aulas.filter(a => a.concluido).length;
  const percentual = aulas.length ? Math.round(concluidas * 100 / aulas.length) : 0;

  texto('cursoProgressoLabel', percentual + '%');
  const barra = document.getElementById('cursoProgressoBarra');
  if (barra) barra.style.width = percentual + '%';
  texto('cursoAulasResumo', concluidas + ' de ' + aulas.length + ' aulas concluídas');

  if (aulas.length === 0) {
    alvo.innerHTML = '<div class="card"><p class="text-sm text-muted">Este curso ainda não tem aulas publicadas.</p></div>';
    return;
  }

  // A primeira aula não concluída é a "atual": é para onde o botão principal
  // leva, para o aluno não precisar procurar onde parou.
  const proxima = aulas.find(a => !a.concluido) || aulas[aulas.length - 1];
  const paginaAula = CONTEXTO.pagina().includes('-bd') ? 'aula-bd.html' : 'aula.html';

  alvo.innerHTML = aulas.map((a, i) => {
    const atual = a.id === proxima.id && !a.concluido;
    const estado = a.concluido ? 'done' : (atual ? 'active' : 'locked');
    const classe = a.concluido ? '' : (atual ? 'module-item--active' : 'module-item--locked');
    const icone = a.concluido ? 'ti-check' : (atual ? 'ti-player-play' : 'ti-lock');
    const selo = a.concluido
      ? '<span class="badge badge--success">CONCLUÍDO</span>'
      : (atual
          ? `<a href="${paginaAula}?id=${curso.id}&aula=${a.id}" class="badge badge--primary">ASSISTIR</a>`
          : '<span class="badge badge--neutral">A SEGUIR</span>');

    return `
    <div class="module-item ${classe} animate-in">
      <div class="module-item__icon module-item__icon--${estado}"><i class="ti ${icone}"></i></div>
      ${i < aulas.length - 1 ? '<div class="module-item__line"></div>' : ''}
      <div class="module-item__body">
        <div class="module-item__card">
          <div class="flex justify-between items-center">
            <span class="module-item__label ${atual ? 'text-primary' : ''}">AULA ${i + 1}${atual ? ' • ATUAL' : ''}</span>
            ${selo}
          </div>
          <h4 class="module-item__title">${a.titulo || 'Aula sem título'}</h4>
          <p class="module-item__text">Tipo: ${a.tipo || 'material'}</p>
          ${a.porcentagemAssistida > 0 && !a.concluido
            ? `<div class="progress mt-sm"><div class="progress__fill" style="width:${a.porcentagemAssistida}%"></div></div>`
            : ''}
        </div>
      </div>
    </div>`;
  }).join('');

  // O botão da prova só existe se o curso tiver uma, e só libera quando
  // todas as aulas estiverem concluídas — a mesma regra que o servidor
  // aplica para emitir o certificado.
  const areaProva = document.getElementById('areaProva');
  if (areaProva && curso.idProva) {
    const liberada = concluidas === aulas.length;
    const paginaProva = CONTEXTO.pagina().includes('-bd') ? 'prova-bd.html' : 'prova.html';
    areaProva.innerHTML = liberada
      ? `<a href="${paginaProva}?id=${curso.id}" class="btn btn--primary btn--full btn--lg">
           Fazer a prova <i class="ti ti-file-text"></i></a>`
      : `<button class="btn btn--secondary btn--full btn--lg" disabled>
           Conclua todas as aulas para liberar a prova</button>`;
  }
}

/** Matrícula no curso da página. Idempotente: repetir não duplica. */
async function matricularNoCurso(botao) {
  const curso = TQ.cursoAtual;
  if (!curso) { avisar('Curso não carregado.', 'erro'); return; }

  await comBotaoOcupado(botao, 'Matriculando...', async () => {
    try {
      await API.matricular(curso.id);
      avisar('Matrícula confirmada em <strong>' + curso.nome + '</strong>.', 'sucesso');
      await renderCursoDetalhe();
    } catch (e) {
      avisar(e.mensagem || 'Não foi possível matricular.', 'erro');
    }
  });
}

/**
 * Marca a aula como concluída. A resposta já traz XP, nível, percentual do
 * curso e medalhas novas — tudo numa chamada só, para a tela não precisar
 * consultar o servidor de novo.
 */
async function concluirAula(botao) {
  const idAula = CONTEXTO.parametro('aula');
  const idCurso = CONTEXTO.parametro('id');

  let alvo = idAula;
  if (!alvo) {
    // Sem ?aula= na URL: assume a primeira aula não concluída do curso.
    try {
      const curso = TQ.cursoAtual || await CONTEXTO.curso();
      const pendente = (curso.materiais || []).find(a => !a.concluido);
      alvo = pendente ? pendente.id : null;
    } catch (e) { /* tratado abaixo */ }
  }

  if (!alvo) { avisar('Não foi possível identificar a aula.', 'erro'); return; }

  await comBotaoOcupado(botao, 'Registrando...', async () => {
    try {
      const r = await API.progressoAula(alvo, true, 100);

      let msg = `Aula concluída. <strong>${r.aulasConcluidas} de ${r.totalAulas}</strong> ` +
                `(${r.percentualCurso}%) — você está no nível ${r.nivel} com ${r.xpAtual} XP.`;
      if (r.medalhasNovas && r.medalhasNovas.length) {
        msg += '<br>Nova conquista: <strong>' + r.medalhasNovas.join(', ') + '</strong>.';
      }
      if (r.cursoConcluido) {
        msg += '<br>Curso concluído — seu certificado já foi emitido.';
      }
      avisar(msg, 'sucesso');

      fecharModal('modalConcluir');
      setTimeout(() => {
        const destino = CONTEXTO.pagina().includes('-bd') ? 'curso-bd.html' : 'curso.html';
        window.location.href = destino + (idCurso ? '?id=' + idCurso : '');
      }, 2200);
    } catch (e) {
      avisar(e.mensagem || 'Não foi possível registrar a aula.', 'erro');
    }
  });
}

/** Salva telefone, data de nascimento e cidade do próprio usuário. */
async function salvarPerfilEstudante(botao) {
  const valor = id => { const el = document.getElementById(id); return el ? el.value.trim() : ''; };

  // A API espera data ISO (aaaa-mm-dd); a tela usa o formato brasileiro.
  let nascimento = null;
  const digitado = valor('perfilNascimento');
  if (digitado) {
    const br = digitado.match(/^(\d{2})\/(\d{2})\/(\d{4})$/);
    const iso = digitado.match(/^\d{4}-\d{2}-\d{2}$/);
    if (br) nascimento = `${br[3]}-${br[2]}-${br[1]}`;
    else if (iso) nascimento = digitado;
    else { avisar('Data de nascimento inválida. Use dd/mm/aaaa.', 'erro'); return; }
  }

  await comBotaoOcupado(botao, 'Salvando...', async () => {
    try {
      await API.salvarPerfil({
        telefone: valor('perfilTelefone') || null,
        dataNascimento: nascimento,
        cidade: valor('perfilCidade') || null
      });
      avisar('Perfil atualizado.', 'sucesso');
      setTimeout(() => { window.location.href = 'perfil.html'; }, 1500);
    } catch (e) {
      avisar(e.mensagem || 'Não foi possível salvar o perfil.', 'erro');
    }
  });
}

/** Preenche o formulário de edição com os dados reais do usuário. */
async function carregarPerfilParaEdicao() {
  try {
    const p = await API.perfil();
    const preencher = (id, v) => { const el = document.getElementById(id); if (el && v != null) el.value = v; };
    preencher('perfilNome', p.nome);
    preencher('perfilEmail', p.email);
    preencher('perfilTelefone', p.telefone || '');
    preencher('perfilCidade', p.cidade || '');
    preencher('perfilNascimento', p.dataNascimento ? FMT.data(p.dataNascimento) : '');
  } catch (e) {
    avisar(e.mensagem || 'Não foi possível carregar o perfil.', 'erro');
  }
}

/**
 * Abre um chamado para o tutor. No modelo do PIM III, Chamado é a tabela que
 * registra dúvidas e solicitações — não foi preciso criar nada novo.
 */
async function enviarChamado(botao) {
  const valor = id => { const el = document.getElementById(id); return el ? el.value.trim() : ''; };
  const assunto = valor('chamadoAssunto');
  const descricao = valor('chamadoDescricao');
  const tipo = valor('chamadoTipo') || 'Duvida';

  if (!assunto) { avisar('Informe o assunto.', 'erro'); return; }
  if (!descricao) { avisar('Descreva sua dúvida.', 'erro'); return; }

  await comBotaoOcupado(botao, 'Enviando...', async () => {
    try {
      const c = await API.abrirChamado(tipo, assunto, descricao, null);
      avisar('Chamado aberto com o número <strong>#' + c.id + '</strong>.', 'sucesso');
      ['chamadoAssunto', 'chamadoDescricao'].forEach(id => {
        const el = document.getElementById(id); if (el) el.value = '';
      });
    } catch (e) {
      avisar(e.mensagem || 'Não foi possível abrir o chamado.', 'erro');
    }
  });
}

// -------------- 9.5 Ações do tutor --------------

/**
 * Cria um curso. Ele nasce como Rascunho: publicar depende de submeter para
 * avaliação e de um administrador aprovar — a máquina de estados vive no
 * servidor, e a tela apenas a percorre.
 */
async function criarCurso(botao) {
  const form = document.querySelector('form');
  if (!form) { avisar('Formulário não encontrado.', 'erro'); return; }

  const v = id => { const el = document.getElementById(id); return el ? el.value.trim() : ''; };
  const nome = v('cursoNome');
  const descricao = v('cursoDescricaoNovo');
  const categoria = v('cursoCategoriaNovo');
  const nivel = v('cursoNivelNovo');
  const horas = parseInt(v('cursoHoras'), 10);

  if (!nome) { avisar('Informe o nome do curso.', 'erro'); return; }

  await comBotaoOcupado(botao, 'Criando...', async () => {
    try {
      const curso = await API.tutorCriarCurso({
        nome,
        descricao: descricao || null,
        categoria: categoria || null,
        nivel: nivel || null,
        duracaoHoras: isNaN(horas) ? null : horas
      });
      avisar('Curso <strong>' + curso.nome + '</strong> criado como rascunho. ' +
             'Adicione as aulas e submeta para avaliação.', 'sucesso');
      setTimeout(() => { window.location.href = 'editar-curso.html?id=' + curso.id; }, 1800);
    } catch (e) {
      avisar(e.mensagem || 'Não foi possível criar o curso.', 'erro');
    }
  });
}

/** Envia o curso para avaliação do administrador (Rascunho -> Pendente). */
async function submeterCurso(idCurso, botao) {
  await comBotaoOcupado(botao, 'Enviando...', async () => {
    try {
      await API.tutorSubmeterCurso(idCurso);
      avisar('Curso enviado para avaliação. Você será avisado pelo painel de dúvidas.', 'sucesso');
      setTimeout(() => window.location.reload(), 1600);
    } catch (e) {
      // O servidor recusa curso sem aula e curso que já está publicado;
      // a mensagem dele é mais precisa do que qualquer texto fixo aqui.
      avisar(e.mensagem || 'Não foi possível submeter o curso.', 'erro');
    }
  });
}

/** Adiciona uma aula ao curso em edição. */
async function adicionarAula(idCurso, botao) {
  const v = id => { const el = document.getElementById(id); return el ? el.value.trim() : ''; };
  const titulo = v('aulaTitulo');
  const tipo = v('aulaTipo') || 'video';

  if (!titulo) { avisar('Informe o título da aula.', 'erro'); return; }

  await comBotaoOcupado(botao, 'Adicionando...', async () => {
    try {
      await API.tutorAdicionarAula(idCurso, titulo, tipo);
      avisar('Aula adicionada.', 'sucesso');
      const campo = document.getElementById('aulaTitulo');
      if (campo) campo.value = '';
      if (typeof carregarCursoDoTutor === 'function') carregarCursoDoTutor();
    } catch (e) {
      avisar(e.mensagem || 'Não foi possível adicionar a aula.', 'erro');
    }
  });
}

/**
 * Responde a uma dúvida do aluno. O modelo não tem tabela de mensagens
 * encadeadas: a resposta é um chamado novo endereçado a quem perguntou, e o
 * chamado original é fechado. É reúso da tabela Chamado, não gambiarra.
 */
async function responderDuvida(idChamado, idRemetente, botao) {
  const campo = document.getElementById('respostaDuvida');
  const texto = campo ? campo.value.trim() : '';
  if (!texto) { avisar('Escreva a resposta antes de enviar.', 'erro'); return; }

  await comBotaoOcupado(botao, 'Enviando...', async () => {
    try {
      await API.abrirChamado('Resposta', 'Resposta do tutor', texto, idRemetente);
      await API.fecharChamado(idChamado);
      avisar('Resposta enviada e dúvida encerrada.', 'sucesso');
      fecharModal('modalResponder');
      setTimeout(() => window.location.reload(), 1500);
    } catch (e) {
      avisar(e.mensagem || 'Não foi possível enviar a resposta.', 'erro');
    }
  });
}

/** Marca a dúvida como respondida sem enviar mensagem. */
async function encerrarDuvida(idChamado, botao) {
  await comBotaoOcupado(botao, 'Encerrando...', async () => {
    try {
      await API.fecharChamado(idChamado);
      avisar('Dúvida marcada como respondida.', 'sucesso');
      setTimeout(() => window.location.reload(), 1200);
    } catch (e) {
      avisar(e.mensagem || 'Não foi possível encerrar a dúvida.', 'erro');
    }
  });
}

// -------------- 9.6 Ações do administrador --------------

async function aprovarCurso(idCurso, botao) {
  await comBotaoOcupado(botao, 'Aprovando...', async () => {
    try {
      await API.adminAprovar(idCurso);
      avisar('Curso aprovado e publicado. Já aparece para os estudantes.', 'sucesso');
      setTimeout(() => window.location.reload(), 1500);
    } catch (e) {
      avisar(e.mensagem || 'Não foi possível aprovar o curso.', 'erro');
    }
  });
}

/**
 * Rejeita o curso. O motivo vira um chamado para o tutor — a tabela Curso não
 * tem campo de parecer, e Chamado já é o canal de comunicação do modelo.
 */
async function rejeitarCurso(idCurso, botao) {
  const campo = document.getElementById('motivoRejeicao');
  const motivo = campo ? campo.value.trim() : '';

  await comBotaoOcupado(botao, 'Rejeitando...', async () => {
    try {
      await API.adminRejeitar(idCurso, motivo || null);
      avisar('Curso rejeitado. O tutor recebeu o motivo na caixa de dúvidas.', 'sucesso');
      setTimeout(() => window.location.reload(), 1500);
    } catch (e) {
      avisar(e.mensagem || 'Não foi possível rejeitar o curso.', 'erro');
    }
  });
}

/** Cria usuário com papel definido pelo administrador. */
async function criarUsuarioAdmin(botao) {
  const v = id => { const el = document.getElementById(id); return el ? el.value.trim() : ''; };
  const nome = v('novoNome');
  const email = v('novoEmail');
  const senha = v('novaSenha');
  const papel = v('novoPapel') || 'Tutor';

  if (!nome) { avisar('Informe o nome.', 'erro'); return; }
  if (!email) { avisar('Informe o e-mail.', 'erro'); return; }
  if (senha.length < 6) { avisar('A senha deve ter ao menos 6 caracteres.', 'erro'); return; }

  await comBotaoOcupado(botao, 'Criando...', async () => {
    try {
      const u = await API.adminCriarUsuario({ nome, email, senha, papel });
      avisar('Usuário <strong>' + u.nome + '</strong> criado como ' + u.papel + '.', 'sucesso');
      setTimeout(() => { window.location.href = 'usuarios.html'; }, 1800);
    } catch (e) {
      avisar(e.mensagem || 'Não foi possível criar o usuário.', 'erro');
    }
  });
}

/**
 * Ativa ou desativa uma conta.
 *
 * É esta ação que dispara a trigger TRG_Auditoria_StatusUsuario no banco: a
 * linha em Log_Auditoria nasce do próprio UPDATE, sem a API inserir nada.
 */
async function alterarStatusUsuario(idUsuario, ativar, botao) {
  await comBotaoOcupado(botao, ativar ? 'Ativando...' : 'Desativando...', async () => {
    try {
      await API.adminAlterarStatus(idUsuario, ativar);
      avisar('Usuário ' + (ativar ? 'ativado' : 'desativado') +
             '. O registro foi gravado no log de auditoria pelo banco.', 'sucesso');
      setTimeout(() => window.location.reload(), 1500);
    } catch (e) {
      avisar(e.mensagem || 'Não foi possível alterar o status.', 'erro');
    }
  });
}

// -------------- 9.7 Telas de leitura do estudante --------------

/**
 * Perfil do estudante.
 *
 * No PIM III esta tela trazia "Felipe Almeida" e "Nivel 7 - 2.450 XP" escritos
 * no HTML: qualquer pessoa que entrasse via o perfil do Felipe. Agora tudo vem
 * de /api/perfil, para o usuario logado.
 */
async function carregarPerfilEstudante() {
  const texto = (id, v) => { const el = document.getElementById(id); if (el) el.textContent = v; };

  let p;
  try {
    p = await API.perfil();
  } catch (e) {
    avisar(e.mensagem || 'Nao foi possivel carregar o perfil.', 'erro');
    return;
  }

  texto('perfilIniciais', p.iniciais);
  texto('perfilNomeTopo', p.nome);
  texto('perfilEmailTopo', p.email || '');
  texto('perfilNivelXp', `Nivel ${p.nivel} - ${p.xp.toLocaleString('pt-BR')} XP`);

  texto('statProvas', p.provasAprovadas);
  texto('statMedalhas', p.medalhas);
  texto('statCertificados', p.cursosConcluidos);
  texto('statAulas', p.aulasConcluidas);

  texto('perfilNomeDados', p.nome);
  texto('perfilEmailDados', p.email || '-');
  texto('perfilTelefoneDados', p.telefone || 'nao informado');
  texto('perfilNascimentoDados', p.dataNascimento ? FMT.data(p.dataNascimento) : 'nao informada');
  texto('perfilCidadeDados', p.cidade || 'nao informada');
}

/**
 * Historico de provas do aluno, gerado pela PROCEDURE do banco
 * (SP_RelatorioDesempenhoEstudante) e nao por consulta montada na API.
 */
async function carregarMinhasProvas() {
  const lista = document.getElementById('provasList');
  if (!lista) return;

  let linhas;
  try {
    linhas = await API.desempenho();
  } catch (e) {
    lista.innerHTML = '<div class="card"><p class="text-sm text-danger">' +
      (e.mensagem || 'Nao foi possivel carregar suas provas.') + '</p></div>';
    return;
  }

  if (!linhas.length) {
    lista.innerHTML = '<div class="card"><p class="text-sm text-muted">' +
      'Voce ainda nao realizou nenhuma prova.</p></div>';
    return;
  }

  lista.innerHTML = linhas.map(l => {
    // A nota minima nao vem na procedure; 7,0 e o padrao do modelo.
    const aprovado = (l.nota || 0) >= 7;
    return `
    <div class="card" data-situacao="${aprovado ? 'aprovado' : 'reprovado'}"
         style="border-left:3px solid var(--color-${aprovado ? 'success' : 'danger'});">
      <div class="flex justify-between items-center mb-sm">
        <strong>${l.prova || 'Prova'}</strong>
        <span class="badge badge--${aprovado ? 'success' : 'danger'}">
          ${aprovado ? 'APROVADO' : 'REPROVADO'}
        </span>
      </div>
      <div class="flex justify-between text-sm text-secondary">
        <span>Nota <strong class="text-${aprovado ? 'success' : 'danger'}">
          ${l.nota != null ? l.nota.toFixed(1) : '-'}</strong></span>
        <span>Tentativa ${l.tentativas}</span>
        <span>${FMT.data(l.dataRealizacao)}</span>
      </div>
    </div>`;
  }).join('');
}

/** Aulas dos cursos em que o aluno esta matriculado, com o estado de cada uma. */
async function carregarMeusMateriais() {
  const lista = document.getElementById('materiaisList');
  if (!lista) return;

  try {
    const historico = await API.historico();
    if (!historico.length) {
      lista.innerHTML = '<div class="card"><p class="text-sm text-muted">' +
        'Voce ainda nao esta matriculado em nenhum curso.</p></div>';
      return;
    }

    // Uma chamada por curso matriculado: sao poucos, e cada uma ja traz as
    // aulas com o progresso deste aluno.
    const cursos = await Promise.all(historico.map(h => API.curso(h.idCurso)));

    const blocos = cursos.filter(Boolean).map(c => {
      const aulas = c.materiais || [];
      const paginaAula = (c.nome || '').toLowerCase().includes('banco de dados')
        ? 'aula-bd.html' : 'aula.html';

      const itens = aulas.length
        ? aulas.map(a => `
          <a href="${paginaAula}?id=${c.id}&aula=${a.id}" class="list-item"
             data-busca="${(a.titulo || '').toLowerCase()}" data-concluido="${a.concluido}">
            <div class="list-item__icon list-item__icon--${a.concluido ? 'success' : 'neutral'}">
              <i class="ti ti-${a.concluido ? 'circle-check' : 'player-play'}"></i>
            </div>
            <div class="list-item__body">
              <div class="list-item__title">${a.titulo || 'Aula'}</div>
              <div class="list-item__subtitle">${c.nome} - ${a.tipo || 'material'}</div>
            </div>
            ${a.concluido ? '<span class="badge badge--success">OK</span>' : ''}
          </a>`).join('')
        : '<p class="text-sm text-muted">Sem aulas publicadas.</p>';

      return `<h3 class="section-title mt-lg">${c.nome}</h3>${itens}`;
    });

    lista.innerHTML = blocos.join('');

    // Busca local: os dados ja estao na tela, nao vale ida ao servidor.
    const busca = document.getElementById('buscaMaterial');
    if (busca) {
      busca.addEventListener('input', () => {
        const termo = busca.value.trim().toLowerCase();
        lista.querySelectorAll('[data-busca]').forEach(el => {
          el.style.display = !termo || el.dataset.busca.includes(termo) ? '' : 'none';
        });
      });
    }
  } catch (e) {
    lista.innerHTML = '<div class="card"><p class="text-sm text-danger">' +
      (e.mensagem || 'Nao foi possivel carregar seus materiais.') + '</p></div>';
  }
}

/** Chamados abertos pelo proprio usuario, para a tela de suporte. */
async function carregarMeusChamados() {
  const alvo = document.getElementById('meusChamados');
  if (!alvo) return;

  try {
    const chamados = await API.chamados();
    if (!chamados.length) {
      alvo.innerHTML = '<p class="text-sm text-muted">Nenhum chamado registrado.</p>';
      return;
    }
    alvo.innerHTML = chamados.slice(0, 8).map(c => {
      const aberto = c.status === 'Aberto';
      return `
      <div class="list-item">
        <div class="list-item__icon list-item__icon--${aberto ? 'warning' : 'success'}">
          <i class="ti ti-${aberto ? 'clock' : 'circle-check'}"></i>
        </div>
        <div class="list-item__body">
          <div class="list-item__title">${c.assunto || c.tipo || 'Chamado'}</div>
          <div class="list-item__subtitle">
            Protocolo #${c.id} - ${aberto ? 'em analise' : 'resolvido'} - ${FMT.relativo(c.dataAbertura)}
          </div>
        </div>
      </div>`;
    }).join('');
  } catch (e) {
    alvo.innerHTML = '<p class="text-sm text-danger">' +
      (e.mensagem || 'Nao foi possivel carregar seus chamados.') + '</p>';
  }
}

// -------------- 9.8 Telas de detalhe do tutor e do admin --------------

/**
 * Envia uma mensagem direta a um aluno. Reusa a tabela Chamado, que e o canal
 * de comunicacao do modelo — nao existe tabela de mensagens.
 */
async function enviarMensagemAluno(idUsuarioAluno, botao) {
  const campo = document.getElementById('mensagemAluno');
  const texto = campo ? campo.value.trim() : '';
  if (!texto) { avisar('Escreva a mensagem antes de enviar.', 'erro'); return; }
  if (!idUsuarioAluno) { avisar('Aluno nao identificado.', 'erro'); return; }

  await comBotaoOcupado(botao, 'Enviando...', async () => {
    try {
      await API.abrirChamado('Mensagem', 'Mensagem do tutor', texto, idUsuarioAluno);
      avisar('Mensagem enviada ao aluno.', 'sucesso');
      if (campo) campo.value = '';
      fecharModal('modalMensagem');
    } catch (e) {
      avisar(e.mensagem || 'Nao foi possivel enviar a mensagem.', 'erro');
    }
  });
}

/**
 * Painel do administrador com os numeros reais da plataforma.
 *
 * Os graficos de serie temporal do PIM III (engajamento diario, receita
 * mensal) nao tem lastro no modelo: nao ha tabela de acesso diario nem de
 * pagamento. Em vez de exibir numero inventado como se fosse medicao, a tela
 * mostra o que o banco sabe e marca o restante como ilustrativo.
 */
async function carregarPainelAdmin() {
  const texto = (id, v) => { const el = document.getElementById(id); if (el) el.textContent = v; };
  try {
    const r = await API.adminResumo();
    texto('kpiUsuarios', r.totalUsuarios);
    texto('kpiAtivos', r.usuariosAtivos);
    texto('kpiEstudantes', r.estudantes);
    texto('kpiTutores', r.tutores);
    texto('kpiCursosPublicados', r.cursosPublicados);
    texto('kpiCursosPendentes', r.cursosPendentes);
    texto('kpiMatriculas', r.totalMatriculas);
    texto('kpiCertificados', r.certificadosEmitidos);
    texto('kpiChamados', r.chamadosAbertos);
  } catch (e) {
    avisar(e.mensagem || 'Nao foi possivel carregar o painel.', 'erro');
  }
}

// -------------- 9.9 Conta do proprio usuario --------------
/**
 * As tres operacoes abaixo exigem a senha atual, mesmo com o usuario ja
 * autenticado. Nao e burocracia: um computador de laboratorio com a sessao
 * aberta nao pode servir para trocar a senha, transferir a conta pelo e-mail
 * ou encerra-la. A validacao real acontece no servidor; a daqui so evita uma
 * ida inutil.
 */

async function trocarSenha(botao) {
  const v = id => { const el = document.getElementById(id); return el ? el.value : ''; };
  const atual = v('senhaAtual');
  const nova = v('novaSenha');
  const confirmar = v('confirmarSenha');

  if (!atual) { avisar('Informe sua senha atual.', 'erro'); return; }
  if (nova.length < 6) { avisar('A nova senha deve ter ao menos 6 caracteres.', 'erro'); return; }
  if (nova !== confirmar) { avisar('A confirmacao nao confere com a nova senha.', 'erro'); return; }
  if (nova === atual) { avisar('A nova senha deve ser diferente da atual.', 'erro'); return; }

  await comBotaoOcupado(botao, 'Salvando...', async () => {
    try {
      await API.trocarSenha(atual, nova);
      avisar('Senha alterada. Use a nova senha no proximo acesso.', 'sucesso');
      ['senhaAtual', 'novaSenha', 'confirmarSenha'].forEach(id => {
        const el = document.getElementById(id); if (el) el.value = '';
      });
      setTimeout(() => { window.location.href = 'perfil.html'; }, 1800);
    } catch (e) {
      avisar(e.mensagem || 'Nao foi possivel alterar a senha.', 'erro');
    }
  });
}

/**
 * Troca o e-mail de login. O token continua valido depois da troca, porque
 * ele carrega o ID do usuario e nao o e-mail -- nao e preciso entrar de novo.
 * O que precisa ser atualizado e a copia do usuario guardada na sessao, senao
 * a interface seguiria mostrando o e-mail antigo ate o proximo login.
 */
async function trocarEmail(botao) {
  const v = id => { const el = document.getElementById(id); return el ? el.value.trim() : ''; };
  const senha = v('emailSenha');
  const novo = v('emailNovo');
  const confirmar = v('emailConfirmar');

  if (!senha) { avisar('Informe sua senha.', 'erro'); return; }
  if (!novo) { avisar('Informe o novo e-mail.', 'erro'); return; }
  if (novo.toLowerCase() !== confirmar.toLowerCase()) {
    avisar('Os dois e-mails nao conferem.', 'erro'); return;
  }

  await comBotaoOcupado(botao, 'Salvando...', async () => {
    try {
      const r = await API.trocarEmail(senha, novo);

      const u = API.usuario;
      if (u) {
        u.email = r.email;
        sessionStorage.setItem(API.CHAVE_USUARIO, JSON.stringify(u));
      }

      avisar('E-mail alterado para <strong>' + r.email + '</strong>.', 'sucesso');
      setTimeout(() => { window.location.href = 'perfil.html'; }, 1800);
    } catch (e) {
      avisar(e.mensagem || 'Nao foi possivel alterar o e-mail.', 'erro');
    }
  });
}

/**
 * Encerra a propria conta.
 *
 * A conta e DESATIVADA, nao apagada: historico, desempenho e certificados
 * apontam para o usuario, e certificados ja emitidos continuam sendo
 * validados por terceiros pelo codigo publico. A tela diz isso ao usuario em
 * vez de prometer uma exclusao que o sistema nao faz.
 */
async function excluirConta(botao) {
  const campo = document.getElementById('senhaExclusao');
  const senha = campo ? campo.value : '';
  if (!senha) { avisar('Informe sua senha para confirmar.', 'erro'); return; }

  await comBotaoOcupado(botao, 'Encerrando...', async () => {
    try {
      await API.desativarConta(senha);
      avisar('Conta encerrada. Voce sera desconectado.', 'sucesso');
      API.limparSessao();
      setTimeout(() => { window.location.href = TQ.getPath('index.html'); }, 2000);
    } catch (e) {
      avisar(e.mensagem || 'Nao foi possivel encerrar a conta.', 'erro');
    }
  });
}

// -------------- 9.10 Editor de curso do tutor --------------
/**
 * A tela do PIM III editava MODULOS, com titulo e descricao proprios. O
 * modelo de dados nao tem modulo: Material (a aula) pende direto de Curso.
 * Entao esta pagina nao foi apenas religada -- ela foi redesenhada sobre o
 * que existe de fato, que sao aulas, prova e questoes.
 *
 * O que a tela permite depende do estado do curso, porque o servidor recusa
 * alteracao em curso publicado ou em avaliacao. Oferecer o campo e deixar a
 * API negar seria frustracao gratuita.
 */
let CURSO_EDITADO = null;

async function carregarEditorCurso() {
  const alvo = document.getElementById('cursoEditor');
  if (!alvo) return;

  const id = parseInt(new URLSearchParams(location.search).get('id'), 10);
  if (!id) {
    alvo.innerHTML = '<div class="card"><p class="text-sm text-muted">' +
      'Nenhum curso informado. Volte para <a href="cursos.html" class="form-link">Meus Cursos</a>.</p></div>';
    return;
  }

  let curso, prova = null;
  try {
    curso = await API.curso(id);
    if (curso && curso.idProva) {
      // Endpoint do TUTOR: traz o gabarito, que o endpoint do aluno esconde.
      try {
        const r = await API.tutorProvaCompleta(curso.idProva);
        prova = r;
      } catch (e) { /* prova recem-criada, ainda sem questoes */ }
    }
  } catch (e) {
    alvo.innerHTML = '<div class="card"><p class="text-sm text-danger">' +
      (e.mensagem || 'Nao foi possivel carregar o curso.') + '</p></div>';
    return;
  }

  CURSO_EDITADO = curso;
  const editavel = ['rascunho', 'rejeitado'].includes((curso.status || '').toLowerCase());
  const aulas = curso.materiais || [];

  const avisoEstado = editavel
    ? ''
    : `<div class="card mb-md" style="background:var(--color-warning-light); border-color:var(--color-warning);">
         <p class="text-sm"><i class="ti ti-lock"></i>
         Curso <strong>${curso.status}</strong>: o conteudo nao pode ser alterado.
         Alterar aulas ou questoes agora mudaria o material debaixo de alunos ja
         matriculados e de provas ja respondidas.</p></div>`;

  alvo.innerHTML = `
    ${avisoEstado}

    <h3 class="section-title">Dados do curso</h3>
    <div class="card">
      <div class="form-group">
        <label class="form-label" for="edNome">Titulo</label>
        <input type="text" class="form-input" id="edNome" value="${(curso.nome || '').replace(/"/g, '&quot;')}" ${editavel ? '' : 'disabled'}>
      </div>
      <div class="form-group">
        <label class="form-label" for="edDescricao">Descricao</label>
        <textarea class="form-input" id="edDescricao" rows="3" ${editavel ? '' : 'disabled'}>${curso.descricao || ''}</textarea>
      </div>
      <div class="form-group">
        <label class="form-label" for="edCategoria">Categoria</label>
        <input type="text" class="form-input" id="edCategoria" value="${curso.categoria || ''}" ${editavel ? '' : 'disabled'}>
      </div>
      <div class="form-group">
        <label class="form-label" for="edNivel">Nivel</label>
        <input type="text" class="form-input" id="edNivel" value="${curso.nivel || ''}" ${editavel ? '' : 'disabled'}>
      </div>
      <div class="form-group">
        <label class="form-label" for="edHoras">Carga horaria (horas)</label>
        <input type="number" class="form-input" id="edHoras" min="1" value="${curso.duracaoHoras || ''}" ${editavel ? '' : 'disabled'}>
      </div>
      ${editavel ? `<button class="btn btn--primary btn--full" onclick="salvarCursoEditado(this)">
                      <i class="ti ti-device-floppy"></i> Salvar dados do curso</button>` : ''}
    </div>

    <h3 class="section-title mt-lg">Aulas <span class="text-sm text-muted">(${aulas.length})</span></h3>
    <div id="listaAulasEditor">
      ${aulas.length ? aulas.map((a, i) => `
        <div class="list-item">
          <div class="list-item__icon"><i class="ti ti-${a.tipo === 'pdf' ? 'file-text' : 'player-play'}"></i></div>
          <div class="list-item__body">
            <div class="list-item__title">${i + 1}. ${a.titulo || 'Aula sem titulo'}</div>
            <div class="list-item__subtitle">${a.tipo || 'material'}</div>
          </div>
          ${editavel ? `<button class="btn btn--ghost btn--sm"
                          style="color:var(--color-danger); border-color:var(--color-danger);"
                          onclick="removerAula(${a.id}, this)"><i class="ti ti-trash"></i></button>` : ''}
        </div>`).join('')
        : '<div class="card"><p class="text-sm text-muted">Nenhuma aula cadastrada. Um curso sem aula nao pode ser submetido.</p></div>'}
    </div>

    ${editavel ? `
    <div class="card mt-md">
      <h4 class="font-semibold mb-md">Adicionar aula</h4>
      <div class="form-group">
        <label class="form-label" for="aulaTitulo">Titulo</label>
        <input type="text" class="form-input" id="aulaTitulo" placeholder="Ex: Introducao a heranca">
      </div>
      <div class="form-group">
        <label class="form-label" for="aulaTipo">Tipo</label>
        <select class="form-input" id="aulaTipo">
          <option value="video">Video</option>
          <option value="pdf">PDF</option>
          <option value="texto">Texto</option>
        </select>
      </div>
      <button class="btn btn--secondary btn--full" onclick="adicionarAula(${curso.id}, this)">
        <i class="ti ti-plus"></i> Adicionar aula</button>
    </div>` : ''}

    <h3 class="section-title mt-lg">Prova</h3>
    ${renderAreaProvaTutor(curso, prova, editavel)}

    ${editavel ? `
    <div class="card mt-lg" style="border-color:var(--color-primary);">
      <h4 class="font-semibold mb-sm">Enviar para avaliacao</h4>
      <p class="text-sm text-muted mb-md">O administrador avalia e publica. Enquanto estiver
         em avaliacao, o conteudo fica bloqueado para alteracao.</p>
      <button class="btn btn--primary btn--full btn--lg" onclick="submeterCurso(${curso.id}, this)">
        <i class="ti ti-upload"></i> Submeter curso</button>
    </div>` : ''}
  `;
}

/**
 * A prova e opcional e so pode existir uma por curso: no modelo do PIM III a
 * chave estrangeira da prova fica no curso (1 curso : 1 prova).
 */
function renderAreaProvaTutor(curso, prova, editavel) {
  if (!curso.idProva) {
    return editavel
      ? `<div class="card">
           <p class="text-sm text-muted mb-md">Este curso ainda nao tem prova.</p>
           <div class="form-group">
             <label class="form-label" for="provaTitulo">Titulo da prova</label>
             <input type="text" class="form-input" id="provaTitulo" placeholder="Ex: Prova final de ${curso.nome}">
           </div>
           <div class="form-group">
             <label class="form-label" for="provaNota">Nota minima (0 a 10)</label>
             <input type="number" class="form-input" id="provaNota" min="0" max="10" step="0.5" value="7">
           </div>
           <div class="form-group">
             <label class="form-label" for="provaTempo">Tempo (minutos)</label>
             <input type="number" class="form-input" id="provaTempo" min="5" value="30">
           </div>
           <button class="btn btn--secondary btn--full" onclick="criarProvaDoCurso(${curso.id}, this)">
             <i class="ti ti-plus"></i> Criar prova</button>
         </div>`
      : '<div class="card"><p class="text-sm text-muted">Este curso nao tem prova.</p></div>';
  }

  const questoes = prova ? (prova.questoes || []) : [];
  return `
    <div class="card">
      <div class="flex justify-between items-center mb-sm">
        <strong>${prova ? prova.titulo : 'Prova'}</strong>
        <span class="badge badge--neutral">${questoes.length} questoes</span>
      </div>
      ${prova ? `<div class="text-sm text-muted">Nota minima ${prova.notaMinima} - ${prova.tempoMinutos} min</div>` : ''}
      ${questoes.length === 0
        ? '<p class="text-sm text-danger mt-sm"><i class="ti ti-alert-triangle"></i> Prova sem questoes: o administrador vai rejeitar.</p>'
        : `<div class="mt-md">${questoes.map(q =>
             `<div class="list-item">
                <div class="list-item__icon">${q.ordem}</div>
                <div class="list-item__body">
                  <div class="list-item__title">${(q.enunciado || '').slice(0, 80)}${(q.enunciado || '').length > 80 ? '...' : ''}</div>
                  <div class="list-item__subtitle">
                    ${(q.alternativas || []).length} alternativas - gabarito
                    <strong>${(q.alternativas || []).find(a => a.ehCorreta)?.letra || '?'}</strong>
                  </div>
                </div>
                ${editavel ? `<button class="btn btn--ghost btn--sm"
                                style="color:var(--color-danger); border-color:var(--color-danger);"
                                onclick="removerQuestao(${q.id}, this)"><i class="ti ti-trash"></i></button>` : ''}
              </div>`).join('')}</div>`}
    </div>

    ${editavel ? `
    <div class="card mt-md">
      <h4 class="font-semibold mb-md">Adicionar questao</h4>
      <div class="form-group">
        <label class="form-label" for="qEnunciado">Enunciado</label>
        <textarea class="form-input" id="qEnunciado" rows="3" placeholder="O que a questao pergunta"></textarea>
      </div>
      <div class="form-group">
        <label class="form-label" for="qCodigo">Trecho de codigo (opcional)</label>
        <textarea class="form-input" id="qCodigo" rows="3"
                  placeholder="Codigo exibido em bloco separado do enunciado" style="font-family:monospace;"></textarea>
      </div>
      <p class="text-sm text-muted mb-sm">Marque a alternativa correta. Exatamente uma.</p>
      ${['A','B','C','D','E'].map(letra => `
        <div class="flex items-center gap-sm mb-sm">
          <input type="radio" name="qCorreta" value="${letra}" id="qCorreta${letra}" ${letra === 'A' ? 'checked' : ''}>
          <label for="qCorreta${letra}" class="font-semibold" style="width:18px;">${letra}</label>
          <input type="text" class="form-input" id="qAlt${letra}" placeholder="Texto da alternativa ${letra}">
        </div>`).join('')}
      <button class="btn btn--secondary btn--full mt-md"
              onclick="adicionarQuestao(${curso.idProva}, ${questoes.length + 1}, this)">
        <i class="ti ti-plus"></i> Adicionar questao</button>
    </div>` : ''}
  `;
}

async function removerQuestao(idQuestao, botao) {
  await comBotaoOcupado(botao, '...', async () => {
    try {
      await API.tutorRemoverQuestao(idQuestao);
      avisar('Questao removida.', 'sucesso');
      carregarEditorCurso();
    } catch (e) {
      // O servidor recusa alterar prova que algum aluno ja respondeu.
      avisar(e.mensagem || 'Nao foi possivel remover a questao.', 'erro');
    }
  });
}

async function salvarCursoEditado(botao) {
  if (!CURSO_EDITADO) return;
  const v = id => { const el = document.getElementById(id); return el ? el.value.trim() : ''; };
  const nome = v('edNome');
  if (!nome) { avisar('O titulo do curso e obrigatorio.', 'erro'); return; }

  const horas = parseInt(v('edHoras'), 10);

  await comBotaoOcupado(botao, 'Salvando...', async () => {
    try {
      await API.tutorAtualizarCurso(CURSO_EDITADO.id, {
        nome,
        descricao: v('edDescricao') || null,
        categoria: v('edCategoria') || null,
        nivel: v('edNivel') || null,
        duracaoHoras: isNaN(horas) ? null : horas
      });
      avisar('Dados do curso salvos.', 'sucesso');
    } catch (e) {
      avisar(e.mensagem || 'Nao foi possivel salvar.', 'erro');
    }
  });
}

async function removerAula(idMaterial, botao) {
  await comBotaoOcupado(botao, '...', async () => {
    try {
      await API.tutorRemoverAula(idMaterial);
      avisar('Aula removida.', 'sucesso');
      carregarEditorCurso();
    } catch (e) {
      // O servidor recusa remover aula que algum aluno ja assistiu.
      avisar(e.mensagem || 'Nao foi possivel remover a aula.', 'erro');
    }
  });
}

async function criarProvaDoCurso(idCurso, botao) {
  const v = id => { const el = document.getElementById(id); return el ? el.value.trim() : ''; };
  const titulo = v('provaTitulo');
  if (!titulo) { avisar('Informe o titulo da prova.', 'erro'); return; }

  const nota = parseFloat(v('provaNota'));
  const tempo = parseInt(v('provaTempo'), 10);

  await comBotaoOcupado(botao, 'Criando...', async () => {
    try {
      await API.tutorCriarProva(idCurso, {
        titulo,
        notaMinima: isNaN(nota) ? 7 : nota,
        tempoMinutos: isNaN(tempo) ? 30 : tempo
      });
      avisar('Prova criada. Agora adicione as questoes.', 'sucesso');
      carregarEditorCurso();
    } catch (e) {
      avisar(e.mensagem || 'Nao foi possivel criar a prova.', 'erro');
    }
  });
}

async function adicionarQuestao(idProva, ordem, botao) {
  const v = id => { const el = document.getElementById(id); return el ? el.value.trim() : ''; };
  const enunciado = v('qEnunciado');
  if (!enunciado) { avisar('Informe o enunciado.', 'erro'); return; }

  const correta = (document.querySelector('input[name="qCorreta"]:checked') || {}).value;

  // Alternativa em branco e simplesmente ignorada: nem toda questao usa as cinco.
  const alternativas = ['A','B','C','D','E']
    .map(letra => ({ letra, texto: v('qAlt' + letra), ehCorreta: letra === correta }))
    .filter(a => a.texto);

  if (alternativas.length < 2) { avisar('Preencha ao menos duas alternativas.', 'erro'); return; }
  if (!alternativas.some(a => a.ehCorreta)) {
    avisar('A alternativa marcada como correta esta sem texto.', 'erro'); return;
  }

  await comBotaoOcupado(botao, 'Adicionando...', async () => {
    try {
      await API.tutorAdicionarQuestao(idProva, {
        enunciado,
        codigoExemplo: v('qCodigo') || null,
        ordem,
        alternativas
      });
      avisar('Questao adicionada.', 'sucesso');
      carregarEditorCurso();
    } catch (e) {
      avisar(e.mensagem || 'Nao foi possivel adicionar a questao.', 'erro');
    }
  });
}

// -------------- 10. Init na carga da página --------------
window.addEventListener('DOMContentLoaded', async function () {
  // A guarda roda ANTES de carregar dados: não faz sentido buscar
  // informação de uma página da qual o usuário será expulso.
  if (!protegerPagina()) return;

  await TQ.init();

  const persona = localStorage.getItem('tq_persona');
  if (persona && !TQ.online) TQ.personaAtual = persona;

  injetarSidebarEstudante();
  avisarApiIndisponivel();

  if (typeof onPageLoad === 'function') onPageLoad();
});
