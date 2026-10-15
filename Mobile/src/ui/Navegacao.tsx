import { Pressable, StyleSheet, Text, View, type ColorValue } from 'react-native';
import { router } from 'expo-router';

import { useResponsivo } from '@/hooks/useResponsivo';
import { cores, fonte, raio } from '@/tema/tokens';
import { Icone } from './base';
import type { NomeIcone } from './icones';

/* ==========================================================================
   Peças de navegação compartilhadas por aluno e tutor
   --------------------------------------------------------------------------
   O web troca a barra inferior (.bottom-nav) pelo menu lateral
   (.sidebar-nav) a partir de 768 px. As abas do app fazem o mesmo: no
   celular ficam embaixo, ao alcance do polegar; no tablet viram uma coluna à
   esquerda, que aproveita a largura e deixa a altura para o conteúdo.
   ========================================================================== */

export const opcoesCabecalho = {
  headerStyle: { backgroundColor: cores.superficie },
  headerTintColor: cores.primaria,
  headerTitleStyle: { fontFamily: fonte.semi, fontSize: 16, color: cores.texto },
  headerTitleAlign: 'center' as const,
  contentStyle: { backgroundColor: cores.fundo },
  headerBackButtonDisplayMode: 'minimal' as const
};

export function useOpcoesAbas() {
  const { ehTablet } = useResponsivo();
  return {
    tabBarPosition: ehTablet ? ('left' as const) : ('bottom' as const),
    tabBarVariant: ehTablet ? ('material' as const) : ('uikit' as const),
    tabBarActiveTintColor: cores.primaria,
    tabBarInactiveTintColor: cores.textoApagado,
    tabBarLabelStyle: { fontFamily: fonte.media, fontSize: 11 },
    tabBarStyle: ehTablet
      ? { backgroundColor: cores.superficie, borderRightColor: cores.borda, minWidth: 96 }
      : { backgroundColor: cores.superficie, borderTopColor: cores.borda },
    tabBarActiveBackgroundColor: ehTablet ? cores.primariaMaisClara : undefined,
    headerStyle: { backgroundColor: cores.superficie },
    headerTitleStyle: { fontFamily: fonte.negrito, fontSize: 18, color: cores.primaria },
    headerTitleAlign: 'left' as const,
    sceneStyle: { backgroundColor: cores.fundo }
  };
}

export function iconeAba(nome: NomeIcone) {
  // eslint-disable-next-line react/display-name
  return ({ color, focused }: { color: ColorValue; focused: boolean }) =>
    <Icone nome={nome} tamanho={24} cor={String(color)} traco={focused ? 2.2 : 1.8} />;
}

/** Ações do .top-bar: sino de notificações e avatar com as iniciais. */
export function AcoesTopo({ iniciais, rotaPerfil, rotaNotificacoes }:
  { iniciais: string; rotaPerfil: string; rotaNotificacoes?: string }) {
  return (
    <View style={st.acoes}>
      {rotaNotificacoes && (
        <Pressable onPress={() => router.push(rotaNotificacoes as never)} hitSlop={8}
          accessibilityRole="button" accessibilityLabel="Notificações">
          <Icone nome="bell" tamanho={22} cor={cores.textoSecundario} />
        </Pressable>
      )}
      <Pressable onPress={() => router.navigate(rotaPerfil as never)} accessibilityRole="button" accessibilityLabel="Perfil"
        style={st.avatar}>
        <Text style={st.avatarTexto}>{iniciais}</Text>
      </Pressable>
    </View>
  );
}

const st = StyleSheet.create({
  acoes: { flexDirection: 'row', alignItems: 'center', gap: 14, marginRight: 16 },
  avatar: {
    width: 36, height: 36, borderRadius: raio.total, backgroundColor: cores.primaria,
    alignItems: 'center', justifyContent: 'center'
  },
  avatarTexto: { fontFamily: fonte.semi, fontSize: 13, color: '#FFFFFF' }
});
