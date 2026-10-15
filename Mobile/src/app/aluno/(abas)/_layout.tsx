import { Tabs } from 'expo-router';

import { useSessao } from '@/sessao/SessaoContext';
import { AcoesTopo, iconeAba, useOpcoesAbas } from '@/ui/Navegacao';

/** As cinco abas do .bottom-nav do estudante, na mesma ordem do web. */
export default function AbasAluno() {
  const opcoes = useOpcoesAbas();
  const { usuario } = useSessao();
  const acoes = () => (
    <AcoesTopo iniciais={usuario?.iniciais ?? '?'} rotaPerfil="/aluno/perfil" rotaNotificacoes="/aluno/notificacoes" />
  );

  return (
    <Tabs screenOptions={{ ...opcoes, headerRight: acoes }}>
      <Tabs.Screen name="index" options={{ title: 'Home', headerTitle: 'Tech Quest', tabBarIcon: iconeAba('home') }} />
      <Tabs.Screen name="cursos" options={{ title: 'Cursos', tabBarIcon: iconeAba('book-2') }} />
      <Tabs.Screen name="conquistas" options={{ title: 'Conquistas', tabBarIcon: iconeAba('trophy') }} />
      <Tabs.Screen name="ajuda" options={{ title: 'Ajuda', tabBarIcon: iconeAba('help-circle') }} />
      <Tabs.Screen name="perfil" options={{ title: 'Perfil', tabBarIcon: iconeAba('user') }} />
    </Tabs>
  );
}
