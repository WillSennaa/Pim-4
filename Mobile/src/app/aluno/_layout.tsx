import { Stack } from 'expo-router';

import { opcoesCabecalho } from '@/ui/Navegacao';

/* ==========================================================================
   Área do estudante — equivalente a pages/estudante/
   --------------------------------------------------------------------------
   Pilha de telas: as cinco abas ficam na base (grupo "(abas)") e as telas de
   detalhe empilham por cima com botão de voltar. A mesma organização do web
   (barra inferior com Home, Cursos, Conquistas, Ajuda, Perfil + páginas
   internas), só que com o voltar nativo do Android.
   ========================================================================== */

export default function LayoutAluno() {
  return (
    <Stack screenOptions={opcoesCabecalho}>
      <Stack.Screen name="(abas)" options={{ headerShown: false }} />
      <Stack.Screen name="curso/[id]" options={{ title: 'Curso' }} />
      <Stack.Screen name="aula" options={{ title: 'Material' }} />
      <Stack.Screen name="prova/[id]" options={{ title: 'Prova' }} />
      {/* Sem voltar: a prova já foi entregue, voltar para ela não faz sentido. */}
      <Stack.Screen name="resultado" options={{ title: 'Resultado', headerBackVisible: false, gestureEnabled: false }} />
      <Stack.Screen name="certificados" options={{ title: 'Meus certificados' }} />
      <Stack.Screen name="certificado/[codigo]" options={{ title: 'Certificado' }} />
      <Stack.Screen name="minhas-provas" options={{ title: 'Minhas provas' }} />
      <Stack.Screen name="notificacoes" options={{ title: 'Notificações' }} />
      <Stack.Screen name="suporte" options={{ title: 'Suporte' }} />
      <Stack.Screen name="editar-perfil" options={{ title: 'Editar perfil' }} />
    </Stack>
  );
}
