import { Pressable, ScrollView, StyleSheet, Text, View } from 'react-native';

import { cores, espaco, fonte, raio, sombra } from '@/tema/tokens';

/* ==========================================================================
   Abas de filtro — .tabs (segmentado) e .tabs--pills (pílulas roláveis)
   --------------------------------------------------------------------------
   Filtram a lista JÁ carregada, sem nova ida ao servidor — a mesma decisão
   do web (filtrarDuvidas, filtrarMedalhas, filtrarCertificados).
   ========================================================================== */

interface Props<T extends string> {
  abas: { valor: T; texto: string }[];
  valor: T;
  onChange: (v: T) => void;
  pilulas?: boolean;
}

export function Abas<T extends string>({ abas, valor, onChange, pilulas }: Props<T>) {
  if (pilulas) {
    return (
      <ScrollView horizontal showsHorizontalScrollIndicator={false} style={{ marginBottom: espaco.lg }}
        contentContainerStyle={{ gap: espaco.sm }}>
        {abas.map(a => {
          const ativa = a.valor === valor;
          return (
            <Pressable key={a.valor} onPress={() => onChange(a.valor)} accessibilityRole="tab"
              accessibilityState={{ selected: ativa }}
              style={[st.pilula, ativa && { backgroundColor: cores.primaria, borderColor: cores.primaria }]}>
              <Text style={[st.texto, ativa && { color: '#FFFFFF' }]}>{a.texto}</Text>
            </Pressable>
          );
        })}
      </ScrollView>
    );
  }

  return (
    <View style={st.trilho} accessibilityRole="tablist">
      {abas.map(a => {
        const ativa = a.valor === valor;
        return (
          <Pressable key={a.valor} onPress={() => onChange(a.valor)} accessibilityRole="tab"
            accessibilityState={{ selected: ativa }} style={[st.aba, ativa && st.abaAtiva]}>
            <Text style={[st.texto, ativa && { color: cores.texto }]} numberOfLines={1}>{a.texto}</Text>
          </Pressable>
        );
      })}
    </View>
  );
}

const st = StyleSheet.create({
  trilho: {
    flexDirection: 'row', backgroundColor: cores.borda, borderRadius: raio.md,
    padding: 4, gap: 4, marginBottom: espaco.lg
  },
  aba: { flex: 1, paddingVertical: 10, paddingHorizontal: 4, borderRadius: raio.sm, alignItems: 'center' },
  abaAtiva: { backgroundColor: cores.superficie, ...sombra.sm },
  texto: { fontFamily: fonte.media, fontSize: 13, color: cores.textoSecundario },
  pilula: {
    paddingVertical: 8, paddingHorizontal: 16, borderRadius: raio.total,
    backgroundColor: cores.superficie, borderWidth: 1, borderColor: cores.borda
  }
});
