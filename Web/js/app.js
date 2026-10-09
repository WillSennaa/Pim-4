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

  /**
   * Caminho relativo da pagina atual ate a raiz do site.
   *
   * A versao anterior LISTAVA as pastas de area pelo nome -- estudante, tutor,
   * admin -- e devolvia '../../' para elas e '../' para o resto. Quando
   * pages/conta/ foi criada, ninguem atualizou a lista: a pasta tem a mesma
   * profundidade das outras e recebia '../', um nivel a menos. Dois defeitos
   * sairam dai, ambos nessa tela:
   *   - "Sair da conta" ia para /pages/index.html (404);
   *   - a guarda de rota, ao expirar o token, mandava para
   *     /pages/pages/login.html (404) em vez da tela de login.
   *
   * Agora a profundidade e CONTADA a partir de /pages/. Pasta nova funciona
   * sem ninguem lembrar de vir aqui.
   */
  getPath: function (p) {
    const i = window.location.pathname.indexOf('/pages/');
    if (i < 0) return p;                       // ja esta na raiz
    const resto = window.location.pathname.slice(i + '/pages/'.length);
    return '../'.repeat(resto.split('/').length) + p;
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
      // Substituicao, nao mesclagem: herdar do mock fazia campos ficticios
      // (nivel 7, 2450 XP) sobreviverem quando a API nao respondia, e a tela
      // misturava numero real com numero inventado.
      d.personas.estudante = ({
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
      // Duvidas ENDERECADAS ao tutor, em qualquer situacao. A caixa geral
      // traz tambem o que ele mesmo enviou, que nao e o painel de duvidas.
      API.tutorCursos(), API.tutorAlunos(), API.chamadosRecebidos()
    ]);

    const d = TQ.data;
    const u = API.usuario;

    d.personas = d.personas || {};
    d.personas.tutor = ({
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

      const porStatus = st => cursos.value.filter(c => (c.status || '').toLowerCase() === st).length;
      d.personas.tutor.cursos_publicados = porStatus('publicado');
      d.personas.tutor.cursos_rascunho = porStatus('rascunho');
      d.personas.tutor.cursos_pendentes = porStatus('pendente');
      d.personas.tutor.total_cursos = cursos.value.length;
      d.personas.tutor.total_materiais = cursos.value.reduce((soma, c) => soma + (c.totalAulas || 0), 0);
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
      // Todas as situacoes: as abas Pendentes / Respondidas / Todas filtram
      // em cima desta lista, sem nova ida ao servidor.
      d.tutor_duvidas = chamados.value.map(c => ({
        id: c.id,
        id_remetente: c.idRemetente,
        aluno: c.remetente,
        iniciais: FMT.iniciais(c.remetente),
        pergunta: c.descricao,
        assunto: c.assunto,
        curso: c.curso || 'sem curso informado',
        tempo: FMT.relativo(c.dataAbertura),
        status: c.status,
        pendente: c.status !== 'Resolvido'
      }));
      d.personas.tutor.duvidas_pendentes = d.tutor_duvidas.filter(x => x.pendente).length;
    }
  },

  // ---------------- ADMIN ----------------
  async carregarAdmin() {
    // Os QUATRO estados, nao dois. A tela de cursos do admin tem cartao de
    // Rascunho e de Rejeitado, e o adaptador so trazia Publicado e Pendente:
    // os dois cartoes mostravam zero por falta de dado, nao por ausencia de
    // curso naquele estado.
    const [resumo, pendentes, publicados, rascunhos, rejeitados, usuarios, logs] =
      await Promise.allSettled([
        API.adminResumo(),
        API.adminCursos('Pendente'), API.adminCursos('Publicado'),
        API.adminCursos('Rascunho'), API.adminCursos('Rejeitado'),
        API.adminUsuarios(), API.adminLogs(50)
      ]);

    const d = TQ.data;
    const u = API.usuario;

    d.personas = d.personas || {};
    d.personas.admin = ({
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
    [publicados, pendentes, rascunhos, rejeitados].forEach(r => {
      if (r.status === 'fulfilled') {
        r.value.forEach(c => listaCursos.push({
          id: c.idCurso,
          titulo: c.nome,
          tutor: c.tutor,
          // 'alunos' e 'rating' sairam: eram sempre null e a tela chamava
          // .toLocaleString() neles. Campo que nunca tem valor nao e campo,
          // e armadilha.
          status: (c.status || '').toLowerCase(),
          categoria: c.categoria,
          modulos: c.totalAulas,
          tem_prova: c.temProva,
          total_questoes: c.totalQuestoes,
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
        // ultimo_acesso, cursos_concluidos e xp sairam: o modelo nao registra
        // ultimo acesso, e os outros dois nao vem no DTO de /api/admin/usuarios.
        // Eram sempre '—' ou null -- campo vazio que a tela precisava tratar.
        data_cadastro: FMT.data(x.dataCadastro)
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

  /**
   * Data longa para a saudacao ("Quarta-feira, 8 de outubro de 2026").
   * A home trazia "Segunda-feira, 22 de Maio de 2024" escrito no HTML desde o
   * PIM III -- um texto que envelheceu junto com o arquivo. Data do dia e
   * informacao do RELOGIO DE QUEM OLHA, nao do banco: nao existe (nem deveria
   * existir) tabela para o dia de hoje.
   */
  dataLonga(d) {
    const dt = d ? new Date(d) : new Date();
    if (isNaN(dt)) return '';
    const texto = dt.toLocaleDateString('pt-BR',
      { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' });
    return texto.charAt(0).toUpperCase() + texto.slice(1);
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

  // Area de conta: exige estar autenticado, mas serve as tres personas.
  const areaComum = path.includes('/pages/conta/');

  const area = Object.keys(areas).find(a => path.includes(a));
  if (!area && !areaComum) return true;

  if (typeof API === 'undefined' || !API.autenticado) {
    window.location.href = TQ.getPath('pages/login.html?exigeLogin=1');
    return false;
  }

  const papel = API.papel;

  // Na area comum basta estar autenticado: qualquer papel passa.
  if (areaComum) return true;

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
    'ajuda.html': 'ajuda', 'suporte.html': 'ajuda',
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
    <a href="ajuda.html" class="sidebar-nav__item ${active === 'ajuda' ? 'active' : ''}">
      <i class="ti ti-help-circle"></i> Ajuda e Suporte
    </a>
    <a href="perfil.html" class="sidebar-nav__item ${active === 'perfil' ? 'active' : ''}">
      <i class="ti ti-user"></i> Perfil
    </a>
    <div style="flex:1;"></div>
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

    const idCurso = CONTEXTO.parametro('id');
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

    // O cabecalho dizia "Prova: Arrays e Listas / Curso: Fundamentos de C#"
    // escrito no HTML, para qualquer prova. Agora sai do proprio DTO.
    const titEl = document.getElementById('quizTitulo');
    const subEl = document.getElementById('quizSubtitulo');
    if (titEl) titEl.textContent = this.prova.titulo || 'Prova';
    if (subEl) {
      subEl.textContent = this.prova.totalQuestoes + ' questoes - nota minima ' +
        String(this.prova.notaMinima).replace('.', ',') +
        (this.prova.tempoMinutos ? ' - ' + this.prova.tempoMinutos + ' min' : '');
    }

    // "Sair da prova" voltava para curso.html sem o id, caindo no curso de C#
    // por padrao, qualquer que fosse a prova.
    const sair = document.getElementById('linkSairProva');
    if (sair && idCurso) sair.href = 'curso.html?id=' + idCurso;

    this.renderizar();
  },

  /**
   * Resolve qual prova carregar.
   *
   * ORDEM, e por que ela importa: o ?id= da URL e o id do CURSO (os links
   * chegam de curso.html?id=N e da home), nao o id da prova. Tratar esse
   * numero como id de prova carregaria a prova errada -- e silenciosamente,
   * porque os dois sao inteiros. Por isso a URL e consultada primeiro, pela
   * via do curso, e so depois vem a chave antiga do PIM III.
   */
  resolverIdProva: async function (chave) {
    const idCurso = CONTEXTO.parametro('id');
    if (idCurso) {
      try {
        const detalhe = await API.curso(idCurso);
        if (detalhe && detalhe.idProva) return detalhe.idProva;
      } catch { /* cai para a resolucao por nome abaixo */ }
    }

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
        // O id do CURSO tambem: as telas de resultado tinham links fixos para
        // prova.html e curso.html sem parametro, que caiam sempre no curso de
        // C# por causa do CONTEXTO.PADRAO.
        id_curso: CONTEXTO.parametro('id'),
        nota_minima: r.notaMinima,
        tentativa: r.numeroTentativa,
        xp_ganho: r.xpGanho,
        medalhas_novas: r.medalhasNovas || [],
        correcao: r.correcao,
        // As telas de resultado precisam do ENUNCIADO e das ALTERNATIVAS para
        // montar a revisao; a correcao do servidor traz so as letras. Guardar
        // aqui evita refazer GET /api/provas/{id} na tela seguinte -- e, mais
        // importante, evita pedir a prova de novo depois de ela ter sido
        // entregue. O gabarito (letraCorreta) vem da resposta da submissao,
        // nunca do download da prova.
        prova_titulo: this.prova ? this.prova.titulo : null,
        questoes: this.questoes.map(q => ({
          id: q.id, enunciado: q.enunciado, codigo: q.codigo, alternativas: q.alternativas
        }))
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
async function renderEstudanteHome() {
  if (!TQ.data || !TQ.data.personas) return;
  const e = TQ.data.personas.estudante;
  if (!e) return;
  const setText = (id, val) => { const el = document.getElementById(id); if (el) el.textContent = val; };

  setText('userName', (e.nome || '').split(' ')[0]);
  // Dois elementos tinham o MESMO id="userInitials" (o avatar da barra e o do
  // cartao de nivel). getElementById devolve so o primeiro, entao o segundo
  // ficava com "--" para sempre. A classe resolve os dois.
  document.querySelectorAll('#userInitials, .js-iniciais')
    .forEach(el => { el.textContent = e.iniciais; });
  setText('dataDeHoje', FMT.dataLonga());
  setText('userLevel', e.nivel);
  // userLevelText e userProximoNivel existiam no HTML com valor fixo e nunca
  // eram preenchidos: o badge mostrava o nivel real e o texto ao lado, o do
  // mock. Era a inconsistencia de "Nivel 7" com "faltam X XP para o Nivel 2".
  setText('userLevelText', e.nivel);
  setText('userProximoNivel', (e.nivel || 1) + 1);
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

  await renderCursosEmAndamento();
  await renderAtalhoCertificados();
}

/**
 * "Continue de onde parou".
 *
 * No PIM III eram DOIS cartoes escritos no HTML -- "Fundamentos de C# 50%" e
 * "Banco de Dados e Modelagem 30%" -- com link para arquivos proprios de cada
 * curso. Numero fixo e arquivo por curso sao a mesma limitacao vista de dois
 * angulos: um curso novo exigiria editar a home.
 *
 * Agora a lista sai de /api/perfil/historico (os cursos em que o aluno esta
 * matriculado, com o percentual real) e o cartao de "proxima prova" so aparece
 * quando existe um curso 100% concluido com prova -- que e exatamente a regra
 * que o servidor usa para liberar a prova.
 */
async function renderCursosEmAndamento() {
  const alvo = document.getElementById('cursosEmAndamento');
  if (!alvo) return;

  let historico;
  try {
    historico = await API.historico();
  } catch (e) {
    alvo.innerHTML = '<div class="card"><p class="text-sm text-danger">' +
      (e.mensagem || 'Nao foi possivel carregar seus cursos.') + '</p></div>';
    return;
  }

  const emAndamento = historico.filter(h => h.status !== 'Concluido');
  if (!emAndamento.length) {
    alvo.innerHTML = historico.length
      ? '<div class="card"><p class="text-sm text-muted">Voce concluiu todos os cursos em que esta matriculado. ' +
        '<a href="cursos.html" class="text-primary font-semibold">Ver catalogo</a></p></div>'
      : '<div class="card"><p class="text-sm text-muted">Voce ainda nao esta matriculado em nenhum curso. ' +
        '<a href="cursos.html" class="text-primary font-semibold">Ver catalogo</a></p></div>';
    return;
  }

  alvo.innerHTML = emAndamento.map(h => `
    <div class="card course-progress-card animate-in">
      <span class="badge badge--primary">EM ANDAMENTO</span>
      <h3 class="course-progress-card__title mt-sm">${h.curso}</h3>
      <div class="course-progress-card__info">
        <span class="text-secondary">${h.aulasConcluidas} de ${h.totalAulas} materiais</span>
        <span class="font-semibold">${h.percentualCurso}%</span>
      </div>
      <div class="progress mb-md">
        <div class="progress__fill" style="width:${h.percentualCurso}%"></div>
      </div>
      <a href="curso.html?id=${h.idCurso}" class="btn btn--primary btn--full">
        Continuar <i class="ti ti-player-play"></i></a>
    </div>`).join('');

  // Prova liberada: curso com todos os materiais concluidos e que tem prova.
  const prontos = emAndamento.filter(h => h.totalAulas > 0 && h.percentualCurso === 100);
  const area = document.getElementById('proximaProva');
  if (!area) return;

  for (const h of prontos) {
    let curso;
    try { curso = await API.curso(h.idCurso); } catch { continue; }
    if (!curso || !curso.idProva) continue;
    area.innerHTML = `
      <div class="card next-test-card animate-in">
        <span class="badge badge--danger">PROVA LIBERADA</span>
        <h3 class="course-progress-card__title mt-sm">${curso.nome}</h3>
        <div class="next-test-card__row"><i class="ti ti-checks"></i><span>
          ${h.aulasConcluidas} de ${h.totalAulas} materiais concluidos</span></div>
        <div class="next-test-card__row"><i class="ti ti-chart-bar"></i><span>
          Nivel: ${curso.nivel || 'nao informado'}</span></div>
        <a href="prova.html?id=${curso.id}" class="btn btn--secondary btn--full mt-md">
          Fazer a prova <i class="ti ti-arrow-right"></i></a>
      </div>`;
    return;
  }
  area.innerHTML = '';
}

/** Atalho de certificados na home: contagem real, nao "1 emitido". */
async function renderAtalhoCertificados() {
  const alvo = document.getElementById('atalhoCertificados');
  if (!alvo) return;
  try {
    const certs = await API.certificados();
    alvo.textContent = certs.length === 0
      ? 'nenhum certificado emitido ainda'
      : certs.length === 1
        ? '1 certificado emitido'
        : certs.length + ' certificados emitidos';
  } catch {
    alvo.textContent = 'nao foi possivel consultar';
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
  // Os arquivos *-bd.html do PIM III eram a COPIA das telas do curso de C# com
  // o conteudo do curso de Banco de Dados escrito a mao. Com a pagina
  // parametrizada por ?id=, viraram redirecionamentos: a entrada continua
  // valida, mas nao existe mais conteudo duplicado para manter em dois lugares.
  PADRAO: {
    'curso.html': 'C#', 'aula.html': 'C#', 'prova.html': 'C#',
    'resultado-aprovado.html': 'C#', 'resultado-reprovado.html': 'C#'
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

  // Badges do hero vinham escritos no HTML ("PROGRAMMING", "LEVEL 1") para
  // qualquer curso. Agora saem das colunas Categoria e Nivel.
  texto('cursoCategoriaBadge', (curso.categoria || 'sem categoria').toUpperCase());
  texto('cursoNivelBadge', (curso.nivel || 'sem nivel').toUpperCase());

  // O botao de matricula aparecia sempre, mesmo para quem ja estava
  // matriculado -- e uma segunda matricula e silenciosamente ignorada pelo
  // servidor, o que deixava o aluno sem saber o que aconteceu. Quem sabe se
  // ele esta matriculado e o servidor: o campo vem no CursoDetalheDto.
  const areaMatricula = document.getElementById('areaMatricula');
  if (areaMatricula) {
    areaMatricula.innerHTML = curso.matriculado
      ? '<p class="text-sm text-success mt-md"><i class="ti ti-circle-check"></i> ' +
        'Voce esta matriculado neste curso.</p>'
      : '<button class="btn btn--secondary btn--full btn--sm mt-md" onclick="matricularNoCurso(this)">' +
        '<i class="ti ti-bookmark"></i> Matricular-me neste curso</button>';
  }

  texto('cursoProgressoLabel', percentual + '%');
  const barra = document.getElementById('cursoProgressoBarra');
  if (barra) barra.style.width = percentual + '%';
  texto('cursoAulasResumo', concluidas + ' de ' + aulas.length + ' materiais concluidos');

  if (aulas.length === 0) {
    alvo.innerHTML = '<div class="card"><p class="text-sm text-muted">Este curso ainda nao tem materiais publicados.</p></div>';
    return;
  }

  // A primeira aula não concluída é a "atual": é para onde o botão principal
  // leva, para o aluno não precisar procurar onde parou.
  const proxima = aulas.find(a => !a.concluido) || aulas[aulas.length - 1];
  const paginaAula = 'aula.html';

  alvo.innerHTML = aulas.map((a, i) => {
    const atual = a.id === proxima.id && !a.concluido;
    const estado = a.concluido ? 'done' : (atual ? 'active' : 'locked');
    const classe = a.concluido ? '' : (atual ? 'module-item--active' : '');
    const icone = a.concluido ? 'ti-check' : (atual ? 'ti-player-play' : 'ti-file-text');
    // Material concluido continua aberto para releitura. Travar o acesso
    // depois de concluir nao protege nada -- o progresso ja esta gravado -- e
    // impede o aluno de revisar antes da prova, que e justamente quando ele
    // mais precisa voltar. "A seguir" tambem deixou de ser um cadeado: a
    // ordem e uma sugestao de trilha, nao uma restricao do modelo.
    const link = `${paginaAula}?id=${curso.id}&aula=${a.id}`;
    const selo = a.concluido
      ? `<a href="${link}" class="badge badge--success">CONCLUIDO · REVER</a>`
      : (atual
          ? `<a href="${link}" class="badge badge--primary">ABRIR</a>`
          : `<a href="${link}" class="badge badge--neutral">A SEGUIR</a>`);

    return `
    <div class="module-item ${classe} animate-in">
      <div class="module-item__icon module-item__icon--${estado}"><i class="ti ${icone}"></i></div>
      ${i < aulas.length - 1 ? '<div class="module-item__line"></div>' : ''}
      <div class="module-item__body">
        <div class="module-item__card">
          <div class="flex justify-between items-center">
            <span class="module-item__label ${atual ? 'text-primary' : ''}">MATERIAL ${i + 1}${atual ? ' • ATUAL' : ''}</span>
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
    const paginaProva = 'prova.html';
    areaProva.innerHTML = liberada
      ? `<a href="${paginaProva}?id=${curso.id}" class="btn btn--primary btn--full btn--lg">
           Fazer a prova <i class="ti ti-file-text"></i></a>`
      : `<button class="btn btn--secondary btn--full btn--lg" disabled>
           Conclua todos os materiais para liberar a prova</button>`;
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

  // renderAula() ja resolveu qual material esta aberto; usa-se o mesmo, para
  // a tela nao concluir um material diferente do que o aluno estava lendo.
  let alvo = (TQ.materialAtual && TQ.materialAtual.id) || idAula;
  if (!alvo) {
    // Sem ?aula= na URL: assume o primeiro material nao concluido do curso.
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

      let msg = `Material concluido. <strong>${r.aulasConcluidas} de ${r.totalAulas}</strong> ` +
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
        // Destino calculado em renderAula(): proximo material, prova ou curso.
        // O PIM III mandava sempre para a pagina do curso.
        window.location.href = TQ.destinoPosConclusao
          || ('curso.html' + (idCurso ? '?id=' + idCurso : ''));
      }, 2200);
    } catch (e) {
      avisar(e.mensagem || 'Nao foi possivel registrar o material.', 'erro');
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
/**
 * Abre um chamado. O tipo decide o caminho, e quem escolhe o destinatario e
 * o servidor:
 *   Duvida  -> tutor do curso escolhido
 *   Tecnico -> fila dos administradores
 */
async function enviarChamado(botao) {
  const valor = id => { const el = document.getElementById(id); return el ? el.value.trim() : ''; };
  const assunto = valor('chamadoAssunto');
  const descricao = valor('chamadoDescricao');
  const tipo = valor('chamadoTipo') || 'Tecnico';
  const idCurso = parseInt(valor('chamadoCurso'), 10);

  if (!assunto) { avisar('Informe o assunto.', 'erro'); return; }
  if (!descricao) { avisar('Descreva o que aconteceu.', 'erro'); return; }
  if (tipo === 'Duvida' && isNaN(idCurso)) {
    avisar('Escolha o curso sobre o qual e a duvida.', 'erro'); return;
  }

  await comBotaoOcupado(botao, 'Enviando...', async () => {
    try {
      const c = await API.abrirChamado(tipo, assunto, descricao, null,
                                       isNaN(idCurso) ? null : idCurso);
      const destino = tipo === 'Duvida'
        ? 'Sua duvida foi enviada ao tutor do curso'
        : 'Seu chamado foi enviado ao suporte';
      avisar(destino + '. Protocolo <strong>#' + c.id + '</strong>.', 'sucesso');

      ['chamadoAssunto', 'chamadoDescricao'].forEach(id => {
        const el = document.getElementById(id); if (el) el.value = '';
      });
      carregarMeusChamados();
    } catch (e) {
      avisar(e.mensagem || 'Nao foi possivel abrir o chamado.', 'erro');
    }
  });
}

/** Curso so faz sentido para duvida de conteudo; o campo aparece conforme o tipo. */
function ajustarFormularioChamado() {
  // A central de ajuda manda ?tipo=Duvida ou ?tipo=Tecnico: quem chegou pelo
  // atalho ja encontra o formulario certo, sem precisar escolher de novo.
  const select = document.getElementById('chamadoTipo');
  const daUrl = new URLSearchParams(window.location.search).get('tipo');
  if (select && daUrl && !select.dataset.ajustado) {
    if ([...select.options].some(o => o.value === daUrl)) select.value = daUrl;
    select.dataset.ajustado = '1';
  }

  const tipo = (select || {}).value;
  const area = document.getElementById('areaCurso');
  const aviso = document.getElementById('avisoDestino');

  const ehDuvida = tipo === 'Duvida';
  if (area) area.style.display = ehDuvida ? '' : 'none';
  if (aviso) {
    aviso.innerHTML = ehDuvida
      ? '<i class="ti ti-user-bolt"></i> Esta duvida vai para o <strong>tutor do curso</strong> escolhido.'
      : '<i class="ti ti-tool"></i> Este chamado vai para a <strong>equipe de suporte</strong> (administradores).';
  }
}

/** Preenche a escolha de curso com as matriculas do aluno. */
async function carregarCursosDoChamado() {
  const select = document.getElementById('chamadoCurso');
  if (!select) return;
  try {
    const historico = await API.historico();
    select.innerHTML = historico.length
      ? historico.map(h => `<option value="${h.idCurso}">${h.curso}</option>`).join('')
      : '<option value="">Voce ainda nao esta matriculado em nenhum curso</option>';
  } catch (e) {
    select.innerHTML = '<option value="">Nao foi possivel carregar seus cursos</option>';
  }
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
  const titulo = v('materialTitulo');
  const tipo = v('materialTipo') || 'texto';
  const conteudo = v('materialConteudo');

  if (!titulo) { avisar('Informe o titulo do material.', 'erro'); return; }

  await comBotaoOcupado(botao, 'Adicionando...', async () => {
    try {
      await API.tutorAdicionarAula(idCurso, titulo, tipo, conteudo);
      avisar('Material adicionado.', 'sucesso');
      // Chamava carregarCursoDoTutor(), funcao que nao existe: a lista nunca
      // recarregava e o material novo so aparecia ao atualizar a pagina.
      await carregarEditorCurso();
    } catch (e) {
      avisar(e.mensagem || 'Nao foi possivel adicionar o material.', 'erro');
    }
  });
}

/**
 * Salva titulo, tipo e conteudo de um material ja cadastrado.
 *
 * Material ja assistido PODE ser editado (ao contrario de removido): corrigir
 * o texto de uma aula nao invalida progresso nem nota. Remover quebraria a
 * chave estrangeira com Progresso.
 */
async function salvarMaterial(idMaterial, botao) {
  const v = id => { const el = document.getElementById(id); return el ? el.value : ''; };
  const titulo = v('mTit' + idMaterial).trim();
  const tipo = v('mTipo' + idMaterial);
  const conteudo = v('mCont' + idMaterial);

  if (!titulo) { avisar('O titulo do material e obrigatorio.', 'erro'); return; }

  await comBotaoOcupado(botao, 'Salvando...', async () => {
    try {
      await API.tutorAtualizarAula(idMaterial, titulo, tipo, conteudo);
      avisar('Material salvo.', 'sucesso');
      await carregarEditorCurso();
    } catch (e) {
      avisar(e.mensagem || 'Nao foi possivel salvar o material.', 'erro');
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
      // Um endpoint so: o servidor cria a resposta enderecada a quem
      // perguntou e marca o original como resolvido, na mesma transacao.
      // Antes eram duas chamadas, e a segunda podia falhar deixando a duvida
      // aberta com a resposta ja enviada.
      await API.responderChamado(idChamado, texto);
      avisar('Resposta enviada. A duvida foi marcada como resolvida.', 'sucesso');
      fecharModal('modalResponder');
      setTimeout(() => window.location.reload(), 1500);
    } catch (e) {
      avisar(e.mensagem || 'Nao foi possivel enviar a resposta.', 'erro');
    }
  });
}

/** Sinaliza ao aluno que a duvida esta sendo analisada. */
async function marcarEmAndamento(idChamado, botao) {
  await comBotaoOcupado(botao, '...', async () => {
    try {
      await API.statusChamado(idChamado, 'Em andamento');
      avisar('Duvida marcada como em analise.', 'sucesso');
      setTimeout(() => window.location.reload(), 1200);
    } catch (e) {
      avisar(e.mensagem || 'Nao foi possivel alterar a situacao.', 'erro');
    }
  });
}

/** Marca a dúvida como respondida sem enviar mensagem. */
async function encerrarDuvida(idChamado, botao) {
  await comBotaoOcupado(botao, 'Encerrando...', async () => {
    try {
      await API.statusChamado(idChamado, 'Resolvido');
      avisar('Duvida encerrada.', 'sucesso');
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

  // A barra tinha width:75% escrito no style e o rotulo "75%" no HTML. Era a
  // divergencia que voce encontrou: a home mostrava 21% (o valor real) e o
  // perfil, 75% -- mesmo aluno, mesmo XP, duas telas discordando. O numero
  // real e ProgressoNivel, calculado pelo dominio: (XP % 350) / 350.
  texto('perfilProgressoLabel', p.progressoNivel + '%');
  const barraPerfil = document.getElementById('perfilProgressoBarra');
  if (barraPerfil) barraPerfil.style.width = p.progressoNivel + '%';

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

  // Os quatro cartoes do topo (7 realizadas, 5 aprovadas, 2 reprovadas, media
  // 7,8) eram numeros escritos no HTML. Saem das mesmas linhas listadas
  // abaixo, entao nao ha como a soma discordar da lista.
  const texto = (id, v) => { const el = document.getElementById(id); if (el) el.textContent = v; };
  const aprovadas = linhas.filter(l => (l.nota || 0) >= 7).length;
  const media = linhas.length
    ? (linhas.reduce((t, l) => t + (l.nota || 0), 0) / linhas.length)
    : 0;
  texto('provasTotal', linhas.length);
  texto('provasAprovadas', aprovadas);
  texto('provasReprovadas', linhas.length - aprovadas);
  texto('provasMedia', linhas.length ? media.toFixed(1).replace('.', ',') : '-');

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
      const paginaAula = 'aula.html';

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

    // Cartoes do topo: 12 materiais, 3 em andamento, 5 concluidos, 28h eram
    // numeros fixos no HTML. Agora contam a propria lista renderizada.
    const todos = cursos.filter(Boolean).flatMap(c => c.materiais || []);
    const feitos = todos.filter(a => a.concluido).length;
    const horas = cursos.filter(Boolean).reduce((t, c) => t + (c.duracaoHoras || 0), 0);
    const texto = (id, v) => { const el = document.getElementById(id); if (el) el.textContent = v; };
    texto('materiaisTotal', todos.length);
    texto('materiaisAndamento', todos.length - feitos);
    texto('materiaisConcluidos', feitos);
    texto('materiaisHoras', horas + 'h');

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

    <h3 class="section-title mt-lg">Materiais <span class="text-sm text-muted">(${aulas.length})</span></h3>
    <div id="listaAulasEditor">
      ${aulas.length ? aulas.map((a, i) => `
        <div class="card" style="padding:var(--space-md);">
          <div class="flex justify-between items-center mb-sm">
            <strong class="text-sm">${i + 1}. ${a.titulo || 'Material sem titulo'}</strong>
            <div class="flex gap-sm items-center">
              <span class="badge badge--${a.conteudo ? 'success' : 'warning'}">
                ${a.conteudo ? 'com conteudo' : 'sem conteudo'}</span>
              ${editavel ? `<button class="btn btn--ghost btn--sm"
                              style="color:var(--color-danger); border-color:var(--color-danger);"
                              onclick="removerAula(${a.id}, this)"><i class="ti ti-trash"></i></button>` : ''}
            </div>
          </div>
          <div class="form-group">
            <label class="form-label" for="mTit${a.id}">Titulo</label>
            <input type="text" class="form-input" id="mTit${a.id}"
                   value="${(a.titulo || '').replace(/"/g, '&quot;')}" ${editavel ? '' : 'disabled'}>
          </div>
          <div class="form-group">
            <label class="form-label" for="mTipo${a.id}">Tipo</label>
            <select class="form-input" id="mTipo${a.id}" ${editavel ? '' : 'disabled'}>
              ${['texto', 'exercicio'].map(t =>
                `<option value="${t}" ${a.tipo === t ? 'selected' : ''}>${t}</option>`).join('')}
              ${!['texto', 'exercicio'].includes(a.tipo)
                ? `<option value="${a.tipo}" selected>${a.tipo} (tipo antigo)</option>` : ''}
            </select>
          </div>
          <div class="form-group">
            <label class="form-label" for="mCont${a.id}">Conteudo do material</label>
            <textarea class="form-input" id="mCont${a.id}" rows="8"
                      placeholder="Escreva aqui o texto que o aluno vai ler. Linha em branco separa paragrafos; indente com quatro espacos para criar um bloco de codigo."
                      ${editavel ? '' : 'disabled'}>${(a.conteudo || '').replace(/</g, '&lt;')}</textarea>
          </div>
          ${editavel ? `<button class="btn btn--secondary btn--full btn--sm"
                          onclick="salvarMaterial(${a.id}, this)">
                          <i class="ti ti-device-floppy"></i> Salvar este material</button>` : ''}
        </div>`).join('')
        : '<div class="card"><p class="text-sm text-muted">Nenhum material cadastrado. Um curso sem material nao pode ser submetido.</p></div>'}
    </div>

    ${editavel ? `
    <div class="card mt-md">
      <h4 class="font-semibold mb-md">Adicionar material</h4>
      <div class="form-group">
        <label class="form-label" for="materialTitulo">Titulo</label>
        <input type="text" class="form-input" id="materialTitulo" placeholder="Ex: Apostila de heranca">
      </div>
      <div class="form-group">
        <label class="form-label" for="aulaTipo">Tipo</label>
        <!-- "PDF" saiu junto com "video", pelo mesmo motivo: a plataforma
             nao armazena arquivo. Um material marcado como PDF que entrega
             texto na tela promete o que o sistema nao cumpre. Volta quando
             houver armazenamento -- com arquivo. -->
        <select class="form-input" id="materialTipo">
          <option value="texto">Texto</option>
          <option value="exercicio">Lista de exercicios</option>
        </select>
      </div>
      <div class="form-group">
        <label class="form-label" for="materialConteudo">Conteudo (opcional agora)</label>
        <textarea class="form-input" id="materialConteudo" rows="6"
          placeholder="Pode deixar em branco e escrever depois."></textarea>
      </div>
      <button class="btn btn--secondary btn--full" onclick="adicionarAula(${curso.id}, this)">
        <i class="ti ti-plus"></i> Adicionar material</button>
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
      avisar('Material removido.', 'sucesso');
      carregarEditorCurso();
    } catch (e) {
      // O servidor recusa remover material que algum aluno ja estudou.
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

// -------------- 9.11 Conta comum as tres personas --------------
/**
 * Preenche /pages/conta/perfil.html. A mesma tela serve estudante, tutor e
 * administrador porque /api/perfil responde para qualquer papel -- duplicar
 * a pagina em tres pastas significaria corrigir cada ajuste tres vezes.
 */
async function carregarContaComum() {
  const texto = (id, v) => { const el = document.getElementById(id); if (el) el.textContent = v; };

  // O botao de voltar leva a home da propria area de quem esta logado.
  const voltar = document.getElementById('voltarArea');
  if (voltar) voltar.href = '../' + (API.papel || 'estudante') + '/home.html';

  try {
    const p = await API.perfil();
    texto('contaIniciais', p.iniciais);
    texto('contaAvatar', p.iniciais);
    texto('contaNome', p.nome);
    texto('contaEmail', p.email || '');
    texto('contaPapel', (p.papel || '').toUpperCase());
    texto('contaTelefone', p.telefone || 'nao informado');
    texto('contaCidade', p.cidade || 'nao informada');
    texto('contaDesde', FMT.data(p.dataCadastro));

    renderIndicadoresDaConta(p);
  } catch (e) {
    avisar(e.mensagem || 'Nao foi possivel carregar a conta.', 'erro');
  }
}

/**
 * Indicadores da conta, conforme o papel.
 *
 * A tela do estudante tem estatisticas proprias (provas, medalhas,
 * certificados, materiais) e a do tutor nao tinha nada -- so nome, e-mail e
 * data de cadastro. O tutor tem numeros equivalentes, e eles ja estao
 * carregados pelo adaptador; faltava mostra-los.
 *
 * NAO duplica a tela do estudante: estudante continua em
 * pages/estudante/perfil.html, que tem XP, nivel e atalhos que so fazem
 * sentido para quem estuda. Aqui ficam os numeros de quem ENSINA ou
 * ADMINISTRA.
 */
function renderIndicadoresDaConta(p) {
  const alvo = document.getElementById('contaIndicadores');
  if (!alvo) return;

  const cartao = (rotulo, valor, detalhe) => `
    <div class="card" style="flex:1; min-width:140px; margin:0; padding:var(--space-md);">
      <div class="text-xs text-muted font-semibold" style="letter-spacing:1px;">${rotulo}</div>
      <div class="font-bold" style="font-size:26px;">${valor}</div>
      ${detalhe ? `<div class="text-xs text-muted">${detalhe}</div>` : ''}
    </div>`;

  const papel = (API.papel || '').toLowerCase();

  if (papel === 'tutor') {
    const t = (TQ.data && TQ.data.personas && TQ.data.personas.tutor) || {};
    const duvidas = ((TQ.data && TQ.data.tutor_duvidas) || []).filter(d => d.pendente).length;
    alvo.innerHTML = `
      <h3 class="section-title mt-lg">Minha atuacao</h3>
      <div class="flex gap-sm" style="flex-wrap:wrap;">
        ${cartao('CURSOS', t.total_cursos || 0,
          `${t.cursos_publicados || 0} publicados, ${t.cursos_pendentes || 0} em avaliacao`)}
        ${cartao('MATERIAIS', t.total_materiais || 0, 'publicados nos seus cursos')}
        ${cartao('ALUNOS', t.alunos_ativos || 0, 'matriculados')}
        ${cartao('DUVIDAS', duvidas, 'aguardando resposta')}
      </div>`;
    return;
  }

  if (papel === 'admin') {
    alvo.innerHTML = `
      <h3 class="section-title mt-lg">Plataforma</h3>
      <p class="text-sm text-muted">Os numeros da plataforma estao no
        <a href="../admin/home.html" class="form-link">painel</a>.</p>`;
    return;
  }

  // Estudante que chegou aqui: a tela rica e outra.
  alvo.innerHTML = `
    <h3 class="section-title mt-lg">Meu progresso</h3>
    <div class="flex gap-sm" style="flex-wrap:wrap;">
      ${cartao('NIVEL', p.nivel, `${(p.xp || 0).toLocaleString('pt-BR')} XP`)}
      ${cartao('MATERIAIS', p.aulasConcluidas, 'concluidos')}
      ${cartao('PROVAS', p.provasAprovadas, 'aprovadas')}
      ${cartao('CERTIFICADOS', p.cursosConcluidos, 'cursos concluidos')}
    </div>
    <p class="text-sm text-muted mt-md">Seu perfil completo, com conquistas e historico, esta em
      <a href="../estudante/perfil.html" class="form-link">Perfil</a>.</p>`;
}

/** Filtro das abas de duvidas do tutor. */
function filtrarDuvidas(situacao, botao) {
  document.querySelectorAll('.tab').forEach(b => b.classList.remove('active'));
  if (botao) botao.classList.add('active');

  document.querySelectorAll('#duvidasList [data-situacao]').forEach(el => {
    el.style.display = (situacao === 'todas' || el.dataset.situacao === situacao) ? '' : 'none';
  });

  const vazio = document.getElementById('duvidasVazio');
  if (vazio) {
    const visiveis = [...document.querySelectorAll('#duvidasList [data-situacao]')]
      .filter(el => el.style.display !== 'none').length;
    vazio.style.display = visiveis ? 'none' : '';
  }
}

/** Busca local na lista de alunos do tutor. */
function filtrarAlunos() {
  const campo = document.getElementById('buscaAluno');
  const termo = campo ? campo.value.trim().toLowerCase() : '';

  document.querySelectorAll('#alunosTableBody [data-busca], #alunosTableBody tr[data-busca]')
    .forEach(el => {
      el.style.display = !termo || el.dataset.busca.includes(termo) ? '' : 'none';
    });
}

// -------------- 9.12 Suporte tecnico (administrador) --------------
/**
 * Fila de chamados tecnicos. Nao tem dono: qualquer administrador assume.
 * Duvidas de conteudo nao entram aqui -- vao direto ao tutor do curso.
 */
let CHAMADO_ATUAL = null;

async function carregarChamadosAdmin() {
  const lista = document.getElementById('chamadosList');
  if (!lista) return;

  let chamados;
  try {
    chamados = await API.adminChamados();
  } catch (e) {
    lista.innerHTML = '<div class="card"><p class="text-sm text-danger">' +
      (e.mensagem || 'Nao foi possivel carregar os chamados.') + '</p></div>';
    return;
  }

  if (!chamados.length) {
    lista.innerHTML = '<div class="card"><p class="text-sm text-muted">' +
      'Nenhum chamado tecnico registrado.</p></div>';
    return;
  }

  const CORES = { 'Aberto': 'danger', 'Em andamento': 'warning', 'Resolvido': 'success' };

  lista.innerHTML = chamados.map(c => {
    const resolvido = c.status === 'Resolvido';
    const cor = CORES[c.status] || 'neutral';
    const assunto = (c.assunto || '').replace(/'/g, "&#39;");
    const texto = (c.descricao || '').replace(/'/g, "&#39;").replace(/"/g, '&quot;');

    return `
    <div class="card" data-situacao="${resolvido ? 'resolvidos' : 'abertos'}"
         style="border-left:3px solid var(--color-${cor});">
      <div class="flex justify-between items-center mb-sm">
        <strong>#${c.id} - ${c.assunto || 'Sem assunto'}</strong>
        <span class="badge badge--${cor}">${(c.status || '').toUpperCase()}</span>
      </div>
      <div class="text-xs text-muted mb-sm">
        <i class="ti ti-user"></i> ${c.remetente} - ${FMT.relativo(c.dataAbertura)}
      </div>
      <p class="text-sm">${c.descricao || ''}</p>
      ${!resolvido ? `
      <div class="flex gap-sm mt-md">
        <button class="btn btn--primary btn--sm" style="flex:1;"
                onclick="abrirResolucao(${c.id}, '${assunto}', '${texto}')">
          <i class="ti ti-message-reply"></i> Responder e resolver
        </button>
        ${c.status === 'Aberto' ? `
        <button class="btn btn--ghost btn--sm" onclick="assumirChamado(${c.id}, this)">
          Assumir
        </button>` : ''}
      </div>` : ''}
    </div>`;
  }).join('');

  filtrarChamadosAdmin('abertos', document.querySelector('.tab[data-filtro="abertos"]'));
}

function filtrarChamadosAdmin(situacao, botao) {
  document.querySelectorAll('.tab').forEach(b => b.classList.remove('active'));
  if (botao) botao.classList.add('active');

  document.querySelectorAll('#chamadosList [data-situacao]').forEach(el => {
    el.style.display = (situacao === 'todos' || el.dataset.situacao === situacao) ? '' : 'none';
  });

  const vazio = document.getElementById('chamadosVazio');
  if (vazio) {
    const visiveis = [...document.querySelectorAll('#chamadosList [data-situacao]')]
      .filter(el => el.style.display !== 'none').length;
    vazio.style.display = visiveis ? 'none' : '';
  }
}

function abrirResolucao(id, assunto, descricao) {
  CHAMADO_ATUAL = id;
  const alvo = document.getElementById('chamadoOriginal');
  if (alvo) {
    alvo.innerHTML = '<strong>#' + id + ' - ' + assunto + '</strong>' +
      '<p class="text-sm mt-sm" style="margin:0;">' + descricao + '</p>';
  }
  const campo = document.getElementById('respostaChamado');
  if (campo) campo.value = '';
  abrirModal('modalResolver');
}

/** Sinaliza que um administrador esta cuidando do chamado. */
async function assumirChamado(id, botao) {
  await comBotaoOcupado(botao, '...', async () => {
    try {
      await API.statusChamado(id, 'Em andamento');
      avisar('Chamado marcado como em andamento.', 'sucesso');
      setTimeout(() => window.location.reload(), 1200);
    } catch (e) {
      avisar(e.mensagem || 'Nao foi possivel assumir o chamado.', 'erro');
    }
  });
}

async function responderChamadoTecnico(botao) {
  const campo = document.getElementById('respostaChamado');
  const texto = campo ? campo.value.trim() : '';
  if (!texto) { avisar('Escreva a resolucao antes de enviar.', 'erro'); return; }
  if (!CHAMADO_ATUAL) { avisar('Chamado nao identificado.', 'erro'); return; }

  await comBotaoOcupado(botao, 'Enviando...', async () => {
    try {
      await API.responderChamado(CHAMADO_ATUAL, texto);
      avisar('Resposta enviada. O chamado foi marcado como resolvido.', 'sucesso');
      fecharModal('modalResolver');
      setTimeout(() => window.location.reload(), 1500);
    } catch (e) {
      avisar(e.mensagem || 'Nao foi possivel responder.', 'erro');
    }
  });
}

// -------------- 9.13 Tela de material (antes "aula") --------------
/**
 * No PIM III aula.html era UM material escrito no HTML: titulo "Arrays e
 * Listas em C#", instrutora "Ana Souza", "1.247 visualizacoes", "12 min" e um
 * texto fixo sobre arrays. Abrir qualquer material do curso levava a essa
 * mesma pagina -- o titulo na lista vinha do banco, o conteudo nao.
 *
 * Agora a pagina e parametrizada por ?id=<curso>&aula=<material> e o texto sai
 * da coluna Material.Conteudo (05_conteudo_material.sql).
 *
 * O QUE SAIU, e por que: "visualizacoes" nao existe no modelo -- nao ha tabela
 * de acesso -- e "12 min" tambem nao: Material nao tem duracao, apenas o Curso
 * tem Duracao_Horas. Exibir numero inventado ao lado de numero real e pior que
 * nao exibir: quem olha nao sabe quais confiar.
 */
async function renderAula() {
  const alvo = document.getElementById('aulaConteudo');
  if (!alvo) return;

  const texto = (id, v) => { const el = document.getElementById(id); if (el) el.textContent = v; };

  let curso;
  try {
    curso = await CONTEXTO.curso();
  } catch (e) {
    alvo.innerHTML = '<div class="card"><p class="text-sm text-danger">' +
      (e.mensagem || 'Nao foi possivel carregar o material.') + '</p></div>';
    return;
  }
  if (!curso) { alvo.innerHTML = '<div class="card"><p class="text-sm text-danger">Curso nao encontrado.</p></div>'; return; }

  TQ.cursoAtual = curso;
  const materiais = curso.materiais || [];
  const idAula = CONTEXTO.parametro('aula');

  // Sem ?aula= na URL: abre onde o aluno parou. Mesma regra do botao
  // "Continuar" da home, para os dois caminhos levarem ao mesmo lugar.
  const atual = (idAula && materiais.find(m => m.id === idAula))
    || materiais.find(m => !m.concluido)
    || materiais[0];

  if (!atual) {
    texto('aulaTitulo', curso.nome);
    alvo.innerHTML = '<div class="card"><p class="text-sm text-muted">' +
      'Este curso ainda nao tem materiais publicados.</p></div>';
    return;
  }

  TQ.materialAtual = atual;
  const indice = materiais.findIndex(m => m.id === atual.id);

  document.title = (atual.titulo || 'Material') + ' - Tech Quest';
  texto('aulaTitulo', atual.titulo || 'Material sem titulo');
  texto('aulaCursoNome', curso.nome);
  texto('aulaTrilha', `${curso.nome} > material ${indice + 1} de ${materiais.length}`);
  texto('aulaTipo', (atual.tipo || 'material').toUpperCase());
  texto('aulaInstrutor', curso.instrutor || 'A definir');
  texto('aulaInstrutorIniciais', curso.instrutorIniciais || FMT.iniciais(curso.instrutor));
  texto('aulaNivel', (curso.nivel || 'sem nivel').toUpperCase());

  const selo = document.getElementById('aulaSeloConcluido');
  if (selo) selo.innerHTML = atual.concluido
    ? '<span class="badge badge--success"><i class="ti ti-circle-check"></i> CONCLUIDO</span>'
    : '';

  // Conteudo. Paragrafos separados por linha em branco; blocos indentados com
  // quatro espacos viram code-block, que e como o texto foi escrito no banco.
  alvo.innerHTML = atual.conteudo
    ? formatarConteudoMaterial(atual.conteudo)
    : '<div class="card" style="background:var(--color-warning-light); border-color:var(--color-warning);">' +
      '<p class="text-sm"><i class="ti ti-info-circle"></i> <strong>Este material ainda nao tem conteudo cadastrado.</strong> ' +
      'O tutor do curso escreve o texto pelo editor de curso.</p></div>';

  // Botao final: o PIM III dizia sempre "Marcar como concluido e ir para a
  // Prova", mesmo quando o passo seguinte era outro material. O destino agora
  // depende de onde o aluno esta na trilha.
  const area = document.getElementById('aulaAcoes');
  if (!area) return;

  const proximo = materiais[indice + 1] || null;
  const todosFeitos = materiais.every(m => m.concluido || m.id === atual.id);

  let destino, rotulo;
  if (proximo) {
    destino = `aula.html?id=${curso.id}&aula=${proximo.id}`;
    rotulo = 'Concluir e ir para "' + (proximo.titulo || 'proximo material') + '"';
  } else if (curso.idProva && todosFeitos) {
    destino = `prova.html?id=${curso.id}`;
    rotulo = 'Concluir e ir para a prova';
  } else {
    destino = `curso.html?id=${curso.id}`;
    rotulo = 'Concluir e voltar ao curso';
  }
  TQ.destinoPosConclusao = destino;

  area.innerHTML = atual.concluido
    ? `<a href="${destino}" class="btn btn--primary btn--full btn--lg mt-lg">
         ${proximo ? 'Proximo material' : (curso.idProva && todosFeitos ? 'Ir para a prova' : 'Voltar ao curso')}
         <i class="ti ti-arrow-right"></i></a>
       <p class="text-xs text-muted text-center mt-sm">Material ja concluido. Voce esta revendo o conteudo.</p>`
    : `<button class="btn btn--primary btn--full btn--lg mt-lg" onclick="concluirAula(this)">
         ${rotulo} <i class="ti ti-arrow-right"></i></button>
       <a href="curso.html?id=${curso.id}" class="btn btn--ghost btn--full mt-sm">
         <i class="ti ti-arrow-left"></i> Voltar ao curso sem concluir</a>`;
}

/**
 * Converte o texto do banco em HTML. Nao e markdown completo de proposito: o
 * conteudo e texto, e um interpretador de markdown aqui seria uma dependencia
 * nova para resolver o que quatro regras resolvem.
 */
function formatarConteudoMaterial(txt) {
  const esc = t => String(t)
    .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

  return String(txt).split(/\n\s*\n/).map(bloco => {
    const linhas = bloco.split('\n');
    // Bloco de codigo: toda linha nao vazia comeca com quatro espacos.
    const ehCodigo = linhas.filter(l => l.trim()).every(l => /^ {4}/.test(l));
    if (ehCodigo) {
      const codigo = linhas.map(l => l.replace(/^ {4}/, '')).join('\n').trim();
      return '<div class="code-block"><div class="code-block__header"><span>EXEMPLO</span></div>' +
             '<pre>' + escapeCode(codigo) + '</pre></div>';
    }
    return '<p style="line-height:1.7; margin-bottom:var(--space-md);">' +
           esc(bloco.trim()).replace(/\n/g, '<br>') + '</p>';
  }).join('');
}

// -------------- 9.14 Telas de resultado da prova --------------
/**
 * Serve resultado-aprovado.html e resultado-reprovado.html.
 *
 * O QUE ERA SIMULADO (e o que a banca vai perguntar): as duas telas liam
 * TQ.questoes.prova_csharp e inventavam quais questoes o aluno acertou --
 * "const correta = i !== 2" na aprovada, "i === 1" na reprovada. Com o
 * questoes.json fora do navegador, TQ.questoes ficou vazio e a revisao parou
 * de aparecer. Agora a correcao real vem na resposta da submissao, questao por
 * questao, com a letra marcada e a letra correta.
 *
 * "ANALISE POR TOPICO" FOI REMOVIDA, nao corrigida. Os tres topicos ("Logica
 * de Programacao 40%", "Estrutura de Dados 65%", "Algoritmos Avancados 20%")
 * nao tem lastro: a tabela Questao nao tem coluna de assunto, e sem isso nao
 * existe de onde agrupar acertos por tema. Criar a coluna agora significaria
 * pedir ao tutor para classificar cada questao, decisao de modelagem que nao
 * foi feita. Em lugar dela a tela mostra o que o banco sabe: acertos, nota,
 * nota minima, tentativa e XP.
 */
function renderResultadoProva() {
  const raw = sessionStorage.getItem('tq_resultado');
  const alvo = document.getElementById('revisaoProva');

  if (!raw) {
    if (alvo) {
      alvo.innerHTML = '<div class="card"><p class="text-sm text-muted">' +
        'Nenhum resultado nesta sessao. <a href="cursos.html" class="text-primary font-semibold">' +
        'Voltar aos cursos</a>.</p></div>';
    }
    return;
  }

  const r = JSON.parse(raw);
  const texto = (id, v) => { const el = document.getElementById(id); if (el) el.textContent = v; };
  const aluno = (TQ.data && TQ.data.personas && TQ.data.personas.estudante) || {};

  texto('resultadoNome', (aluno.nome || '').split(' ')[0] || 'estudante');
  texto('resultadoProva', r.prova_titulo || 'Prova');
  texto('notaFinal', String(r.nota).replace('.', ',') + ' / 10');
  texto('acertosTotal', r.acertos + ' de ' + r.total);
  texto('notaMinima', r.nota_minima != null
    ? String(r.nota_minima).replace('.', ',') : '7,0');
  texto('tentativaNumero', r.tentativa);
  texto('xpGanho', '+' + (r.xp_ganho || 0) + ' XP');
  texto('totalQuestoes', r.total + (r.total === 1 ? ' questao' : ' questoes'));

  // Nivel e XP do cartao de gamificacao: valor real do perfil, nao "Nivel 12".
  if (aluno.nivel) {
    texto('resultadoNivel', 'Progresso do nivel ' + aluno.nivel);
    texto('resultadoXpAtual', (aluno.xp || 0).toLocaleString('pt-BR') + ' XP');
    texto('resultadoXpProximo',
      (aluno.xp_proximo_nivel || 0).toLocaleString('pt-BR') + ' XP para o nivel ' + (aluno.nivel + 1));
    const barra = document.getElementById('resultadoBarraNivel');
    if (barra) barra.style.width = (aluno.progresso_nivel || 0) + '%';
  }

  // Medalhas: so aparece o cartao se o servidor realmente concedeu alguma.
  const areaMedalha = document.getElementById('resultadoMedalhas');
  if (areaMedalha) {
    const novas = r.medalhas_novas || [];
    areaMedalha.innerHTML = novas.length
      ? `<div class="card card--accent-warning animate-in">
           <div class="flex items-center gap-md">
             <div class="list-item__icon list-item__icon--warning" style="width:48px; height:48px;">
               <i class="ti ti-medal" style="font-size:24px;"></i></div>
             <div>
               <div class="text-sm text-warning font-semibold">Nova conquista desbloqueada</div>
               <div class="font-bold">${novas.join(', ')}</div>
             </div>
           </div>
         </div>`
      : '';
  }

  if (!alvo) return;

  const correcao = r.correcao || [];
  const porId = {};
  (r.questoes || []).forEach(q => { porId[q.id] = q; });

  if (!correcao.length) {
    alvo.innerHTML = '<p class="text-sm text-muted">O servidor nao devolveu a correcao desta tentativa.</p>';
    return;
  }

  alvo.innerHTML = correcao.map((c, i) => {
    const q = porId[c.idQuestao] || {};
    const alts = q.alternativas || {};
    const cor = c.acertou ? 'success' : 'danger';

    const linhaResposta = (letra, rotulo, classe) => letra
      ? `<div class="text-xs" style="background:var(--color-${classe}-light); padding:6px 10px;
             border-radius:6px; margin-bottom:4px;">
           <strong>${classe === 'success' ? '✓' : '✗'} ${letra})</strong>
           ${alts[letra] || '(alternativa nao disponivel)'} — ${rotulo}
         </div>`
      : '';

    return `
    <div class="card" style="border-left:3px solid var(--color-${cor}); margin-bottom:var(--space-sm);
         padding:var(--space-md);">
      <div class="flex justify-between items-center mb-sm">
        <strong class="text-sm">Questao ${String(c.ordem || i + 1).padStart(2, '0')}</strong>
        <span class="badge badge--${cor}">${c.acertou ? 'CORRETA' : 'INCORRETA'}</span>
      </div>
      <p class="text-sm mb-sm">${q.enunciado || '(enunciado nao disponivel nesta sessao)'}</p>
      ${q.codigo ? `<div class="code-block"><div class="code-block__header"><span>EXEMPLO</span></div>
          <pre>${escapeCode(q.codigo)}</pre></div>` : ''}
      ${c.acertou
        ? linhaResposta(c.letraCorreta, 'sua resposta, correta', 'success')
        : linhaResposta(c.letraMarcada, 'sua resposta', 'danger') +
          linhaResposta(c.letraCorreta, 'resposta correta', 'success')}
    </div>`;
  }).join('');
}

// -------------- 9.15 Conquistas --------------
/**
 * Os dois cartoes do topo ("12 Provas realizadas", "5 Conquistas
 * desbloqueadas") e as contagens das abas ("Conquistadas (5)",
 * "Bloqueadas (7)") eram numeros escritos no HTML. Com uma medalha de fato
 * conquistada, a tela dizia cinco.
 *
 * O render das medalhas estava no <script> da pagina. Trouxe para ca porque
 * contagem e lista precisam sair da MESMA fonte -- foi a separacao que deixou
 * os numeros divergirem.
 */
let FILTRO_MEDALHAS = 'todas';

async function carregarConquistas() {
  const grid = document.getElementById('medalhasGrid');
  if (!grid) return;

  const medalhas = (TQ.data && TQ.data.medalhas) || [];
  const texto = (id, v) => { const el = document.getElementById(id); if (el) el.textContent = v; };

  const conquistadas = medalhas.filter(m => m.status === 'conquistada').length;
  const bloqueadas = medalhas.length - conquistadas;

  let provas = 0;
  try { provas = (await API.perfil()).provasAprovadas; } catch { /* mantem zero */ }

  texto('conquistasProvas', provas);
  texto('conquistasMedalhas', conquistadas);
  texto('abaConquistadas', `Conquistadas (${conquistadas})`);
  texto('abaBloqueadas', `Bloqueadas (${bloqueadas})`);

  renderMedalhasGrid();
}

function filtrarMedalhas(filtro, botao) {
  FILTRO_MEDALHAS = filtro;
  document.querySelectorAll('.tabs .tab').forEach(t => t.classList.remove('active'));
  if (botao) botao.classList.add('active');
  renderMedalhasGrid();
}

function renderMedalhasGrid() {
  const grid = document.getElementById('medalhasGrid');
  if (!grid) return;

  let medalhas = (TQ.data && TQ.data.medalhas) || [];
  if (FILTRO_MEDALHAS === 'conquistadas') medalhas = medalhas.filter(m => m.status === 'conquistada');
  if (FILTRO_MEDALHAS === 'bloqueadas') medalhas = medalhas.filter(m => m.status === 'bloqueada');

  if (!medalhas.length) {
    grid.innerHTML = '<div class="card"><p class="text-sm text-muted">' +
      (FILTRO_MEDALHAS === 'conquistadas'
        ? 'Voce ainda nao desbloqueou nenhuma medalha. Conclua materiais e provas para comecar.'
        : 'Nenhuma medalha nesta situacao.') + '</p></div>';
    return;
  }

  grid.innerHTML = medalhas.map(m => {
    const aberta = m.status === 'conquistada';
    return `<div class="achievement-card ${aberta ? 'achievement-card--unlocked' : 'achievement-card--locked'}">
      <div class="achievement-card__icon"><i class="ti ${aberta ? m.icone : 'ti-lock'}"></i></div>
      <div class="achievement-card__title">${m.titulo}</div>
      ${aberta
        ? `<div class="text-xs text-success font-semibold" style="letter-spacing:0.5px;">CONQUISTADA</div>
           <div class="achievement-card__date">${m.data || ''}</div>`
        : `<div class="text-xs text-muted">${m.descricao || ''}</div>`}
    </div>`;
  }).join('');
}

// -------------- 9.16 Certificados --------------
/**
 * Os quatro cartoes do topo (1 emitido, 1 disponivel, 2 bloqueados, 12h) eram
 * numeros fixos, e a lista abaixo ficava vazia -- o que voce viu no print.
 * Havia um segundo defeito escondido: o render usava c.nota_final, campo que a
 * API nao devolve, e um unico certificado emitido faria a tela estourar
 * TypeError e nao renderizar nada.
 *
 * "NOTA FINAL DO CURSO" NAO EXISTE NO MODELO. Desempenho guarda a nota de cada
 * TENTATIVA de prova; nao ha media de curso em nenhuma tabela. O certificado
 * do PIM III imprimia "nota final 8,5" -- numero que nada sustenta. Saiu.
 *
 * "Bloqueado" tambem saiu: pressupoe pre-requisito entre cursos, que o modelo
 * nao tem. O que existe e: curso concluido (certificado emitido) e curso em
 * andamento (certificado a emitir), e e isso que a tela mostra.
 */
async function carregarCertificados() {
  const grid = document.getElementById('certificadosGrid');
  if (!grid) return;

  let certs = [], historico = [];
  try {
    [certs, historico] = await Promise.all([API.certificados(), API.historico()]);
  } catch (e) {
    grid.innerHTML = '<div class="card"><p class="text-sm text-danger">' +
      (e.mensagem || 'Nao foi possivel carregar seus certificados.') + '</p></div>';
    return;
  }

  const emAndamento = historico.filter(h => h.status !== 'Concluido');
  const horas = certs.reduce((t, c) => t + (c.cargaHoraria || 0), 0);

  const texto = (id, v) => { const el = document.getElementById(id); if (el) el.textContent = v; };
  texto('certEmitidos', certs.length);
  texto('certAndamento', emAndamento.length);
  texto('certHoras', horas + 'h');

  const cartaoEmitido = c => `
    <div class="course-card">
      <div class="course-card__cover course-card__cover--data"
           style="background:linear-gradient(135deg, #15803D 0%, #166534 100%);">
        <i class="ti ti-certificate course-card__cover-icon"></i>
        <span class="course-card__level"><i class="ti ti-check"></i> EMITIDO</span>
        <span class="course-card__name">${c.curso}</span>
      </div>
      <div class="course-card__body">
        <div class="text-xs text-muted mb-sm">Emitido em <strong>${FMT.data(c.dataEmissao)}</strong></div>
        <div class="text-xs text-muted mb-md">Codigo:
          <strong style="font-family:monospace;">${c.codigoAutenticacao}</strong></div>
        <div class="course-card__meta">
          <span><i class="ti ti-clock"></i> ${c.cargaHoraria || 0}h</span>
          <span><i class="ti ti-user"></i> ${c.instrutor || 'tutor nao informado'}</span>
        </div>
        <a href="certificado-detalhe.html?codigo=${c.codigoAutenticacao}" class="btn btn--success btn--full">
          <i class="ti ti-eye"></i> Ver certificado</a>
      </div>
    </div>`;

  const cartaoAndamento = h => `
    <div class="course-card">
      <div class="course-card__cover" style="background:linear-gradient(135deg, #F59E0B 0%, #B45309 100%);">
        <i class="ti ti-certificate course-card__cover-icon"></i>
        <span class="course-card__level"><i class="ti ti-clock"></i> EM ANDAMENTO</span>
        <span class="course-card__name">${h.curso}</span>
      </div>
      <div class="course-card__body">
        <p class="course-card__desc">O certificado e emitido automaticamente quando voce concluir
          todos os materiais e for aprovado na prova do curso.</p>
        <div class="course-card__progress">
          <div class="course-card__progress-label">
            <span class="text-secondary">${h.aulasConcluidas} de ${h.totalAulas} materiais</span>
            <span class="font-semibold">${h.percentualCurso}%</span>
          </div>
          <div class="progress progress--warning">
            <div class="progress__fill" style="width:${h.percentualCurso}%"></div></div>
        </div>
        <a href="curso.html?id=${h.idCurso}" class="btn btn--ghost btn--full">
          <i class="ti ti-player-play"></i> Continuar o curso</a>
      </div>
    </div>`;

  let itens = [];
  if (FILTRO_CERT !== 'andamento') itens = itens.concat(certs.map(cartaoEmitido));
  if (FILTRO_CERT !== 'emitido') itens = itens.concat(emAndamento.map(cartaoAndamento));

  grid.innerHTML = itens.length
    ? itens.join('')
    : '<div class="card"><p class="text-sm text-muted">' +
      (certs.length || emAndamento.length
        ? 'Nenhum certificado nesta situacao.'
        : 'Voce ainda nao esta matriculado em nenhum curso. ' +
          '<a href="cursos.html" class="text-primary font-semibold">Ver catalogo</a>') + '</p></div>';
}

let FILTRO_CERT = 'todos';
function filtrarCertificados(filtro, botao) {
  FILTRO_CERT = filtro;
  document.querySelectorAll('.tabs .tab').forEach(t => t.classList.remove('active'));
  if (botao) botao.classList.add('active');
  carregarCertificados();
}

/**
 * Certificado individual. Antes a tela procurava o id 'cert-001' no mock e
 * imprimia nota final e instrutora fixas. Agora busca pelo CODIGO DE
 * AUTENTICACAO, que e a chave publica do certificado -- a mesma que o
 * endpoint de validacao usa, e a que esta impressa no documento.
 */
async function carregarCertificadoDetalhe() {
  const alvo = document.getElementById('certificadoConteudo');
  if (!alvo) return;

  const codigo = new URLSearchParams(window.location.search).get('codigo');
  const texto = (id, v) => { const el = document.getElementById(id); if (el) el.textContent = v; };

  let cert = null;
  try {
    if (codigo) {
      cert = await API.validarCertificado(codigo);
    } else {
      const lista = await API.certificados();
      cert = lista[0] || null;
    }
  } catch (e) {
    alvo.innerHTML = '<div class="card"><p class="text-sm text-danger">' +
      (e.mensagem || 'Nao foi possivel carregar o certificado.') + '</p></div>';
    return;
  }

  if (!cert) {
    alvo.innerHTML = '<div class="card"><p class="text-sm text-muted">' +
      'Nenhum certificado emitido ainda. Conclua um curso para receber o seu.</p></div>';
    const acoes = document.getElementById('acoesCert');
    if (acoes) acoes.innerHTML = '';
    return;
  }

  texto('codigoValidacao', cert.codigoAutenticacao);
  texto('urlValidacao', cert.codigoAutenticacao);

  alvo.innerHTML = `
    <div class="certificate-wrapper">
      <div class="certificate-seal"><i class="ti ti-award"></i></div>
      <div class="certificate-title">Tech Quest - Plataforma de Aprendizagem</div>
      <h1 class="certificate-heading">Certificado de Conclusao</h1>
      <p class="certificate-text">Certificamos que</p>
      <div class="certificate-name">${cert.estudante}</div>
      <p class="certificate-text">concluiu com aproveitamento o curso de</p>
      <div class="certificate-course">${cert.curso}</div>
      <p class="certificate-text">com carga horaria total de
        <strong>${cert.cargaHoraria || 0} horas</strong>.</p>
      <div class="certificate-meta">
        <div><strong>${FMT.data(cert.dataEmissao)}</strong>Data de emissao</div>
        <div><strong>${cert.instrutor || 'tutor nao informado'}</strong>Tutor responsavel</div>
        <div><strong style="font-family:monospace; font-size:11px;">${cert.codigoAutenticacao}</strong>
          Codigo de validacao</div>
      </div>
    </div>`;
}

// -------------- 9.16b Notificacoes --------------
/**
 * A tela de notificacoes era um estado vazio permanente: "Tudo em dia por
 * aqui", sempre, para todo mundo. O sino da barra superior nunca levava a
 * nada.
 *
 * NAO EXISTE TABELA DE NOTIFICACAO no modelo, e criar uma seria duplicar o
 * que Chamado ja guarda: as mensagens que chegam ao usuario sao as respostas
 * do tutor e do administrador aos chamados que ele abriu, com remetente,
 * assunto, texto e data. Entao a tela lista isso -- a caixa de entrada que o
 * modelo tem -- em vez de uma caixa inventada.
 *
 * O que ficaria faltando com esta decisao: aviso de medalha nova e de
 * certificado emitido, que hoje aparecem no momento da acao (na tela de
 * resultado e na de material concluido) e nao ficam guardados. Guardar
 * exigiria tabela de notificacao -- decisao de modelagem em aberto.
 */
async function carregarNotificacoes() {
  const alvo = document.getElementById('listaNotificacoes');
  if (!alvo) return;

  const vazio = document.getElementById('notificacoesVazio');
  const eu = API.usuario ? API.usuario.id : null;

  let chamados;
  try {
    chamados = await API.chamados();
  } catch (e) {
    alvo.innerHTML = '<div class="card"><p class="text-sm text-danger">' +
      (e.mensagem || 'Nao foi possivel carregar suas notificacoes.') + '</p></div>';
    return;
  }

  // Recebidos: o que foi enderecado a mim. O que eu enviei esta em suporte.
  const recebidos = chamados
    .filter(c => c.idDestinatario === eu && c.idRemetente !== eu)
    .sort((a, b) => new Date(b.dataAbertura) - new Date(a.dataAbertura));

  if (!recebidos.length) {
    if (vazio) vazio.style.display = '';
    alvo.innerHTML = '';
    return;
  }
  if (vazio) vazio.style.display = 'none';

  alvo.innerHTML = recebidos.map(c => `
    <div class="list-item">
      <div class="list-item__icon list-item__icon--${c.tipo === 'Resposta' ? 'success' : 'warning'}">
        <i class="ti ti-${c.tipo === 'Resposta' ? 'message-circle-check' : 'message-circle'}"></i>
      </div>
      <div class="list-item__body">
        <div class="list-item__title">${c.assunto || c.tipo || 'Mensagem'}</div>
        <div class="list-item__subtitle">${(c.descricao || '').slice(0, 120)}</div>
        <div class="text-xs text-muted mt-sm">
          ${c.remetente || 'Tech Quest'}${c.curso ? ' - ' + c.curso : ''} - ${FMT.relativo(c.dataAbertura)}
        </div>
      </div>
    </div>`).join('');
}

// -------------- 9.17 Painel do administrador (home) --------------
/**
 * A home do admin tinha QUATRO indicadores escritos no HTML (1.487 usuarios,
 * 924 ativos, 12 cursos, 3.421 provas) e um grafico de "Taxa de Aprovacao por
 * Curso" com quatro barras fixas, inclusive de cursos que nao existem no banco
 * ("Design Patterns", "POO com C#"). Nenhum tocava a API: carregarPainelAdmin
 * ja existia e so era chamada pela tela de relatorios, e os ids nao casavam.
 *
 * O grafico de taxa de aprovacao por curso FOI REMOVIDO: Desempenho guarda a
 * nota por tentativa e por estudante, e agregar isso por curso exige uma
 * consulta que a API nao expoe. Em lugar dele entram os numeros que o resumo
 * devolve -- matriculas, certificados, chamados abertos --, que sao medicoes
 * reais da plataforma.
 */
async function carregarHomeAdmin() {
  const texto = (id, v) => { const el = document.getElementById(id); if (el) el.textContent = v; };

  const u = (TQ.data && TQ.data.personas && TQ.data.personas.admin) || {};
  texto('adminNome', (u.nome || '').split(' ')[0] || 'administrador');
  texto('adminIniciais', u.iniciais || '--');

  let r;
  try {
    r = await API.adminResumo();
  } catch (e) {
    avisar(e.mensagem || 'Nao foi possivel carregar o painel.', 'erro');
    return;
  }

  texto('kpiUsuarios', r.totalUsuarios);
  texto('kpiAtivos', r.usuariosAtivos);
  texto('kpiAtivosDetalhe', r.totalUsuarios
    ? Math.round(r.usuariosAtivos * 100 / r.totalUsuarios) + '% da base'
    : 'sem usuarios');
  texto('kpiEstudantes', r.estudantes);
  texto('kpiTutores', r.tutores);
  texto('kpiCursosPublicados', r.cursosPublicados);
  texto('kpiCursosPendentes', r.cursosPendentes);
  texto('kpiMatriculas', r.totalMatriculas);
  texto('kpiCertificados', r.certificadosEmitidos);
  texto('kpiChamados', r.chamadosAbertos);

  // O selo de aprovacoes na barra lateral dizia "4" em todas as telas do
  // admin. Agora reflete a fila real -- e desaparece quando ela esta vazia.
  document.querySelectorAll('.js-selo-aprovacoes').forEach(el => {
    el.textContent = r.cursosPendentes;
    el.style.display = r.cursosPendentes ? '' : 'none';
  });

  const atalho = document.getElementById('atalhoAprovacoes');
  if (atalho) {
    atalho.innerHTML = r.cursosPendentes
      ? `<div class="font-bold">${r.cursosPendentes} ${r.cursosPendentes === 1
            ? 'curso aguardando aprovacao' : 'cursos aguardando aprovacao'}</div>
         <div class="text-sm text-secondary">Enviados por tutores e pendentes de avaliacao</div>`
      : `<div class="font-bold">Nenhuma aprovacao pendente</div>
         <div class="text-sm text-secondary">A fila de cursos submetidos esta vazia</div>`;
  }

  const lista = document.getElementById('cadastrosList');
  if (lista) {
    const usuarios = (TQ.data && TQ.data.admin_usuarios) || [];
    lista.innerHTML = usuarios.length
      ? usuarios.slice(0, 5).map(x => `
        <a href="usuario-detalhe.html?id=${x.id}" class="list-item">
          <div class="avatar avatar--sm">${FMT.iniciais(x.nome)}</div>
          <div class="list-item__body">
            <div class="list-item__title">${x.nome}</div>
            <div class="list-item__subtitle">${x.tipo} - cadastrado em ${x.data_cadastro}</div>
          </div>
          <span class="badge badge--${x.status === 'ativo' ? 'success' : 'neutral'}">
            ${(x.status || '').toUpperCase()}</span>
        </a>`).join('')
      : '<p class="text-sm text-muted">Nenhum usuario cadastrado.</p>';
  }
}

/**
 * Iniciais do usuario logado em todo avatar da interface.
 *
 * As 47 paginas tinham as iniciais escritas no HTML -- "FA" nas telas de
 * estudante, "CM" nas de admin, "AS" nas de tutor. Quem entrasse com outro
 * usuario veria as iniciais do Felipe. E uma unica classe (.js-iniciais) em
 * vez de um id por pagina: id tem de ser unico no documento, e a home tinha
 * DOIS avatares com id="userInitials" -- so o primeiro era preenchido.
 */
function preencherIniciais() {
  if (typeof API === 'undefined' || !API.autenticado) return;
  const u = API.usuario;
  if (!u) return;
  const iniciais = u.iniciais || FMT.iniciais(u.nome);
  document.querySelectorAll('.js-iniciais, #userInitials')
    .forEach(el => { el.textContent = iniciais; });
}

/**
 * Selo de aprovacoes pendentes na barra lateral do administrador.
 *
 * O numero 4 estava escrito no HTML das NOVE telas do admin. Como e um
 * elemento de navegacao -- aparece em toda tela da area --, o preenchimento
 * fica no init e nao em cada pagina.
 */
async function preencherSeloAprovacoes() {
  const selos = document.querySelectorAll('.js-selo-aprovacoes');
  if (!selos.length) return;
  if (typeof API === 'undefined' || API.papel !== 'admin') return;

  try {
    const r = await API.adminResumo();
    selos.forEach(el => {
      el.textContent = r.cursosPendentes;
      el.style.display = r.cursosPendentes ? '' : 'none';
    });
  } catch {
    // Sem resumo o selo desaparece: numero desconhecido e pior que nenhum.
    selos.forEach(el => { el.style.display = 'none'; });
  }
}

/** Selo de duvidas pendentes na barra lateral do tutor (dizia 2, fixo). */
function preencherSeloDuvidas() {
  const selos = document.querySelectorAll('.js-selo-duvidas');
  if (!selos.length) return;
  const duvidas = (TQ.data && TQ.data.tutor_duvidas) || [];
  const pendentes = duvidas.filter(d => d.pendente).length;
  selos.forEach(el => {
    el.textContent = pendentes;
    el.style.display = pendentes ? '' : 'none';
  });
}

/**
 * Versao exibida na interface.
 *
 * Tres telas diziam "2.4.1 Build 105" -- numero escrito a mao no HTML, que
 * nao corresponde a nenhuma versao real do projeto e que precisaria ser
 * corrigido em tres lugares. Fica em uma constante so, e o rotulo diz o que
 * de fato esta rodando.
 */
const VERSAO_APP = 'PIM IV - 1.0 (web integrado a API .NET 8)';

function preencherVersao() {
  document.querySelectorAll('#versaoApp, .js-versao')
    .forEach(el => { el.textContent = VERSAO_APP; });
}

// -------------- 9.18 Revisao de curso pelo administrador --------------
/**
 * Tela onde o administrador LE o curso antes de publicar.
 *
 * O QUE EXISTIA: a fila de aprovacao mostrava nome, tutor, descricao e duas
 * contagens -- "2 aulas, 3 questoes" -- com os botoes Aprovar e Rejeitar.
 * Nao havia como ver o conteudo dos materiais nem as questoes da prova.
 * Aprovar publica o curso para todos os estudantes; fazer isso sem ler o que
 * se publica esvazia a etapa de avaliacao, que e a razao de o fluxo ter o
 * estado Pendente.
 *
 * O GABARITO APARECE AQUI, de proposito: avaliar uma prova e justamente
 * conferir se a resposta marcada como correta e a correta. Quem recebe sem
 * gabarito e o estudante -- essa separacao esta nos DTOs, nao num parametro.
 */
let CURSO_EM_REVISAO = null;

async function carregarRevisaoCurso() {
  const alvo = document.getElementById('cursoRevisao');
  if (!alvo) return;

  const id = CONTEXTO.parametro('id');
  if (!id) {
    alvo.innerHTML = '<div class="card"><p class="text-sm text-muted">Nenhum curso informado. ' +
      'Volte para <a href="aprovacoes.html" class="form-link">Aprovacoes</a>.</p></div>';
    return;
  }

  let c;
  try {
    c = await API.adminCurso(id);
  } catch (e) {
    alvo.innerHTML = '<div class="card"><p class="text-sm text-danger">' +
      (e.mensagem || 'Nao foi possivel carregar o curso.') + '</p></div>';
    return;
  }

  CURSO_EM_REVISAO = c;
  const esc = t => String(t == null ? '' : t)
    .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

  const cores = { publicado: 'success', pendente: 'warning', rascunho: 'neutral', rejeitado: 'danger' };
  const estado = (c.status || '').toLowerCase();

  // Sinaliza o que impede a publicacao, antes de o avaliador procurar.
  const impedimentos = [];
  if (!c.materiais.length) impedimentos.push('nao tem nenhum material');
  if (c.materiais.some(m => !m.conteudo)) {
    const n = c.materiais.filter(m => !m.conteudo).length;
    impedimentos.push(n === 1 ? '1 material sem conteudo escrito'
                              : n + ' materiais sem conteudo escrito');
  }
  if (!c.prova) impedimentos.push('nao tem prova');
  else if (!c.prova.questoes.length) impedimentos.push('a prova nao tem questoes');
  else {
    const semGabarito = c.prova.questoes.filter(q => !q.alternativas.some(a => a.ehCorreta)).length;
    if (semGabarito) impedimentos.push(semGabarito + ' questao(oes) sem alternativa correta marcada');
  }

  const materiais = c.materiais.length
    ? c.materiais.map((m, i) => `
      <div class="card" style="padding:var(--space-md);">
        <div class="flex justify-between items-center mb-sm">
          <strong class="text-sm">${i + 1}. ${esc(m.titulo)}</strong>
          <span class="badge badge--${m.conteudo ? 'neutral' : 'danger'}">
            ${esc(m.tipo || 'material')}${m.conteudo ? '' : ' - SEM CONTEUDO'}</span>
        </div>
        ${m.conteudo
          ? `<div style="max-height:260px; overflow:auto; background:var(--color-surface-2);
                 padding:var(--space-md); border-radius:var(--radius-md);">
               ${formatarConteudoMaterial(m.conteudo)}
             </div>`
          : '<p class="text-sm text-danger" style="margin:0;">O aluno abriria este material e nao ' +
            'encontraria texto algum.</p>'}
      </div>`).join('')
    : '<div class="card"><p class="text-sm text-danger">Nenhum material cadastrado.</p></div>';

  const prova = c.prova
    ? `<div class="card">
         <div class="flex justify-between items-center mb-md">
           <strong>${esc(c.prova.titulo)}</strong>
           <span class="text-sm text-muted">nota minima
             ${String(c.prova.notaMinima).replace('.', ',')} - ${c.prova.tempoMinutos} min</span>
         </div>
         ${c.prova.questoes.map(q => `
           <div style="border-left:3px solid var(--color-border); padding-left:var(--space-md);
                margin-bottom:var(--space-lg);">
             <div class="text-xs text-muted font-semibold mb-sm">QUESTAO ${q.ordem}</div>
             <p class="text-sm mb-sm">${esc(q.enunciado)}</p>
             ${q.codigoExemplo
               ? `<div class="code-block"><div class="code-block__header"><span>EXEMPLO</span></div>
                    <pre>${escapeCode(q.codigoExemplo)}</pre></div>`
               : ''}
             ${q.alternativas.map(a => `
               <div class="text-sm" style="padding:4px 8px; border-radius:6px; margin-bottom:3px;
                    ${a.ehCorreta ? 'background:var(--color-success-light); font-weight:600;' : ''}">
                 ${a.ehCorreta ? '<i class="ti ti-check text-success"></i>' : '<span style="opacity:.3;">○</span>'}
                 <strong>${esc(a.letra)})</strong> ${esc(a.texto)}
               </div>`).join('')}
           </div>`).join('')}
       </div>`
    : '<div class="card"><p class="text-sm text-danger">Este curso nao tem prova. ' +
      'Sem prova, o aluno conclui o curso apenas lendo os materiais.</p></div>';

  const acoes = estado === 'pendente'
    ? `<div class="flex gap-sm mt-lg" style="flex-wrap:wrap;">
         <button class="btn btn--primary" style="flex:1; min-width:200px;"
                 onclick="aprovarCurso(${c.idCurso}, this)">
           <i class="ti ti-check"></i> Aprovar e publicar</button>
         <button class="btn btn--ghost" style="flex:1; min-width:200px;
                 color:var(--color-danger); border-color:var(--color-danger);"
                 onclick="abrirModal('modalRejeitarRevisao')">
           <i class="ti ti-x"></i> Rejeitar</button>
       </div>`
    : `<p class="text-sm text-muted mt-lg">Curso <strong>${esc(c.status)}</strong>:
         nao ha acao de avaliacao pendente. Esta tela serve tambem para auditar
         curso ja publicado.</p>`;

  alvo.innerHTML = `
    <div class="card">
      <div class="flex justify-between items-center mb-md" style="flex-wrap:wrap; gap:var(--space-sm);">
        <h2 style="margin:0; font-size:20px;">${esc(c.nome)}</h2>
        <span class="badge badge--${cores[estado] || 'neutral'}">${esc((c.status || '').toUpperCase())}</span>
      </div>
      <p class="text-sm mb-md">${esc(c.descricao) || '<em>Sem descricao.</em>'}</p>
      <div class="flex justify-between mb-sm"><span class="text-secondary">Tutor</span>
        <span class="font-medium">${esc(c.tutor) || 'nao identificado'}</span></div>
      <div class="flex justify-between mb-sm"><span class="text-secondary">Categoria</span>
        <span class="font-medium">${esc(c.categoria) || '-'}</span></div>
      <div class="flex justify-between mb-sm"><span class="text-secondary">Nivel</span>
        <span class="font-medium">${esc(c.nivel) || '-'}</span></div>
      <div class="flex justify-between"><span class="text-secondary">Carga horaria</span>
        <span class="font-medium">${c.duracaoHoras || 0} horas</span></div>
    </div>

    ${impedimentos.length
      ? `<div class="card" style="background:var(--color-danger-light); border-color:var(--color-danger);">
           <strong class="text-sm text-danger"><i class="ti ti-alert-triangle"></i>
             Pontos de atencao antes de publicar</strong>
           <ul class="text-sm" style="margin:8px 0 0 18px;">
             ${impedimentos.map(x => '<li>' + x + '</li>').join('')}
           </ul>
         </div>`
      : `<div class="card" style="background:var(--color-success-light); border-color:var(--color-success);">
           <p class="text-sm" style="margin:0;"><i class="ti ti-circle-check text-success"></i>
             Materiais com conteudo e prova com gabarito completo.</p>
         </div>`}

    <h3 class="section-title mt-lg">Materiais (${c.materiais.length})</h3>
    ${materiais}

    <h3 class="section-title mt-lg">Prova</h3>
    ${prova}

    ${acoes}`;
}

/** Rejeicao a partir da tela de revisao: o motivo volta ao tutor como chamado. */
async function rejeitarDaRevisao(botao) {
  if (!CURSO_EM_REVISAO) return;
  fecharModal('modalRejeitarRevisao');
  await rejeitarCurso(CURSO_EM_REVISAO.idCurso, botao);
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
  preencherIniciais();
  preencherSeloAprovacoes();
  preencherSeloDuvidas();
  preencherVersao();
  avisarApiIndisponivel();

  if (typeof onPageLoad === 'function') onPageLoad();
});
