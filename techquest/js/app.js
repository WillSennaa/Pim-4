/* ==========================================================================
   Tech Quest - JavaScript principal
   ========================================================================== */

// -------------- 1. Estado global --------------
const TQ = {
  data: null,
  questoes: null,
  personaAtual: 'estudante',
  init: async function () {
    try {
      const [mockRes, quizRes] = await Promise.all([
        fetch(this.getPath('data/mock.json')),
        fetch(this.getPath('data/questoes.json'))
      ]);
      this.data = await mockRes.json();
      this.questoes = await quizRes.json();
    } catch (e) {
      console.warn('Erro ao carregar dados:', e);
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

// -------------- 2. Sidebar do estudante (injeção automática) --------------
function injetarSidebarEstudante() {
  const path = window.location.pathname;
  if (!path.includes('/pages/estudante/')) return;

  const container = document.querySelector('.app-container');
  if (!container) return;
  if (container.querySelector('.sidebar-nav')) return;

  // Detecta qual item está ativo pela URL
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

  // Move tudo o conteúdo atual pra dentro de um wrapper
  const wrapper = document.createElement('div');
  wrapper.style.cssText = 'flex:1; display:flex; flex-direction:column; min-width:0;';
  while (container.firstChild) {
    wrapper.appendChild(container.firstChild);
  }
  container.appendChild(sidebar);
  container.appendChild(wrapper);
}

// -------------- 3. Persona switcher --------------
function trocarPersona(persona) {
  TQ.personaAtual = persona;
  localStorage.setItem('tq_persona', persona);
  const basePath = TQ.getPath('');
  const rotas = {
    'estudante': basePath + 'pages/estudante/home.html',
    'tutor': basePath + 'pages/tutor/home.html',
    'admin': basePath + 'pages/admin/home.html'
  };
  window.location.href = rotas[persona] || rotas['estudante'];
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
  // Cria o modal dinamicamente se não existir
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
  // Mantém a persona para facilitar próximo login (UX), mas limpa sessão
  sessionStorage.clear();
  // Caminho relativo para o index baseado em onde estamos
  const path = window.location.pathname;
  if (path.includes('/pages/estudante/') || path.includes('/pages/tutor/') || path.includes('/pages/admin/')) {
    window.location.href = '../../index.html';
  } else if (path.includes('/pages/')) {
    window.location.href = '../index.html';
  } else {
    window.location.href = 'index.html';
  }
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

// -------------- 7. Login / Cadastro fake --------------
function fazerLogin(event) {
  if (event) event.preventDefault();
  // Respeita a persona selecionada (se houver)
  const persona = localStorage.getItem('tq_persona') || 'estudante';
  const rotas = {
    'estudante': 'pages/estudante/home.html',
    'tutor': 'pages/tutor/home.html',
    'admin': 'pages/admin/home.html'
  };
  window.location.href = TQ.getPath(rotas[persona]);
  return false;
}
function fazerCadastro(event) {
  if (event) event.preventDefault();
  alert('Cadastro realizado com sucesso! Você será redirecionado para o login.');
  window.location.href = TQ.getPath('pages/login.html');
  return false;
}

// -------------- 8. Quiz (suporta múltiplas provas) --------------
const QUIZ = {
  questaoAtual: 0,
  respostas: {},
  questoes: [],
  provaKey: 'prova_csharp',
  rotaAprovado: 'resultado-aprovado.html',
  rotaReprovado: 'resultado-reprovado.html',
  rotaVoltar: 'curso.html',

  iniciar: async function (provaKey, rotaApr, rotaRep, rotaVoltar) {
    if (!TQ.questoes) await TQ.init();
    this.provaKey = provaKey || 'prova_csharp';
    if (rotaApr) this.rotaAprovado = rotaApr;
    if (rotaRep) this.rotaReprovado = rotaRep;
    if (rotaVoltar) this.rotaVoltar = rotaVoltar;
    const prova = TQ.questoes[this.provaKey];
    if (!prova) {
      console.error('Prova não encontrada:', this.provaKey);
      return;
    }
    this.questoes = prova.questoes;
    this.questaoAtual = 0;
    this.respostas = {};

    // Atualiza header
    const titEl = document.getElementById('quizTitulo');
    const subEl = document.getElementById('quizSubtitulo');
    if (titEl) titEl.textContent = prova.titulo;
    if (subEl) subEl.textContent = 'Curso: ' + prova.curso;

    this.renderizar();
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
    const letras = Object.keys(q.alternativas);
    letras.forEach(letra => {
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

  finalizar: function () {
    let acertos = 0;
    this.questoes.forEach((q, i) => {
      if (QUIZ.respostas[i] === q.resposta_correta) acertos++;
    });
    const nota = (acertos / this.questoes.length) * 10;
    const notaMinima = TQ.questoes[this.provaKey].nota_minima;
    const aprovado = nota >= notaMinima;
    sessionStorage.setItem('tq_resultado', JSON.stringify({
      acertos, total: this.questoes.length, nota: nota.toFixed(1),
      aprovado, prova: this.provaKey
    }));
    window.location.href = aprovado ? this.rotaAprovado : this.rotaReprovado;
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
  if (!TQ.data) return;
  const e = TQ.data.personas.estudante;
  const setText = (id, val) => { const el = document.getElementById(id); if (el) el.textContent = val; };

  setText('userName', e.nome.split(' ')[0]);
  setText('userInitials', e.iniciais);
  setText('userLevel', e.nivel);
  setText('userXP', e.xp.toLocaleString('pt-BR'));
  setText('userXPNext', e.xp_proximo_nivel.toLocaleString('pt-BR'));
  setText('userQuote', e.frase_motivacional);
  const bar = document.getElementById('userProgressBar');
  if (bar) bar.style.width = e.progresso_nivel + '%';
  const barLabel = document.getElementById('userProgressLabel');
  if (barLabel) barLabel.textContent = e.progresso_nivel + '%';

  const conqList = document.getElementById('conquistasList');
  if (conqList) {
    conqList.innerHTML = TQ.data.conquistas_recentes.map(c =>
      '<div class="list-item">' +
        '<div class="list-item__icon"><i class="ti ' + c.icone + '"></i></div>' +
        '<div class="list-item__body">' +
          '<div class="list-item__title">' + c.titulo + '</div>' +
          '<div class="list-item__subtitle">' + c.tempo + '</div>' +
        '</div>' +
        '<div class="text-warning font-semibold text-sm">+' + c.xp + ' XP</div>' +
      '</div>'
    ).join('');
  }
}

// -------------- 10. Init na carga da página --------------
window.addEventListener('DOMContentLoaded', async function () {
  await TQ.init();
  const persona = localStorage.getItem('tq_persona');
  if (persona) TQ.personaAtual = persona;

  // Injeta sidebar nas páginas do estudante
  injetarSidebarEstudante();

  if (typeof onPageLoad === 'function') onPageLoad();
});
