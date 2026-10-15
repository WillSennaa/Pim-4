/* ==========================================================================
   Tokens visuais — espelho de Web/css/style.css (:root)
   --------------------------------------------------------------------------
   POR QUE COPIAR OS VALORES EM VEZ DE REINVENTAR: o app é a mesma plataforma
   em outro dispositivo. O aluno que usa o site no laboratório e o app no
   ônibus deve reconhecer as mesmas cores, os mesmos raios e o mesmo ritmo de
   espaçamento. Cada constante aqui tem o nome da variável CSS correspondente
   ao lado, para a equivalência ser verificável linha a linha.

   React Native não tem CSS nem cascata: o estilo é um objeto JavaScript. Por
   isso as variáveis viram constantes exportadas, importadas por cada
   componente.
   ========================================================================== */

export const cores = {
  primaria: '#2563EB',          // --color-primary
  primariaEscura: '#1E40AF',    // --color-primary-dark
  primariaClara: '#DBEAFE',     // --color-primary-light
  primariaMaisClara: '#EFF6FF', // --color-primary-lighter

  sucesso: '#22C55E',           // --color-success
  sucessoClaro: '#DCFCE7',      // --color-success-light
  sucessoEscuro: '#15803D',     // --color-success-dark

  perigo: '#EF4444',            // --color-danger
  perigoClaro: '#FEE2E2',       // --color-danger-light
  perigoEscuro: '#B91C1C',      // --color-danger-dark

  alerta: '#F59E0B',            // --color-warning
  alertaClaro: '#FEF3C7',       // --color-warning-light
  alertaEscuro: '#B45309',      // --color-warning-dark

  fundo: '#F1F5F9',             // --color-bg
  superficie: '#FFFFFF',        // --color-surface
  superficie2: '#F8FAFC',       // --color-surface-2
  texto: '#0F172A',             // --color-text
  textoSecundario: '#475569',   // --color-text-secondary
  textoApagado: '#94A3B8',      // --color-text-muted
  borda: '#E2E8F0',             // --color-border
  bordaForte: '#CBD5E1',        // --color-border-strong

  // Bloco de código (.code-block) e capa roxa (.course-card__cover--bd)
  codigoFundo: '#1E293B',       // .code-block background
  codigoTexto: '#E2E8F0',
  codigoPalavra: '#818CF8',     // .code-block .keyword
  codigoString: '#FCA5A5',      // .code-block .string
  codigoComentario: '#64748B',  // .code-block .comment
  codigoNumero: '#FBBF24',      // .code-block .number
  roxo: '#8B5CF6',
  roxoEscuro: '#6D28D9',
  ouroBorda: '#B45309',
  ouroFundo1: '#FEFCE8',
  ouroFundo2: '#FEF3C7'
} as const;

export const espaco = {
  xs: 4,    // --space-xs
  sm: 8,    // --space-sm
  md: 12,   // --space-md
  lg: 16,   // --space-lg
  xl: 24,   // --space-xl
  xxl: 32,  // --space-2xl
  xxxl: 48  // --space-3xl
} as const;

export const raio = {
  sm: 6,      // --radius-sm
  md: 10,     // --radius-md
  lg: 16,     // --radius-lg
  xl: 20,     // --radius-xl
  total: 9999 // --radius-full
} as const;

/**
 * Famílias carregadas em app/_layout.tsx. No web o navegador sintetiza o
 * negrito a partir de uma família só; no Android cada peso é um arquivo de
 * fonte, então o peso escolhe a família, e não a propriedade fontWeight.
 */
export const fonte = {
  regular: 'Inter_400Regular',
  media: 'Inter_500Medium',
  semi: 'Inter_600SemiBold',
  negrito: 'Inter_700Bold',
  mono: 'JetBrainsMono_400Regular'
} as const;

/** Sombras de --shadow-sm/md: elevation no Android, shadow* no iOS/web. */
export const sombra = {
  sm: {
    shadowColor: '#0F172A', shadowOpacity: 0.05, shadowRadius: 2,
    shadowOffset: { width: 0, height: 1 }, elevation: 1
  },
  md: {
    shadowColor: '#0F172A', shadowOpacity: 0.08, shadowRadius: 12,
    shadowOffset: { width: 0, height: 4 }, elevation: 3
  }
} as const;

/** Cores de estado usadas por Badge, ícones de lista e bordas laterais. */
export type Tom = 'primaria' | 'sucesso' | 'perigo' | 'alerta' | 'neutro';

export const tons: Record<Tom, { forte: string; claro: string; escuro: string }> = {
  primaria: { forte: cores.primaria, claro: cores.primariaClara, escuro: cores.primariaEscura },
  sucesso: { forte: cores.sucesso, claro: cores.sucessoClaro, escuro: cores.sucessoEscuro },
  perigo: { forte: cores.perigo, claro: cores.perigoClaro, escuro: cores.perigoEscuro },
  alerta: { forte: cores.alerta, claro: cores.alertaClaro, escuro: cores.alertaEscuro },
  neutro: { forte: cores.textoApagado, claro: cores.superficie2, escuro: cores.textoSecundario }
};
