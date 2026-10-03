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
