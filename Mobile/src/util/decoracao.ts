import type { NomeIcone } from '@/ui/icones';

/* ==========================================================================
   Decoração — porte de DECORACAO (Web/js/app.js)
   --------------------------------------------------------------------------
   Ícone e cor nunca foram dados de negócio: são apresentação. O banco não tem
   coluna de ícone em Curso nem em Medalha, e não deve ter. No web a
   decoração vinha do mock.json casada pelo título; aqui ela é deduzida da
   categoria e do nome, para que um curso NOVO criado pelo tutor também
   receba um visual coerente sem ninguém editar o app.
   ========================================================================== */

export interface Capa { gradiente: [string, string]; icone: NomeIcone; }

const CAPAS: Record<'primaria' | 'bd' | 'web' | 'dados', Capa> = {
  primaria: { gradiente: ['#2563EB', '#1E40AF'], icone: 'code' },     // .course-card__cover
  bd: { gradiente: ['#8B5CF6', '#6D28D9'], icone: 'database' },       // --bd
  web: { gradiente: ['#22C55E', '#15803D'], icone: 'code' },          // --web
  dados: { gradiente: ['#F59E0B', '#B45309'], icone: 'chart-bar' }    // --data
};

export function capaDoCurso(nome?: string | null, categoria?: string | null): Capa {
  const t = `${nome || ''} ${categoria || ''}`.toLowerCase();
  if (/banco|sql|dados|modelagem|database/.test(t)) return CAPAS.bd;
  if (/web|html|css|front|javascript|react/.test(t)) return CAPAS.web;
  if (/an[aá]lise|estat|data|bi\b/.test(t)) return CAPAS.dados;
  if (/c#|programa|l[oó]gica|java|python|poo|algoritmo/.test(t)) return CAPAS.primaria;
  return { gradiente: CAPAS.primaria.gradiente, icone: 'book-2' };
}

/** Mesmos ícones do mock.json > medalhas, casados pelo nome da medalha. */
const ICONE_MEDALHA: Record<string, NomeIcone> = {
  'primeiro passo': 'target-arrow',
  'mestre em arrays': 'medal',
  maratonista: 'flame',
  'estudante dedicado': 'star',
  'pontuação perfeita': 'trophy',
  'pontuacao perfeita': 'trophy',
  'mestre em c#': 'code',
  'poo master': 'affiliate',
  'quiz champion': 'help-square',
  'bibliotecário': 'book',
  bibliotecario: 'book',
  'arquiteto solid': 'building',
  'dba iniciante': 'database',
  'lenda da tech quest': 'crown'
};

export function iconeDaMedalha(nome?: string | null): NomeIcone {
  return ICONE_MEDALHA[(nome || '').trim().toLowerCase()] ?? 'medal';
}

/** XP concedido por medalha (RegrasGamificacao), o mesmo 75 que o web exibe. */
export const XP_POR_MEDALHA = 75;
