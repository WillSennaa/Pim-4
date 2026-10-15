import type { Tom } from '@/tema/tokens';

/**
 * Estados de FluxoPublicacaoCurso (domínio) com o rótulo e a cor da tela —
 * o mesmo mapa ESTADOS de pages/tutor/cursos.html.
 */
export const ESTADOS_CURSO: Record<string, { tom: Tom; rotulo: string }> = {
  publicado: { tom: 'sucesso', rotulo: 'Publicado' },
  pendente: { tom: 'alerta', rotulo: 'Em avaliação' },
  rascunho: { tom: 'neutro', rotulo: 'Rascunho' },
  rejeitado: { tom: 'perigo', rotulo: 'Rejeitado' }
};

export function estadoDoCurso(status: string | null | undefined) {
  return ESTADOS_CURSO[(status || '').toLowerCase()] ?? ESTADOS_CURSO.rascunho;
}
