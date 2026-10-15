import { Children, type PropsWithChildren, type ReactNode } from 'react';
import {
  KeyboardAvoidingView, Platform, RefreshControl, ScrollView, StyleSheet, View,
  type StyleProp, type ViewStyle
} from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

import { useResponsivo } from '@/hooks/useResponsivo';
import { cores, espaco } from '@/tema/tokens';
import { Texto } from './base';

/* ==========================================================================
   Tela — o .main-content do web
   --------------------------------------------------------------------------
   Toda tela rola, puxa para atualizar e tem margem lateral que acompanha a
   largura do aparelho. Em tablet o conteúdo não se estica de borda a borda:
   ele é limitado e centralizado, como o .app-container--wide (1200 px) e o
   .content-narrow (600 px) do web. Linha de texto com 1.200 dp de largura é
   cansativa de ler, e formulário largo demais parece quebrado.
   ========================================================================== */

interface Props extends PropsWithChildren {
  /** Puxar para atualizar. */
  aoAtualizar?: () => void;
  atualizando?: boolean;
  /** 'leitura' = 720 (aula, prova), 'formulario' = 600, 'painel' = 1200. */
  largura?: 'leitura' | 'formulario' | 'painel';
  /** Conteúdo fixo abaixo da rolagem (ex.: navegação da prova). */
  rodape?: ReactNode;
  semRolagem?: boolean;
  estiloConteudo?: StyleProp<ViewStyle>;
}

const LARGURAS = { leitura: 720, formulario: 600, painel: 1200 } as const;

export function Tela({ children, aoAtualizar, atualizando = false, largura = 'painel', rodape, semRolagem, estiloConteudo }: Props) {
  const { margem } = useResponsivo();
  const insets = useSafeAreaInsets();

  const conteudo = (
    <View style={[st.limite, { maxWidth: LARGURAS[largura] }, estiloConteudo]}>{children}</View>
  );

  return (
    <KeyboardAvoidingView style={st.raiz} behavior={Platform.OS === 'ios' ? 'padding' : undefined}>
      {semRolagem ? (
        <View style={[st.raiz, { paddingHorizontal: margem, paddingTop: espaco.lg }]}>{conteudo}</View>
      ) : (
        <ScrollView
          style={st.raiz}
          contentContainerStyle={{ paddingHorizontal: margem, paddingTop: espaco.lg, paddingBottom: espaco.xxl + (rodape ? 0 : insets.bottom) }}
          keyboardShouldPersistTaps="handled"
          refreshControl={aoAtualizar
            ? <RefreshControl refreshing={atualizando} onRefresh={aoAtualizar} colors={[cores.primaria]} tintColor={cores.primaria} />
            : undefined}
        >
          {conteudo}
        </ScrollView>
      )}
      {rodape && (
        <View style={[st.rodape, { paddingHorizontal: margem, paddingBottom: espaco.md + insets.bottom }]}>
          <View style={[st.limite, { maxWidth: LARGURAS[largura] }]}>{rodape}</View>
        </View>
      )}
    </KeyboardAvoidingView>
  );
}

/** Título + subtítulo de página (.page-title / .page-subtitle). */
export function Cabecalho({ titulo, subtitulo, direita }: { titulo: string; subtitulo?: string; direita?: ReactNode }) {
  return (
    <View style={st.cabecalho}>
      <View style={{ flex: 1 }}>
        <Texto v="titulo">{titulo}</Texto>
        {!!subtitulo && <Texto v="subtitulo" style={{ marginTop: 4 }}>{subtitulo}</Texto>}
      </View>
      {direita}
    </View>
  );
}

/** Título de seção (.section-title). */
export function Secao({ titulo, direita }: { titulo: string; direita?: ReactNode }) {
  return (
    <View style={st.secao}>
      <Texto v="secao" style={{ flex: 1 }}>{titulo}</Texto>
      {direita}
    </View>
  );
}

/* --------------------------------------------------------------------------
   Grade responsiva — .courses-grid / .dashboard-grid
   Distribui os filhos em N colunas de mesma largura. No celular, uma coluna;
   no tablet, duas ou três. Linhas incompletas recebem células vazias para
   o último cartão não esticar até a largura toda.
   -------------------------------------------------------------------------- */
export function Grade({ children, colunas, gap = espaco.md }: PropsWithChildren<{ colunas?: number; gap?: number }>) {
  const { colunas: padrao } = useResponsivo();
  const n = Math.max(1, colunas ?? padrao);
  const itens = Children.toArray(children).filter(Boolean);

  if (n === 1) return <View style={{ gap }}>{itens}</View>;

  const linhas: ReactNode[][] = [];
  for (let i = 0; i < itens.length; i += n) linhas.push(itens.slice(i, i + n));

  return (
    <View style={{ gap }}>
      {linhas.map((linha, i) => (
        <View key={i} style={{ flexDirection: 'row', gap, alignItems: 'stretch' }}>
          {linha.map((item, j) => <View key={j} style={{ flex: 1, minWidth: 0 }}>{item}</View>)}
          {Array.from({ length: n - linha.length }).map((_, k) => <View key={'v' + k} style={{ flex: 1 }} />)}
        </View>
      ))}
    </View>
  );
}

const st = StyleSheet.create({
  raiz: { flex: 1, backgroundColor: cores.fundo },
  limite: { width: '100%', alignSelf: 'center' },
  rodape: {
    backgroundColor: cores.superficie, borderTopWidth: 1, borderTopColor: cores.borda, paddingTop: espaco.md
  },
  cabecalho: { flexDirection: 'row', alignItems: 'flex-start', gap: espaco.md, marginBottom: espaco.lg },
  secao: { flexDirection: 'row', alignItems: 'center', marginTop: espaco.lg, marginBottom: espaco.md, gap: espaco.sm }
});
