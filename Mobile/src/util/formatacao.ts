/* ==========================================================================
   Formatação — porte de FMT (Web/js/app.js)
   --------------------------------------------------------------------------
   A API devolve data em ISO, como deve ser: formatar é trabalho do cliente,
   que conhece o fuso e o idioma de quem está olhando. As regras são as
   mesmas do web para as duas telas dizerem a mesma coisa do mesmo dado.

   Por que não toLocaleDateString('pt-BR'): o motor JavaScript do Android
   (Hermes) pode vir sem os dados de idioma completos, e a mesma chamada que
   no navegador devolve "08/10/2026" no aparelho pode devolver "10/8/2026".
   Montar o texto à mão elimina a dependência.
   ========================================================================== */

const MESES = ['janeiro', 'fevereiro', 'março', 'abril', 'maio', 'junho', 'julho',
  'agosto', 'setembro', 'outubro', 'novembro', 'dezembro'];
const DIAS = ['domingo', 'segunda-feira', 'terça-feira', 'quarta-feira', 'quinta-feira',
  'sexta-feira', 'sábado'];

const dois = (n: number) => String(n).padStart(2, '0');

/** "2026-10-08" (DateOnly do C#) vira data LOCAL, não meia-noite UTC. */
function paraData(valor: string | Date | null | undefined): Date | null {
  if (!valor) return null;
  if (valor instanceof Date) return isNaN(valor.getTime()) ? null : valor;
  const soData = /^(\d{4})-(\d{2})-(\d{2})$/.exec(valor);
  const d = soData ? new Date(+soData[1], +soData[2] - 1, +soData[3]) : new Date(valor);
  return isNaN(d.getTime()) ? null : d;
}

export const FMT = {
  data(valor: string | Date | null | undefined): string {
    const d = paraData(valor);
    return d ? `${dois(d.getDate())}/${dois(d.getMonth() + 1)}/${d.getFullYear()}` : '';
  },

  /** "Quinta-feira, 9 de outubro de 2026" — relógio de quem olha, não do banco. */
  dataLonga(valor?: string | Date): string {
    const d = paraData(valor ?? new Date());
    if (!d) return '';
    const t = `${DIAS[d.getDay()]}, ${d.getDate()} de ${MESES[d.getMonth()]} de ${d.getFullYear()}`;
    return t.charAt(0).toUpperCase() + t.slice(1);
  },

  dataHora(valor: string | null | undefined): string {
    const d = paraData(valor);
    return d ? `${FMT.data(d)} ${dois(d.getHours())}:${dois(d.getMinutes())}` : '';
  },

  relativo(valor: string | null | undefined): string {
    const d = paraData(valor);
    if (!d) return '';
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
    return FMT.data(d);
  },

  iniciais(nome: string | null | undefined): string {
    if (!nome) return '??';
    const p = String(nome).trim().split(/\s+/);
    if (p.length === 1) return p[0].charAt(0).toUpperCase();
    return (p[0].charAt(0) + p[p.length - 1].charAt(0)).toUpperCase();
  },

  primeiroNome(nome: string | null | undefined): string {
    return (nome || '').trim().split(/\s+/)[0] || '';
  },

  /** 1234 -> "1.234" sem depender de Intl. */
  milhar(n: number | null | undefined): string {
    return String(Math.round(n ?? 0)).replace(/\B(?=(\d{3})+(?!\d))/g, '.');
  },

  /** 7.5 -> "7,5" */
  nota(n: number | null | undefined, casas = 1): string {
    return n == null ? '-' : n.toFixed(casas).replace('.', ',');
  },

  /** "dd/mm/aaaa" digitado -> "aaaa-mm-dd" que o DateOnly do C# aceita. */
  dataBrParaIso(texto: string): string | null | 'invalida' {
    const t = texto.trim();
    if (!t) return null;
    const br = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(t);
    if (!br) return 'invalida';
    const d = new Date(+br[3], +br[2] - 1, +br[1]);
    if (d.getDate() !== +br[1] || d.getMonth() !== +br[2] - 1) return 'invalida';
    return `${br[3]}-${br[2]}-${br[1]}`;
  },

  /** Plural simples: plural(1,'questão','questões'). */
  plural(n: number, um: string, varios: string): string {
    return `${n} ${n === 1 ? um : varios}`;
  }
};

/** Remove acentos e caixa para busca local ("Lógica" casa com "logica"). */
export function normalizar(texto: string | null | undefined): string {
  return String(texto || '').toLowerCase().normalize('NFD').replace(/[̀-ͯ]/g, '');
}
