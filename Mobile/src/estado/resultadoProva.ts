import type { ResultadoTentativa } from '@/api/tipos';

/* ==========================================================================
   Resultado da última prova entregue
   --------------------------------------------------------------------------
   No web, o resultado ia da tela da prova para a de resultado pelo
   sessionStorage ('tq_resultado'). Aqui as duas telas vivem no MESMO
   processo JavaScript, então basta guardar em memória.

   Junto com a correção do servidor ficam o enunciado e as alternativas de
   cada questão, para a revisão mostrar o texto de cada letra. O GABARITO vem
   só da resposta da submissão (letraCorreta), nunca do download da prova —
   a prova chega ao app sem a resposta certa.

   ALTERNATIVA REJEITADA: passar o resultado como parâmetro de rota. Rota
   carrega texto curto; a correção com enunciados e códigos não cabe bem ali,
   e ficaria visível em logs de navegação.
   ========================================================================== */

export interface QuestaoRevisao {
  id: number;
  enunciado: string;
  codigo: string | null;
  alternativas: Record<string, string>;
}

export interface ResultadoGuardado {
  resultado: ResultadoTentativa;
  idCurso: number;
  tituloProva: string;
  questoes: QuestaoRevisao[];
}

let ultimo: ResultadoGuardado | null = null;

export const resultadoProva = {
  guardar(r: ResultadoGuardado) { ultimo = r; },
  ler(): ResultadoGuardado | null { return ultimo; }
};
