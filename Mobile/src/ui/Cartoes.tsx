import type { ReactNode } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';

import { cores, espaco, fonte, raio, tons, type Tom } from '@/tema/tokens';
import { Icone, Texto } from './base';
import type { NomeIcone } from './icones';

/* ==========================================================================
   Cartões compostos — .course-card e os KPIs dos painéis
   ========================================================================== */

/** Capa com gradiente (.course-card__cover). */
export function CartaoCurso({ titulo, rotulo, gradiente, icone, children, onPress, iconeRotulo }: {
  titulo: string;
  rotulo: string;
  gradiente: [string, string];
  icone: NomeIcone;
  iconeRotulo?: NomeIcone;
  children?: ReactNode;
  onPress?: () => void;
}) {
  const corpo = (
    <>
      <LinearGradient colors={gradiente} start={{ x: 0, y: 0 }} end={{ x: 1, y: 1 }} style={st.capa}>
        <View style={st.capaIcone}><Icone nome={icone} tamanho={32} cor="rgba(255,255,255,0.3)" /></View>
        <View style={{ flexDirection: 'row', alignItems: 'center', gap: 4 }}>
          {iconeRotulo && <Icone nome={iconeRotulo} tamanho={12} cor="rgba(255,255,255,0.85)" />}
          <Text style={st.capaRotulo}>{rotulo.toUpperCase()}</Text>
        </View>
        <Text style={st.capaNome} numberOfLines={2}>{titulo}</Text>
      </LinearGradient>
      {children ? <View style={st.corpo}>{children}</View> : null}
    </>
  );
  return onPress ? (
    <Pressable onPress={onPress} accessibilityRole="button" accessibilityLabel={titulo}
      style={({ pressed }) => [st.cartao, pressed && { opacity: 0.9 }]}>{corpo}</Pressable>
  ) : <View style={st.cartao}>{corpo}</View>;
}

/** Linha de metadados com ícone (.course-card__meta). */
export function Meta({ itens }: { itens: { icone: NomeIcone; texto: string }[] }) {
  return (
    <View style={st.meta}>
      {itens.map((i, k) => (
        <View key={k} style={st.metaItem}>
          <Icone nome={i.icone} tamanho={14} cor={cores.textoApagado} />
          <Text style={st.metaTexto}>{i.texto}</Text>
        </View>
      ))}
    </View>
  );
}

/** Indicador numérico dos painéis (KPI com borda superior colorida). */
export function Kpi({ rotulo, valor, detalhe, tom = 'primaria', onPress }:
  { rotulo: string; valor: string | number; detalhe?: string; tom?: Tom; onPress?: () => void }) {
  const conteudo = (
    <>
      <Texto v="rotulo" numberOfLines={1}>{rotulo.toUpperCase()}</Texto>
      <Text style={st.kpiValor} numberOfLines={1} adjustsFontSizeToFit>{valor}</Text>
      {!!detalhe && <Texto v="xs" cor={cores.textoApagado} numberOfLines={2}>{detalhe}</Texto>}
    </>
  );
  const estilo = [st.kpi, { borderTopColor: tons[tom].forte }];
  return onPress
    ? <Pressable onPress={onPress} accessibilityRole="button" style={({ pressed }) => [estilo, pressed && { opacity: 0.85 }]}>{conteudo}</Pressable>
    : <View style={estilo}>{conteudo}</View>;
}

const st = StyleSheet.create({
  cartao: {
    backgroundColor: cores.superficie, borderRadius: raio.lg, borderWidth: 1, borderColor: cores.borda,
    overflow: 'hidden', flex: 1
  },
  capa: { height: 140, padding: espaco.lg, justifyContent: 'flex-end' },
  capaIcone: { position: 'absolute', top: espaco.md, right: espaco.md },
  capaRotulo: { fontFamily: fonte.semi, fontSize: 11, letterSpacing: 1, color: 'rgba(255,255,255,0.85)' },
  capaNome: { fontFamily: fonte.negrito, fontSize: 18, color: '#FFFFFF', marginTop: 2 },
  corpo: { padding: espaco.lg, flex: 1 },
  meta: { flexDirection: 'row', flexWrap: 'wrap', gap: espaco.md, marginBottom: espaco.md },
  metaItem: { flexDirection: 'row', alignItems: 'center', gap: 4 },
  metaTexto: { fontFamily: fonte.regular, fontSize: 12, color: cores.textoApagado },
  kpi: {
    flex: 1, backgroundColor: cores.superficie, borderRadius: raio.lg, borderWidth: 1, borderColor: cores.borda,
    borderTopWidth: 3, padding: espaco.lg, minHeight: 110
  },
  kpiValor: { fontFamily: fonte.negrito, fontSize: 30, color: cores.texto, marginVertical: 2 }
});
