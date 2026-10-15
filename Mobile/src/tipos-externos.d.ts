/*
 * O pacote @tabler/icons-react-native publica os arquivos de cada ícone em
 * "@tabler/icons-react-native/IconNome", mas o mapa de tipos do package.json
 * aponta para uma pasta que não existe na versão instalada. Esta declaração
 * tipa a importação individual com o mesmo tipo que o pacote exporta.
 */
declare module '@tabler/icons-react-native/*' {
  import type { ForwardRefExoticComponent } from 'react';
  import type { SvgProps } from 'react-native-svg';

  const Icone: ForwardRefExoticComponent<SvgProps & { size?: number | string; stroke?: number | string; color?: string }>;
  export default Icone;
}
