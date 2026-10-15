import { ActivityIndicator, Pressable, StyleSheet, Text, View, type StyleProp, type ViewStyle } from 'react-native';

import { cores, espaco, fonte, raio } from '@/tema/tokens';
import { Icone } from './base';
import type { NomeIcone } from './icones';

/* ==========================================================================
   Botão — .btn, .btn--primary/--secondary/--success/--danger/--ghost
   --------------------------------------------------------------------------
   Inclusive a borda TRACEJADA de 1,5 px, que é a assinatura visual dos
   botões do Tech Quest desde o PIM III.

   "carregando" substitui o comBotaoOcupado() do web: enquanto a requisição
   não volta, o botão fica desabilitado e mostra o indicador. Evita o toque
   duplo que, numa prova, registraria duas tentativas.
   ========================================================================== */

export type VarianteBotao = 'primario' | 'secundario' | 'sucesso' | 'perigo' | 'fantasma';
type Tamanho = 'sm' | 'md' | 'lg';

const VARIANTES: Record<VarianteBotao, { fundo: string; texto: string; borda: string }> = {
  primario: { fundo: cores.primaria, texto: '#FFFFFF', borda: cores.primariaEscura },
  secundario: { fundo: cores.superficie, texto: cores.primaria, borda: cores.primaria },
  sucesso: { fundo: cores.sucesso, texto: '#FFFFFF', borda: cores.sucessoEscuro },
  perigo: { fundo: cores.perigo, texto: '#FFFFFF', borda: cores.perigoEscuro },
  fantasma: { fundo: 'transparent', texto: cores.primaria, borda: cores.bordaForte }
};

const TAMANHOS: Record<Tamanho, { py: number; px: number; fs: number; icone: number }> = {
  sm: { py: 10, px: 16, fs: 13, icone: 16 },
  md: { py: 14, px: 24, fs: 15, icone: 18 },
  lg: { py: 16, px: 28, fs: 16, icone: 20 }
};

interface Props {
  texto: string;
  onPress?: () => void;
  variante?: VarianteBotao;
  tamanho?: Tamanho;
  icone?: NomeIcone;
  /** Ícone depois do texto (setas "Próxima →"). */
  iconeDireita?: NomeIcone;
  cheio?: boolean;
  carregando?: boolean;
  textoCarregando?: string;
  desabilitado?: boolean;
  /** Cor do texto/borda sobrescrita (botão "Remover" em vermelho no estilo fantasma). */
  corTexto?: string;
  style?: StyleProp<ViewStyle>;
}

export function Botao({
  texto, onPress, variante = 'primario', tamanho = 'md', icone, iconeDireita, cheio,
  carregando, textoCarregando, desabilitado, corTexto, style
}: Props) {
  const v = VARIANTES[variante];
  const t = TAMANHOS[tamanho];
  const inativo = desabilitado || carregando;
  const corFinal = corTexto ?? v.texto;

  return (
    <Pressable
      onPress={inativo ? undefined : onPress}
      accessibilityRole="button"
      accessibilityState={{ disabled: !!inativo, busy: !!carregando }}
      accessibilityLabel={texto}
      style={({ pressed }) => [
        st.base,
        {
          backgroundColor: v.fundo,
          borderColor: corTexto ?? v.borda,
          paddingVertical: t.py,
          paddingHorizontal: t.px
        },
        cheio && { alignSelf: 'stretch' },
        inativo && { opacity: 0.5 },
        pressed && !inativo && { transform: [{ scale: 0.98 }], opacity: 0.9 },
        style
      ]}
    >
      <View style={st.conteudo}>
        {carregando
          ? <ActivityIndicator size="small" color={corFinal} />
          : icone && <Icone nome={icone} tamanho={t.icone} cor={corFinal} />}
        <Text style={[st.texto, { color: corFinal, fontSize: t.fs }]} numberOfLines={2}>
          {carregando && textoCarregando ? textoCarregando : texto}
        </Text>
        {!carregando && iconeDireita && <Icone nome={iconeDireita} tamanho={t.icone} cor={corFinal} />}
      </View>
    </Pressable>
  );
}

/** Botão só de ícone (lixeira, copiar). */
export function BotaoIcone({ icone, onPress, cor = cores.primaria, rotulo, carregando }:
  { icone: NomeIcone; onPress: () => void; cor?: string; rotulo: string; carregando?: boolean }) {
  return (
    <Pressable
      onPress={carregando ? undefined : onPress}
      accessibilityRole="button"
      accessibilityLabel={rotulo}
      hitSlop={8}
      style={({ pressed }) => [st.icone, { borderColor: cor }, pressed && { opacity: 0.6 }]}
    >
      {carregando ? <ActivityIndicator size="small" color={cor} /> : <Icone nome={icone} tamanho={18} cor={cor} />}
    </Pressable>
  );
}

const st = StyleSheet.create({
  base: {
    borderRadius: raio.md,
    borderWidth: 1.5,
    borderStyle: 'dashed',
    alignItems: 'center',
    justifyContent: 'center',
    minHeight: 44 // alvo de toque mínimo recomendado (Material/WCAG)
  },
  conteudo: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', gap: espaco.sm },
  texto: { fontFamily: fonte.semi, textAlign: 'center', flexShrink: 1 },
  icone: {
    width: 40, height: 40, borderRadius: raio.md, borderWidth: 1.5, borderStyle: 'dashed',
    alignItems: 'center', justifyContent: 'center'
  }
});
