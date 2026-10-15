import { useWindowDimensions } from 'react-native';

import { espaco } from '@/tema/tokens';

/* ==========================================================================
   Responsividade — o equivalente aos @media do style.css
   --------------------------------------------------------------------------
   React Native não tem media query. O que existe é a dimensão da janela,
   que useWindowDimensions entrega e ATUALIZA quando o aparelho gira, quando
   o app entra em tela dividida ou quando um dobrável abre. Os pontos de
   quebra são os mesmos do web, para o app e o site mudarem de arranjo nas
   mesmas larguras:

     < 600 px   celular          1 coluna,  margem 16 (--space-lg)
     >= 600     celular deitado/ 2 colunas, margem 24 (--space-xl)
                tablet pequeno
     >= 768     tablet           menu lateral no lugar da barra inferior
                                 (o mesmo .sidebar-nav do web >= 768)
     >= 1024    tablet grande    3 colunas, margem 32 (--space-2xl)

   Medida em dp (pixel independente de densidade), não em pixel físico: um
   celular de 1080 px de largura com densidade 2.75 tem ~392 dp e cai, com
   razão, na faixa de celular.
   ========================================================================== */

export const QUEBRA = { media: 600, tablet: 768, grande: 1024 } as const;

export function useResponsivo() {
  const { width, height, fontScale } = useWindowDimensions();

  const ehMedio = width >= QUEBRA.media;
  const ehTablet = width >= QUEBRA.tablet;
  const ehGrande = width >= QUEBRA.grande;

  return {
    largura: width,
    altura: height,
    paisagem: width > height,
    ehMedio,
    ehTablet,
    ehGrande,
    /** Colunas para grades de cartões (.courses-grid, .dashboard-grid). */
    colunas: ehGrande ? 3 : ehMedio ? 2 : 1,
    /** Colunas para indicadores pequenos (KPI): 2 no celular, 4 no tablet. */
    colunasKpi: ehTablet ? 4 : 2,
    margem: ehGrande ? espaco.xxl : ehMedio ? espaco.xl : espaco.lg,
    /**
     * Fonte do sistema ampliada (acessibilidade). Os componentes não travam o
     * tamanho do texto — quem aumentou a fonte do Android precisa dela —, mas
     * os cartões de número usam este fator para não estourar a largura.
     */
    escalaFonte: fontScale
  };
}
