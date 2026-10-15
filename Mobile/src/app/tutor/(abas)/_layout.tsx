import { Tabs } from 'expo-router';

import { useSessao } from '@/sessao/SessaoContext';
import { AcoesTopo, iconeAba, useOpcoesAbas } from '@/ui/Navegacao';

/** Abas do tutor: as quatro do .bottom-nav do web + Conta. */
export default function AbasTutor() {
  const opcoes = useOpcoesAbas();
  const { usuario } = useSessao();
  const acoes = () => (
    <AcoesTopo iniciais={usuario?.iniciais ?? '?'} rotaPerfil="/tutor/conta" rotaNotificacoes="/tutor/notificacoes" />
  );

  return (
    <Tabs screenOptions={{ ...opcoes, headerRight: acoes }}>
      <Tabs.Screen name="index" options={{ title: 'Painel', headerTitle: 'Tech Quest · Tutor', tabBarIcon: iconeAba('layout-dashboard') }} />
      <Tabs.Screen name="alunos" options={{ title: 'Alunos', tabBarIcon: iconeAba('users') }} />
      <Tabs.Screen name="duvidas" options={{ title: 'Dúvidas', tabBarIcon: iconeAba('message-circle') }} />
      <Tabs.Screen name="cursos" options={{ title: 'Cursos', tabBarIcon: iconeAba('book') }} />
      <Tabs.Screen name="conta" options={{ title: 'Conta', tabBarIcon: iconeAba('user') }} />
    </Tabs>
  );
}
